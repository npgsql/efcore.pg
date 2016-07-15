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
        public NpgsqlMigrationsSqlGenerator(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalAnnotationProvider annotations)
            : base(commandBuilderFactory, sqlGenerationHelper, typeMapper, annotations)
        {
        }

        protected override void Generate(MigrationOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var createDatabaseOperation = operation as NpgsqlCreateDatabaseOperation;
            if (createDatabaseOperation != null)
            {
                Generate(createDatabaseOperation, model, builder);
                return;
            }

            var dropDatabaseOperation = operation as NpgsqlDropDatabaseOperation;
            if (dropDatabaseOperation != null)
            {
                Generate(dropDatabaseOperation, model, builder);
                return;
            }

            var createExtensionOperation = operation as NpgsqlCreatePostgresExtensionOperation;
            if (createExtensionOperation != null)
            {
                Generate(createExtensionOperation, model, builder);
                return;
            }

            var dropExtensionOperation = operation as NpgsqlDropPostgresExtensionOperation;
            if (dropExtensionOperation != null)
            {
                Generate(dropExtensionOperation, model, builder);
                return;
            }

            base.Generate(operation, model, builder);
        }

        #region Standard migrations

        protected override void Generate(AlterColumnOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var type = operation.ColumnType ?? GetColumnType(operation.Schema, operation.Table, operation.Name, operation.ClrType, null, null, false, model);

            var generatedOnAddAnnotation = operation[NpgsqlAnnotationNames.Prefix + NpgsqlAnnotationNames.ValueGeneratedOnAdd];
            var generatedOnAdd = generatedOnAddAnnotation != null && (bool)generatedOnAddAnnotation;

            string sequenceName = null;
            var defaultValueSql = operation.DefaultValueSql;
            if (generatedOnAdd && operation.DefaultValue == null && operation.DefaultValueSql == null)
            {
                switch (type)
                {
                case "int":
                case "int4":
                case "bigint":
                case "int8":
                case "smallint":
                case "int2":
                    sequenceName = $"{operation.Table}_{operation.Name}_seq";
                    Generate(new CreateSequenceOperation
                    {
                        Name = sequenceName,
                        ClrType = typeof(long)
                    }, model, builder, false);
                    defaultValueSql = $@"nextval({SqlGenerationHelper.DelimitIdentifier(sequenceName)})";
                    // Note: we also need to set the sequence ownership, this is done below
                    // after the ALTER COLUMN
                    break;
                case "uuid":
                    defaultValueSql = "uuid_generate_v4()";
                    break;
                default:
                    throw new InvalidOperationException($"Column {operation.Name} of type {type} has ValueGenerated.OnAdd but no default value is defined");
                }
            }

            var identifier = SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema);
            var alterBase = $"ALTER TABLE {identifier} ALTER COLUMN {SqlGenerationHelper.DelimitIdentifier(operation.Name)} ";

            // TYPE
            builder.Append(alterBase)
                .Append("TYPE ")
                .Append(type)
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            // NOT NULL
            builder.Append(alterBase)
                .Append(operation.IsNullable ? "DROP NOT NULL" : "SET NOT NULL")
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            // DEFAULT
            builder.Append(alterBase);
            if (operation.DefaultValue != null || defaultValueSql != null)
            {
                builder.Append("SET");
                DefaultValue(operation.DefaultValue, defaultValueSql, builder);
            }
            else
                builder.Append("DROP DEFAULT");

            // ALTER SEQUENCE
            if (sequenceName != null)
            {
                // Terminate the DEFAULT above
                builder.AppendLine(SqlGenerationHelper.StatementTerminator);

                builder
                    .Append("ALTER SEQUENCE ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(sequenceName))
                    .Append(" OWNED BY ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table))
                    .Append('.')
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name));
            }

            EndStatement(builder);
        }

        protected override void Generate(CreateSequenceOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Generate(operation, model, builder, true);
        }

        void Generate(CreateSequenceOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder, bool endStatement)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.ClrType != typeof(long))
            {
                throw new NotSupportedException("PostgreSQL sequences can only be bigint (long)");
            }

            builder
                .Append("CREATE SEQUENCE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

            builder
                .Append(" START WITH ")
                .Append(SqlGenerationHelper.GenerateLiteral(operation.StartValue));

            SequenceOptions(operation, model, builder);

            builder.AppendLine(SqlGenerationHelper.StatementTerminator);

            if (endStatement)
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
                    .Append(operation.Schema)
                    .Append(".");
            }
            qualifiedName
                .Append(operation.Table)
                .Append(".")
                .Append(operation.Name);

            // TODO: Rename across schema will break, see #44
            Rename(qualifiedName.ToString(), operation.NewName, "INDEX", builder);
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
                        .Append(operation.Schema)
                        .Append(".");
                }
                qualifiedName.Append(operation.Name);

                Rename(qualifiedName.ToString(), operation.NewName, "SEQUENCE", builder);

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
                        .Append(operation.Schema)
                        .Append(".");
                }
                qualifiedName.Append(operation.Name);

                Rename(qualifiedName.ToString(), operation.NewName, "TABLE", builder);

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
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ON ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));

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

            builder.AppendLine(SqlGenerationHelper.StatementTerminator);

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
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        public virtual void Generate(NpgsqlCreateDatabaseOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE DATABASE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (operation.Template != null)
            {
                builder
                    .Append(" TEMPLATE ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Template));
            }

            builder.AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder, suppressTransaction: true);
        }

        public virtual void Generate(NpgsqlDropDatabaseOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var dbName = SqlGenerationHelper.DelimitIdentifier(operation.Name);

            builder
                // TODO: The following revokes connection only for the public role, what about other connecting roles?
                .Append("REVOKE CONNECT ON DATABASE ")
                .Append(dbName)
                .Append(" FROM PUBLIC")
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                // TODO: For PG <= 9.1, the column name is prodpic, not pid (see http://stackoverflow.com/questions/5408156/how-to-drop-a-postgresql-database-if-there-are-active-connections-to-it)
                .Append(
                    "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '")
                .Append(operation.Name)
                .Append("'")
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: true)
                .Append("DROP DATABASE ")
                .Append(dbName)
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder, suppressTransaction: true);
        }

        protected override void Generate(DropIndexOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(SqlGenerationHelper.StatementTerminator);

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
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" RENAME COLUMN ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" TO ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        #endregion Standard migrations

        #region PostgreSQL extensions

        public virtual void Generate(NpgsqlCreatePostgresExtensionOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE EXTENSION IF NOT EXISTS ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(" SCHEMA ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Schema));
            }

            if (operation.Version != null)
            {
                builder
                    .Append(" VERSION ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Version));
            }

            builder.AppendLine(SqlGenerationHelper.StatementTerminator);
            EndStatement(builder, suppressTransaction: true);
        }

        public virtual void Generate(NpgsqlDropPostgresExtensionOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP EXTENSION ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator);
            EndStatement(builder, suppressTransaction: true);
        }

        #endregion PostgreSQL extensions

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

            var generatedOnAddAnnotation = annotatable[NpgsqlAnnotationNames.Prefix + NpgsqlAnnotationNames.ValueGeneratedOnAdd];
            var generatedOnAdd = generatedOnAddAnnotation != null && (bool)generatedOnAddAnnotation;

            // int-like properties marked with OnAdd and without default values are
            // treated as serial column. Similar for UUID.
            if (generatedOnAdd && defaultValue == null && defaultValueSql == null)
            {
                switch (type)
                {
                case "int":
                case "int4":
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
                case "uuid":
                    defaultValueSql = "uuid_generate_v4()";
                    break;
                default:
                    throw new InvalidOperationException($"Column {name} of type {type} has ValueGenerated.OnAdd but no default value is defined");
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
        }

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
                .Append(SqlGenerationHelper.DelimitIdentifier(name))
                .Append(" RENAME TO ")
                .Append(SqlGenerationHelper.DelimitIdentifier(newName))
                .AppendLine(SqlGenerationHelper.StatementTerminator);
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
                .Append(SqlGenerationHelper.DelimitIdentifier(name, schema))
                .Append(" SET SCHEMA ")
                .Append(SqlGenerationHelper.DelimitIdentifier(newSchema));
        }

        protected override void ForeignKeyAction(ReferentialAction referentialAction, MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            if (referentialAction == ReferentialAction.Restrict)
            {
                builder.Append("NO ACTION");
            }
            else
            {
                base.ForeignKeyAction(referentialAction, builder);
            }
        }

        string ColumnList(string[] columns) => string.Join(", ", columns.Select(SqlGenerationHelper.DelimitIdentifier));

        #endregion Utilities
    }
}
