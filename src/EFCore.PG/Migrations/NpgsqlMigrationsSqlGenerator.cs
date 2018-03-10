#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class NpgsqlMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        public NpgsqlMigrationsSqlGenerator([NotNull] MigrationsSqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override void Generate(MigrationOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation is NpgsqlCreateDatabaseOperation createDatabaseOperation)
            {
                Generate(createDatabaseOperation, model, builder);
                return;
            }

            if (operation is NpgsqlDropDatabaseOperation dropDatabaseOperation)
            {
                Generate(dropDatabaseOperation, model, builder);
                return;
            }

            base.Generate(operation, model, builder);
        }

        #region Standard migrations

        protected override void Generate(
            CreateTableOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            // Filter out any system columns
            if (operation.Columns.Any(c => IsSystemColumn(c.Name)))
            {
                var filteredOperation = new CreateTableOperation
                {
                    Name = operation.Name,
                    Schema = operation.Schema,
                    PrimaryKey = operation.PrimaryKey,
                };
                filteredOperation.Columns.AddRange(operation.Columns.Where(c => !_systemColumnNames.Contains(c.Name)));
                filteredOperation.ForeignKeys.AddRange(operation.ForeignKeys);
                filteredOperation.UniqueConstraints.AddRange(operation.UniqueConstraints);
                operation = filteredOperation;
            }

            base.Generate(operation, model, builder, false);

            // CockroachDB "interleave in parent" (https://www.cockroachlabs.com/docs/stable/interleave-in-parent.html)
            if (operation[CockroachDbAnnotationNames.InterleaveInParent] is string)
            {
                var interleaveInParent = new CockroachDbInterleaveInParent(operation);
                var parentTableSchema = interleaveInParent.ParentTableSchema;
                var parentTableName = interleaveInParent.ParentTableName;
                var interleavePrefix = interleaveInParent.InterleavePrefix;

                builder
                    .AppendLine()
                    .Append("INTERLEAVE IN PARENT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(parentTableName, parentTableSchema))
                    .Append(" (")
                    .Append(string.Join(", ", interleavePrefix.Select(c => Dependencies.SqlGenerationHelper.DelimitIdentifier(c))))
                    .Append(')');
            }

            var storageParameters = GetStorageParameters(operation);
            if (storageParameters.Count > 0)
            {
                builder
                    .AppendLine()
                    .Append("WITH (")
                    .Append(string.Join(", ", storageParameters.Select(p => $"{p.Key}={p.Value}")))
                    .Append(')');
            }

            // Comment on the table
            var comment = operation[NpgsqlAnnotationNames.Comment] as string;
            if (comment != null)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

                builder
                    .Append("COMMENT ON TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                    .Append(" IS ")
                    .Append(stringTypeMapping.GenerateSqlLiteral(comment));
            }

            // Comments on the columns
            foreach (var columnOp in operation.Columns.Where(c => c[NpgsqlAnnotationNames.Comment] != null))
            {
                var columnComment = columnOp[NpgsqlAnnotationNames.Comment];
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

                builder
                    .Append("COMMENT ON COLUMN ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                    .Append('.')
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnOp.Name))
                    .Append(" IS ")
                    .Append(stringTypeMapping.GenerateSqlLiteral(columnComment));
            }

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected override void Generate(AlterTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            var madeChanges = false;

            // Storage parameters
            var oldStorageParameters = GetStorageParameters(operation.OldTable);
            var newStorageParameters = GetStorageParameters(operation);

            var newOrChanged = newStorageParameters.Where(p =>
                    !oldStorageParameters.ContainsKey(p.Key) ||
                    oldStorageParameters[p.Key] != p.Value
            ).ToList();

            if (newOrChanged.Count > 0)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

                builder
                    .Append(" SET (")
                    .Append(string.Join(", ", newOrChanged.Select(p => $"{p.Key}={p.Value}")))
                    .Append(")");

                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                madeChanges = true;
            }

            var removed = oldStorageParameters
                .Select(p => p.Key)
                .Where(pn => !newStorageParameters.ContainsKey(pn))
                .ToList();

            if (removed.Count > 0)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

                builder
                    .Append(" RESET (")
                    .Append(string.Join(", ", removed))
                    .Append(")");

                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                madeChanges = true;
            }

            // Comment
            var oldComment = operation.OldTable[NpgsqlAnnotationNames.Comment] as string;
            var newComment = operation[NpgsqlAnnotationNames.Comment] as string;

            if (oldComment != newComment)
            {
                var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

                builder
                    .Append("COMMENT ON TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                    .Append(" IS ")
                    .Append(stringTypeMapping.GenerateSqlLiteral(newComment));

                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                madeChanges = true;
            }

            if (madeChanges)
                EndStatement(builder);
        }

        protected override void Generate(
            DropColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            // Never touch system columns
            if (IsSystemColumn(operation.Name))
                return;

            base.Generate(operation, model, builder, terminate);
        }

        protected override void Generate(
            AddColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            // Never touch system columns
            if (IsSystemColumn(operation.Name))
                return;

            base.Generate(operation, model, builder, terminate: false);

            var comment = operation[NpgsqlAnnotationNames.Comment] as string;
            if (comment != null)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

                builder
                    .Append("COMMENT ON COLUMN ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append('.')
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" IS ")
                    .Append(stringTypeMapping.GenerateSqlLiteral(comment));
            }

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected override void Generate(AlterColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            // Never touch system columns
            if (IsSystemColumn(operation.Name))
                return;

            var type = operation.ColumnType ?? GetColumnType(operation.Schema, operation.Table, operation.Name, operation.ClrType, null, operation.MaxLength, false, model);

            string newSequenceName = null;
            var defaultValueSql = operation.DefaultValueSql;

            var table = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema);
            var column = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name);
            var alterBase = $"ALTER TABLE {table} ALTER COLUMN {column} ";

            CheckForOldValueGenerationAnnotation(operation);

            var oldStrategy = operation.OldColumn[NpgsqlAnnotationNames.ValueGenerationStrategy] as NpgsqlValueGenerationStrategy?;
            var newStrategy = operation[NpgsqlAnnotationNames.ValueGenerationStrategy] as NpgsqlValueGenerationStrategy?;

            if (oldStrategy != newStrategy)
            {
                // We have a value generation strategy change

                if (oldStrategy == NpgsqlValueGenerationStrategy.SerialColumn)
                {
                    // TODO: It would be better to actually select for the owned sequence.
                    // This would require plpgsql.
                    var sequenceName = Dependencies.SqlGenerationHelper.DelimitIdentifier($"{operation.Table}_{operation.Name}_seq");
                    switch (newStrategy)
                    {
                    case null:
                        // Drop the serial, converting the column to a regular int
                        builder.AppendLine($"DROP SEQUENCE {sequenceName} CASCADE;");
                        break;
                    case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                    case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                        // Convert serial column to identity, maintaining the current sequence value
                        var identityTypeClause = newStrategy == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                            ? "ALWAYS"
                            : "BY DEFAULT";
                        var oldSequenceName = Dependencies.SqlGenerationHelper.DelimitIdentifier($"{operation.Table}_{operation.Name}_old_seq");
                        builder
                            .AppendLine($"ALTER SEQUENCE {sequenceName} RENAME TO {oldSequenceName};")
                            .AppendLine($"ALTER TABLE {table} ALTER COLUMN {column} DROP DEFAULT;")
                            .AppendLine($"ALTER TABLE {table} ALTER COLUMN {column} ADD GENERATED {identityTypeClause} AS IDENTITY;")
                            .AppendLine($"SELECT * FROM setval('{sequenceName}', nextval('{oldSequenceName}'), false);")
                            .AppendLine($"DROP SEQUENCE {oldSequenceName};");
                        break;
                    default:
                        throw new NotSupportedException($"Don't know how to migrate serial column to {newStrategy}");
                    }
                }
                else if (oldStrategy == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn ||
                         oldStrategy == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                {
                    switch (newStrategy)
                    {
                    case null:
                        // Drop the identity, converting the column to a regular int
                        builder.AppendLine(alterBase).AppendLine("DROP IDENTITY;");
                        break;
                    case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                        builder.Append(alterBase).AppendLine("SET GENERATED ALWAYS;");
                        break;
                    case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                        builder.Append(alterBase).AppendLine("SET GENERATED BY DEFAULT;");
                        break;
                    case NpgsqlValueGenerationStrategy.SerialColumn:
                        throw new NotSupportedException("Migrating from identity to serial isn't currently supported (and is a bad idea)");
                    default:
                        throw new NotSupportedException($"Don't know how to migrate identity column to {newStrategy}");
                    }
                }
                else if (oldStrategy == null)
                {
                    switch (newStrategy)
                    {
                    case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                        builder.Append(alterBase).AppendLine("ADD GENERATED ALWAYS AS IDENTITY;");
                        break;
                    case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                        builder.Append(alterBase).AppendLine("ADD GENERATED BY DEFAULT AS IDENTITY;");
                        break;
                    case NpgsqlValueGenerationStrategy.SerialColumn:
                        switch (type)
                        {
                        case "int":
                        case "int4":
                        case "bigint":
                        case "int8":
                        case "smallint":
                        case "int2":
                            newSequenceName = $"{operation.Table}_{operation.Name}_seq";
                            Generate(new CreateSequenceOperation
                            {
                                Name = newSequenceName,
                                ClrType = operation.ClrType
                            }, model, builder);
                            defaultValueSql = $@"nextval('{Dependencies.SqlGenerationHelper.DelimitIdentifier(newSequenceName)}')";
                            // Note: we also need to set the sequence ownership, this is done below after the ALTER COLUMN
                            break;
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Don't know how to apply value generation strategy {newStrategy}");
                    }
                }
            }

            // TYPE
            builder.Append(alterBase)
                .Append("TYPE ")
                .Append(type)
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            // NOT NULL
            builder.Append(alterBase)
                .Append(operation.IsNullable ? "DROP NOT NULL" : "SET NOT NULL")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            // DEFAULT.
            // Note that identity columns don't have a regular default but trying
            // to drop the default triggers an error.
            if (newStrategy != NpgsqlValueGenerationStrategy.IdentityAlwaysColumn &&
                newStrategy != NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
            {
                builder.Append(alterBase);
                if (operation.DefaultValue != null || defaultValueSql != null)
                {
                    builder.Append("SET");
                    DefaultValue(operation.DefaultValue, defaultValueSql, builder);
                }
                else
                    builder.Append("DROP DEFAULT");
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }


            // A sequence has been created because this column was altered to be a serial.
            // Change the sequence's ownership.
            if (newSequenceName != null)
            {
                builder
                    .Append("ALTER SEQUENCE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(newSequenceName))
                    .Append(" OWNED BY ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table))
                    .Append('.')
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
            }

            // Comment
            var oldComment = operation.OldColumn[NpgsqlAnnotationNames.Comment] as string;
            var newComment = operation[NpgsqlAnnotationNames.Comment] as string;

            if (oldComment != newComment)
            {
                var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

                builder
                    .Append("COMMENT ON COLUMN ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append('.')
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" IS ")
                    .Append(stringTypeMapping.GenerateSqlLiteral(newComment));
            }

            EndStatement(builder);
        }

        protected override void Generate(RenameIndexOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var qualifiedName = new StringBuilder();
            if (operation.Schema != null)
            {
                qualifiedName
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Schema))
                    .Append(".");
            }
            qualifiedName.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            // TODO: Rename across schema will break, see #44
            Rename(qualifiedName.ToString(), Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName), "INDEX", builder);
            EndStatement(builder);
        }

        protected override void Generate(RenameSequenceOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var name = operation.Name;
            if (operation.NewName != null)
            {
                var qualifiedName = new StringBuilder();
                if (operation.Schema != null)
                {
                    qualifiedName
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Schema))
                        .Append(".");
                }
                qualifiedName.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

                Rename(qualifiedName.ToString(), Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName), "SEQUENCE", builder);

                name = operation.NewName;
            }

            if (operation.NewSchema != null)
            {
                Transfer(operation.NewSchema, operation.Schema, name, "SEQUENCE", builder);
            }

            EndStatement(builder);
        }

        protected override void Generate(RenameTableOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var name = operation.Name;
            if (operation.NewName != null)
            {
                var qualifiedName = new StringBuilder();
                if (operation.Schema != null)
                {
                    qualifiedName
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Schema))
                        .Append(".");
                }
                qualifiedName.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

                Rename(qualifiedName.ToString(), Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName), "TABLE", builder);

                name = operation.NewName;
            }

            if (operation.NewSchema != null)
            {
                Transfer(operation.NewSchema, operation.Schema, name, "TABLE", builder);
            }

            EndStatement(builder);
        }

        protected override void Generate(
            [NotNull] CreateIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var method = (string)operation[NpgsqlAnnotationNames.Prefix + NpgsqlAnnotationNames.IndexMethod];

            builder.Append("CREATE ");

            if (operation.IsUnique)
            {
                builder.Append("UNIQUE ");
            }

            builder
                .Append("INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ON ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));

            if (method != null)
            {
                builder
                    .Append(" USING ")
                    .Append(method);
            }

            builder
                .Append(" (")
                .Append(ColumnList(operation.Columns))
                .Append(")");

            if (!string.IsNullOrEmpty(operation.Filter))
            {
                builder
                    .Append(" WHERE ")
                    .Append(operation.Filter);
            }

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(EnsureSchemaOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            // PostgreSQL 9.2 and below unfortunately doesn't have CREATE SCHEMA IF NOT EXISTS.
            // An attempted workaround by creating a function which checks and creates the schema, and then invoking it, failed because
            // of #641 (pg_temp doesn't exist yet).
            // So the only workaround for pre-9.3 PostgreSQL, at least for now, is to define all tables in the public schema.
            // TODO: Since Npgsql 3.1 we can now ensure schema with a function in pg_temp

            // NOTE: Technically the public schema can be dropped so we should also be ensuring it, but this is a rare case and
            // we want to allow pre-9.3
            if (operation.Name == "public")
                return;

            builder
                .Append("CREATE SCHEMA IF NOT EXISTS ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        public virtual void Generate(NpgsqlCreateDatabaseOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE DATABASE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (operation.Template != null)
            {
                builder
                    .Append(" TEMPLATE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Template));
            }

            if (operation.Tablespace != null)
            {
                builder
                    .Append(" TABLESPACE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Tablespace));
            }

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder, suppressTransaction: true);
        }

        public virtual void Generate(NpgsqlDropDatabaseOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var dbName = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name);

            builder
                // TODO: The following revokes connection only for the public role, what about other connecting roles?
                .Append("REVOKE CONNECT ON DATABASE ")
                .Append(dbName)
                .Append(" FROM PUBLIC")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                // TODO: For PG <= 9.1, the column name is prodpic, not pid (see http://stackoverflow.com/questions/5408156/how-to-drop-a-postgresql-database-if-there-are-active-connections-to-it)
                .Append(
                    "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE datname = '")
                .Append(operation.Name)
                .Append("'")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: true)
                .Append("DROP DATABASE ")
                .Append(dbName)
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder, suppressTransaction: true);
        }

        protected override void Generate(
            AlterDatabaseOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            foreach (var extension in PostgresExtension.GetPostgresExtensions(operation))
            {
                builder
                    .Append("CREATE EXTENSION IF NOT EXISTS ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(extension.Name));

                if (extension.Schema != null)
                {
                    builder
                        .Append(" SCHEMA ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(extension.Schema));
                }

                if (extension.Version != null)
                {
                    builder
                        .Append(" VERSION ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(extension.Version));
                }

                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder, suppressTransaction: true);
            }
        }

        protected override void Generate(DropIndexOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(
            [NotNull] RenameColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" RENAME COLUMN ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" TO ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        #endregion Standard migrations

        #region Utilities

        protected override void ColumnDefinition(
            [CanBeNull] string schema,
            [NotNull] string table,
            [NotNull] string name,
            [NotNull] Type clrType,
            [CanBeNull] string type,
            [CanBeNull] bool? unicode,
            [CanBeNull] int? maxLength,
            bool rowVersion,
            bool nullable,
            [CanBeNull] object defaultValue,
            [CanBeNull] string defaultValueSql,
            [CanBeNull] string computedColumnSql,
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(builder, nameof(builder));

            if (type == null)
                type = GetColumnType(schema, table, name, clrType, unicode, maxLength, rowVersion, model);

            CheckForOldValueGenerationAnnotation(annotatable);
            var valueGenerationStrategy = annotatable[NpgsqlAnnotationNames.ValueGenerationStrategy] as NpgsqlValueGenerationStrategy?;
            if (valueGenerationStrategy == NpgsqlValueGenerationStrategy.SerialColumn)
            {
                switch (type)
                {
                case "int":
                case "int4":
                case "integer":
                    type = "serial";
                    break;
                case "bigint":
                case "int8":
                    type = "bigserial";
                    break;
                case "smallint":
                case "int2":
                    type = "smallserial";
                    break;
                }
            }

            base.ColumnDefinition(
                schema,
                table,
                name,
                clrType,
                type,
                unicode,
                maxLength,
                rowVersion,
                nullable,
                defaultValue,
                defaultValueSql,
                computedColumnSql,
                annotatable,
                model,
                builder);

            switch (valueGenerationStrategy)
            {
            case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                builder.Append(" GENERATED ALWAYS AS IDENTITY");
                break;
            case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                builder.Append(" GENERATED BY DEFAULT AS IDENTITY");
                break;
            }
        }

#pragma warning disable 618
        // Version 1.0 had a bad strategy for expressing serial columns, which depended on a
        // ValueGeneratedOnAdd annotation. Detect that and throw.
        static void CheckForOldValueGenerationAnnotation([NotNull] IAnnotatable annotatable)
        {
            if (annotatable.FindAnnotation(NpgsqlAnnotationNames.ValueGeneratedOnAdd) != null)
                throw new NotSupportedException("The Npgsql:ValueGeneratedOnAdd annotation has been found in your migrations, but is no longer supported. Please replace it with '.Annotation(\"Npgsql:ValueGenerationStrategy\", NpgsqlValueGenerationStrategy.SerialColumn)' where you want PostgreSQL serial (autoincrement) columns, and remove it in all other cases.");
        }
#pragma warning restore 618

        /// <summary>
        /// Renames a database object such as an index or a sequence.
        /// </summary>
        /// <param name="name">An already delimited name of the object to rename</param>
        /// <param name="newName">An already delimited name of the new name</param>
        /// <param name="type">The type of the object (e.g. INDEX, SEQUENCE)</param>
        /// <param name="builder"></param>
        public virtual void Rename(
            [NotNull] string name,
            [NotNull] string newName,
            [NotNull] string type,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(newName, nameof(newName));
            Check.NotEmpty(type, nameof(type));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER ")
                .Append(type)
                .Append(' ')
                .Append(name)
                .Append(" RENAME TO ")
                .Append(newName)
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        public virtual void Transfer(
            [NotNull] string newSchema,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string type,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(newSchema, nameof(newSchema));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER ")
                .Append(type)
                .Append(" ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name, schema))
                .Append(" SET SCHEMA ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(newSchema));
        }

        #endregion Utilities

        #region System column utilities

        bool IsSystemColumn(string name) => _systemColumnNames.Contains(name);

        /// <summary>
        /// Tables in PostgreSQL implicitly have a set of system columns, which are always there.
        /// We want to allow users to access these columns (i.e. xmin for optimistic concurrency) but
        /// they should never generate migration operations.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/ddl-system-columns.html
        /// </remarks>
        readonly string[] _systemColumnNames = { "oid", "tableoid", "xmin", "cmin", "xmax", "cmax", "ctid" };

        #endregion System column utilities

        #region Storage parameter utilities

        Dictionary<string, string> GetStorageParameters(Annotatable annotatable)
            => annotatable.GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix))
                .ToDictionary(
                    a => a.Name.Substring(NpgsqlAnnotationNames.StorageParameterPrefix.Length),
                    a => GenerateStorageParameterValue(a.Value)
                );

        static string GenerateStorageParameterValue(object value)
        {
            if (value is bool)
                return (bool)value ? "true" : "false";
            if (value is string)
                return $"'{value}'";
            return value.ToString();
        }

        #endregion Storage parameter utilities
    }
}
