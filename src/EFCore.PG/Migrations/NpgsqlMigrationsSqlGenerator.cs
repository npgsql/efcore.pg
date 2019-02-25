using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations
{
    public class NpgsqlMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        readonly NpgsqlSqlGenerationHelper _sqlGenerationHelper;
        readonly NpgsqlTypeMappingSource _typeMappingSource;

        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull] readonly Version _postgresVersion;

        public NpgsqlMigrationsSqlGenerator(
            [NotNull] MigrationsSqlGeneratorDependencies dependencies,
            [NotNull] INpgsqlOptions npgsqlOptions)
            : base(dependencies)
        {
            _sqlGenerationHelper = (NpgsqlSqlGenerationHelper)dependencies.SqlGenerationHelper;
            _typeMappingSource = (NpgsqlTypeMappingSource)dependencies.TypeMappingSource;
            _postgresVersion = npgsqlOptions.PostgresVersion;
        }

        protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
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

            #region Customized base call

            // TODO: Could EF Core make this easier to override?

            builder.Append("CREATE ");

            if (operation[NpgsqlAnnotationNames.UnloggedTable] is bool unlogged && unlogged)
                builder.Append("UNLOGGED ");

            builder
                .Append("TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(" (");

            using (builder.Indent())
            {
                for (var i = 0; i < operation.Columns.Count; i++)
                {
                    var column = operation.Columns[i];
                    ColumnDefinition(column, model, builder);

                    if (i != operation.Columns.Count - 1)
                        builder.AppendLine(",");
                }

                if (operation.PrimaryKey != null)
                {
                    builder.AppendLine(",");
                    PrimaryKeyConstraint(operation.PrimaryKey, model, builder);
                }

                foreach (var uniqueConstraint in operation.UniqueConstraints)
                {
                    builder.AppendLine(",");
                    UniqueConstraint(uniqueConstraint, model, builder);
                }

                foreach (var foreignKey in operation.ForeignKeys)
                {
                    builder.AppendLine(",");
                    ForeignKeyConstraint(foreignKey, model, builder);
                }

                builder.AppendLine();
            }

            builder.Append(")");

            #endregion

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
            if (operation[NpgsqlAnnotationNames.Comment] is string comment && comment.Length > 0)
            {
                builder.AppendLine(';');

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
                builder.AppendLine(';');

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
                builder.AppendLine(';');
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

                builder.AppendLine(';');
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

                builder.AppendLine(';');
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

                builder.AppendLine(';');
                madeChanges = true;
            }

            // Unlogged table (null is equivalent to false)
            var oldUnlogged = operation.OldTable[NpgsqlAnnotationNames.UnloggedTable] is bool ou && ou;
            var newUnlogged = operation[NpgsqlAnnotationNames.UnloggedTable] is bool nu && nu;

            if (oldUnlogged != newUnlogged)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                    .Append(" SET ")
                    .Append(newUnlogged ? "UNLOGGED" : "LOGGED")
                    .AppendLine(";");

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

            if (operation[NpgsqlAnnotationNames.Comment] is string comment && comment.Length > 0)
            {
                builder.AppendLine(';');

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
                builder.AppendLine(';');
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

            var type = operation.ColumnType ?? GetColumnType(operation.Schema, operation.Table, operation.Name, operation.ClrType, null, operation.MaxLength, operation.IsFixedLength, false, model);

            string newSequenceName = null;
            var defaultValueSql = operation.DefaultValueSql;

            var table = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema);
            var column = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name);
            var alterBase = $"ALTER TABLE {table} ALTER COLUMN {column} ";

            // TYPE
            builder.Append(alterBase)
                .Append("TYPE ")
                .Append(type)
                .AppendLine(';');

            // NOT NULL
            builder.Append(alterBase)
                .Append(operation.IsNullable ? "DROP NOT NULL" : "SET NOT NULL")
                .AppendLine(';');

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
                        case "integer":
                        case "int":
                        case "int4":
                        case "bigint":
                        case "int8":
                        case "smallint":
                        case "int2":
                            newSequenceName = $"{operation.Table}_{operation.Name}_seq";
                            Generate(new CreateSequenceOperation
                            {
                                Schema = operation.Schema,
                                Name = newSequenceName,
                                ClrType = operation.ClrType
                            }, model, builder);

                            builder.Append(alterBase).Append("SET");
                            DefaultValue(null, $@"nextval('{Dependencies.SqlGenerationHelper.DelimitIdentifier(newSequenceName, operation.Schema)}')", builder);
                            builder.AppendLine(';');
                            // Note: we also need to set the sequence ownership, this is done below after the ALTER COLUMN
                            break;
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Don't know how to apply value generation strategy {newStrategy}");
                    }
                }
            }

            // DEFAULT.
            // Note that defaults values for value-generated columns (identity, serial) are managed above. This is
            // only for regular columns with user-specified default settings.
            if (newStrategy == null)
            {
                builder.Append(alterBase);
                if (operation.DefaultValue != null || defaultValueSql != null)
                {
                    builder.Append("SET");
                    DefaultValue(operation.DefaultValue, defaultValueSql, builder);
                }
                else
                    builder.Append("DROP DEFAULT");
                builder.AppendLine(';');
            }


            // A sequence has been created because this column was altered to be a serial.
            // Change the sequence's ownership.
            if (newSequenceName != null)
            {
                builder
                    .Append("ALTER SEQUENCE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(newSequenceName, operation.Schema))
                    .Append(" OWNED BY ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append('.')
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .AppendLine(';');
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
                    .Append(stringTypeMapping.GenerateSqlLiteral(newComment))
                    .AppendLine(';');
            }

            EndStatement(builder);
        }

        protected override void Generate(RenameIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.NewName != null &&
                operation.NewName != operation.Name)
            {
                Rename(operation.Schema, operation.Name, operation.NewName, "INDEX", builder);
            }

            // N.B. indexes are always stored in the same schema as the table.
            EndStatement(builder);
        }

        protected override void Generate(RenameSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var name = operation.Name;
            if (operation.NewName != null &&
                operation.NewName != operation.Name)
            {
                Rename(operation.Schema, operation.Name, operation.NewName, "SEQUENCE", builder);

                name = operation.NewName;
            }

            if (operation.NewSchema != null &&
                operation.NewSchema != operation.Schema)
            {
                Transfer(operation.NewSchema, operation.Schema, name, "SEQUENCE", builder);
            }

            EndStatement(builder);
        }

        protected override void Generate(RenameTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var name = operation.Name;
            if (operation.NewName != null &&
                operation.NewName != operation.Name)
            {
                Rename(operation.Schema, operation.Name, operation.NewName, "TABLE", builder);

                name = operation.NewName;
            }

            if (operation.NewSchema != null &&
                operation.NewSchema != operation.Schema)
            {
                Transfer(operation.NewSchema, operation.Schema, name, "TABLE", builder);
            }

            EndStatement(builder);
        }

        protected override void Generate(
            CreateIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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

            if (operation[NpgsqlAnnotationNames.IndexMethod] is string method && method.Length > 0)
            {
                builder
                    .Append(" USING ")
                    .Append(method);
            }

            var operators = operation[NpgsqlAnnotationNames.IndexOperators] as string[];

            builder
                .Append(" (")
                .Append(IndexColumnList(operation.Columns, operators))
                .Append(")");

            IndexOptions(operation, model, builder);

            builder.AppendLine(';');

            EndStatement(builder);
        }

        protected override void IndexOptions(CreateIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            if (operation[NpgsqlAnnotationNames.IndexInclude] is string[] includeProperties && includeProperties.Length > 0)
            {
                builder
                    .Append(" INCLUDE (")
                    .Append(ColumnList(includeProperties))
                    .Append(")");
            }

            base.IndexOptions(operation, model, builder);
        }

        protected override void Generate(EnsureSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
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
                .AppendLine(';');

            EndStatement(builder);
        }

        protected virtual void Generate(NpgsqlCreateDatabaseOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
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

            builder.AppendLine(';');

            EndStatement(builder, suppressTransaction: true);
        }

        public virtual void Generate(NpgsqlDropDatabaseOperation operation, [CanBeNull] IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var dbName = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name);

            builder
                // TODO: The following revokes connection only for the public role, what about other connecting roles?
                .AppendLine($"REVOKE CONNECT ON DATABASE {dbName} FROM PUBLIC;")
                // TODO: For PG <= 9.1, the column name is prodpic, not pid (see http://stackoverflow.com/questions/5408156/how-to-drop-a-postgresql-database-if-there-are-active-connections-to-it)
                .AppendLine($"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE datname = '{operation.Name}';")
                .EndCommand(suppressTransaction: true)
                .AppendLine($"DROP DATABASE {dbName};");

            EndStatement(builder, suppressTransaction: true);
        }

        protected override void Generate(
            AlterDatabaseOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(model, nameof(model));
            Check.NotNull(builder, nameof(builder));

            GenerateEnumStatements(operation, model, builder);
            GenerateRangeStatements(operation, model, builder);

            foreach (var extension in operation.Npgsql().PostgresExtensions)
                GenerateCreateExtension(extension, model, builder);

            builder.EndCommand();
        }

        protected virtual void GenerateCreateExtension(
            PostgresExtension extension,
            IModel model,
            MigrationCommandListBuilder builder)
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

            builder.AppendLine(';');
        }

        #region Enum management

        protected virtual void GenerateEnumStatements(
                [NotNull] AlterDatabaseOperation operation,
                [NotNull] IModel model,
                [NotNull] MigrationCommandListBuilder builder)
        {
            foreach (var enumTypeToCreate in operation.Npgsql().PostgresEnums
                .Where(ne => operation.Npgsql().OldPostgresEnums.All(oe => oe.Name != ne.Name)))
            {
                GenerateCreateEnum(enumTypeToCreate, model, builder);
            }

            foreach (var enumTypeToDrop in operation.Npgsql().OldPostgresEnums
                .Where(oe => operation.Npgsql().PostgresEnums.All(ne => ne.Name != oe.Name)))
            {
                GenerateDropEnum(enumTypeToDrop, model, builder);
            }

            // TODO: Some forms of enum alterations are actually supported...
            if (operation.Npgsql().PostgresEnums.FirstOrDefault(nr =>
                operation.Npgsql().OldPostgresEnums.Any(or => or.Name == nr.Name)
            ) is PostgresEnum enumTypeToAlter)
            {
                throw new NotSupportedException($"Altering enum type ${enumTypeToAlter} isn't supported.");
            }
        }

        protected virtual void GenerateCreateEnum(
            [NotNull] PostgresEnum enumType,
            [NotNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            var schema = enumType.Schema ?? model.Relational().DefaultSchema;

            // Schemas are normally created (or rather ensured) by the model differ, which scans all tables, sequences
            // and other database objects. However, it isn't aware of enums, so we always ensure schema on enum creation.
            if (schema != null)
                Generate(new EnsureSchemaOperation { Name = schema }, model, builder);

            builder
                .Append("CREATE TYPE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(enumType.Name, schema))
                .Append(" AS ENUM (");

            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            var labels = enumType.Labels;
            for (var i = 0; i < labels.Count; i++)
            {
                builder.Append(stringTypeMapping.GenerateSqlLiteral(labels[i]));
                if (i < labels.Count - 1)
                    builder.Append(", ");
            }

            builder.AppendLine(");");
        }

        protected virtual void GenerateDropEnum(
            [NotNull] PostgresEnum enumType,
            [NotNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            var schema = enumType.Schema ?? model.Relational().DefaultSchema;

            builder
                .Append("DROP TYPE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(enumType.Name, schema))
                .AppendLine(";");
        }

        #endregion Enum management

        #region Range management

        protected virtual void GenerateRangeStatements(
                [NotNull] AlterDatabaseOperation operation,
                [NotNull] IModel model,
                [NotNull] MigrationCommandListBuilder builder)
        {
            foreach (var rangeTypeToCreate in operation.Npgsql().PostgresRanges
                .Where(ne => operation.Npgsql().OldPostgresRanges.All(oe => oe.Name != ne.Name)))
            {
                GenerateCreateRange(rangeTypeToCreate, model, builder);
            }

            foreach (var rangeTypeToDrop in operation.Npgsql().OldPostgresRanges
                .Where(oe => operation.Npgsql().PostgresRanges.All(ne => ne.Name != oe.Name)))
            {
                GenerateDropRange(rangeTypeToDrop, model, builder);
            }

            if (operation.Npgsql().PostgresRanges.FirstOrDefault(nr =>
                operation.Npgsql().OldPostgresRanges.Any(or => or.Name == nr.Name)
            ) is PostgresRange rangeTypeToAlter)
            {
                throw new NotSupportedException($"Altering range type ${rangeTypeToAlter} isn't supported.");
            }
        }

        protected virtual void GenerateCreateRange(
            [NotNull] PostgresRange rangeType,
            [NotNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            var schema = rangeType.Schema ?? model.Relational().DefaultSchema;

            // Schemas are normally created (or rather ensured) by the model differ, which scans all tables, sequences
            // and other database objects. However, it isn't aware of ranges, so we always ensure schema on range creation.
            if (schema != null)
                Generate(new EnsureSchemaOperation { Name = schema }, model, builder);

            builder
                .Append("CREATE TYPE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(rangeType.Name, schema))
                .AppendLine($" AS RANGE (")
                .IncrementIndent();

            var def = new List<string> { $"SUBTYPE = {rangeType.Subtype}" };
            if (rangeType.CanonicalFunction != null)
                def.Add($"CANONICAL = {rangeType.CanonicalFunction}");
            if (rangeType.SubtypeOpClass != null)
                def.Add($"SUBTYPE_OPCLASS = {rangeType.SubtypeOpClass}");
            if (rangeType.CanonicalFunction != null)
                def.Add($"COLLATION = {rangeType.Collation}");
            if (rangeType.SubtypeDiff != null)
                def.Add($"SUBTYPE_DIFF = {rangeType.SubtypeDiff}");

            for (var i = 0; i < def.Count; i++)
                builder
                    .Append(def[i] + (i == def.Count - 1 ? null : ","))
                    .AppendLine();

            builder
                .DecrementIndent()
                .AppendLine(");");
        }

        protected virtual void GenerateDropRange(
            [NotNull] PostgresRange rangeType,
            [NotNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            var schema = rangeType.Schema ?? model.Relational().DefaultSchema;

            builder
                .Append("DROP TYPE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(rangeType.Name, schema))
                .AppendLine(";");
        }

        #endregion Range management

        protected override void Generate(DropIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(';');

            EndStatement(builder);
        }

        protected override void Generate(
            RenameColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" RENAME COLUMN ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" TO ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                .AppendLine(';');

            EndStatement(builder);
        }

        /// <summary>
        /// Builds commands for the given <see cref="InsertDataOperation" /> by making calls on the given
        /// <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            InsertDataOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var sqlBuilder = new StringBuilder();
            foreach (var modificationCommand in operation.GenerateModificationCommands(model))
            {
                var overridingSystemValue = modificationCommand.ColumnModifications.Any(m =>
                    m.Property?.Npgsql().ValueGenerationStrategy == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
                ((NpgsqlUpdateSqlGenerator)Dependencies.UpdateSqlGenerator).AppendInsertOperation(
                    sqlBuilder,
                    modificationCommand,
                    0,
                    overridingSystemValue);
            }

            builder.Append(sqlBuilder.ToString());

            builder.EndCommand();
        }

        protected override void Generate(CreateSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));

            if (VersionAtLeast(10, 0))
            {
                base.Generate(operation, model, builder);
            }
            else
            {
                // "CREATE SEQUENCE name AS type" expression is supported only in PostgreSQL 10 or above.
                // The base MigrationsSqlGenerator.Generate method generates that expression.
                // https://github.com/aspnet/EntityFrameworkCore/blob/master/src/EFCore.Relational/Migrations/MigrationsSqlGenerator.cs#L533-L535
                var oldValue = operation.ClrType;
                operation.ClrType = typeof(long);
                base.Generate(operation, model, builder);
                operation.ClrType = oldValue;
            }
        }

        #endregion Standard migrations

        #region Utilities

        protected override void ColumnDefinition(
            string schema,
            string table,
            string name,
            Type clrType,
            string type,
            bool? unicode,
            int? maxLength,
            bool? fixedLength,
            bool rowVersion,
            bool nullable,
            object defaultValue,
            string defaultValueSql,
            string computedColumnSql,
            IAnnotatable annotatable,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(builder, nameof(builder));

            if (type == null)
                type = GetColumnType(schema, table, name, clrType, unicode, maxLength, fixedLength, rowVersion, model);

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
                fixedLength,
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

        // Required to "activate" IsFixedLength
        protected override void ColumnDefinition(AddColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
            => ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation.ClrType,
                operation.ColumnType,
                operation.IsUnicode,
                operation.MaxLength,
                operation.IsFixedLength,
                operation.IsRowVersion,
                operation.IsNullable,
                operation.DefaultValue,
                operation.DefaultValueSql,
                operation.ComputedColumnSql,
                operation,
                model,
                builder);

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
        /// Renames a database object such as a table, index, or sequence.
        /// </summary>
        /// <param name="schema">The current schema of the object to rename.</param>
        /// <param name="name">The current name of the object to rename.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="type">The type of the object (e.g. TABLE, INDEX, SEQUENCE).</param>
        /// <param name="builder">The builder to which operations are appended.</param>
        public virtual void Rename(
            [CanBeNull] string schema,
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
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name, schema))
                .Append(" RENAME TO ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(newName))
                .AppendLine(';');
        }

        /// <summary>
        /// Transfers a database object such as a table, index, or sequence between schemas.
        /// </summary>
        /// <param name="newSchema">The new schema.</param>
        /// <param name="schema">The current schema.</param>
        /// <param name="name">The name of the object to transfer.</param>
        /// <param name="type">The type of the object (e.g. TABLE, INDEX, SEQUENCE).</param>
        /// <param name="builder">The builder to which operations are appended.</param>
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
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(newSchema))
                .AppendLine(';');
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

        #region Helpers

        /// <summary>
        /// True if <see cref="_postgresVersion"/> is null, greater than, or equal to the specified version.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <returns>
        /// True if <see cref="_postgresVersion"/> is null, greater than, or equal to the specified version.
        /// </returns>
        bool VersionAtLeast(int major, int minor)
            => _postgresVersion is null || new Version(major, minor) <= _postgresVersion;

        string IndexColumnList(string[] columns, string[] operators)
        {
            if (operators == null || operators.Length == 0)
                return ColumnList(columns);

            return string.Join(", ", columns.Select((v, i) =>
            {
                var identifier = Dependencies.SqlGenerationHelper.DelimitIdentifier(v);

                if (i >= operators.Length)
                    return identifier;

                var @operator = operators[i];

                if (string.IsNullOrEmpty(@operator))
                    return identifier;

                var delimitedOperator = TryParseSchema(@operator, out var name, out var schema)
                    ? Dependencies.SqlGenerationHelper.DelimitIdentifier(name, schema)
                    : Dependencies.SqlGenerationHelper.DelimitIdentifier(@operator);

                return string.Concat(identifier, " ", delimitedOperator);
            }));
        }

        static bool TryParseSchema(string identifier, out string name, out string schema)
        {
            var index = identifier.IndexOf('.');

            if (index >= 0)
            {
                schema = identifier.Substring(0, index);
                name = identifier.Substring(index + 1);
                return true;
            }

            schema = default;
            name = default;
            return false;
        }

        #endregion
    }
}
