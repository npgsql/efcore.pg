using System.Globalization;
using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

/// <summary>
///     PostgreSQL-specific implementation of <see cref="MigrationsSqlGenerator" />.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each <see cref="DbContext" /> instance will use
///         its own instance of this service. The implementation may depend on other services registered with any lifetime. The
///         implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>.
///     </para>
/// </remarks>
public class NpgsqlMigrationsSqlGenerator : MigrationsSqlGenerator
{
    private IReadOnlyList<MigrationOperation> _operations = null!;
    private readonly RelationalTypeMapping _stringTypeMapping;

    /// <summary>
    ///     The backend version to target.
    /// </summary>
    private readonly Version _postgresVersion;

    /// <summary>
    ///     Creates a new <see cref="NpgsqlMigrationsSqlGenerator" /> instance.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    /// <param name="npgsqlSingletonOptions">The singleton options to use.</param>
    public NpgsqlMigrationsSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies,
        INpgsqlSingletonOptions npgsqlSingletonOptions)
        : base(dependencies)
    {
        _postgresVersion = npgsqlSingletonOptions.PostgresVersion;
        _stringTypeMapping = dependencies.TypeMappingSource.GetMapping(typeof(string))
            ?? throw new InvalidOperationException("No string type mapping found");
    }

    /// <inheritdoc />
    public override IReadOnlyList<MigrationCommand> Generate(
        IReadOnlyList<MigrationOperation> operations,
        IModel? model = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
    {
        IReadOnlyList<MigrationCommand> results;
        _operations = operations;

        try
        {
            results = base.Generate(operations, model, options);

            AddSequenceBumpingForSeeding();

            return results;
        }
        finally
        {
            _operations = null!;
        }

        void AddSequenceBumpingForSeeding()
        {
            // For all tables where we had data seeding insertions, find all columns mapped to properties with identity/serial value
            // generation strategy. We'll bump the sequences for those columns.
            var seededGeneratedColumns = operations
                .OfType<InsertDataOperation>()
                .Select(o => new { o.Schema, o.Table })
                .Distinct()
                .SelectMany(
                    t => model?.GetRelationalModel().FindTable(t.Table, t.Schema)?.Columns
                            .Where(
                                c => c.PropertyMappings.Any(
                                    p => p.Property.GetValueGenerationStrategy() is
                                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                                        or NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                                        or NpgsqlValueGenerationStrategy.SerialColumn))
                        ?? [])
                .Distinct()
                .ToArray();

            if (!seededGeneratedColumns.Any())
            {
                return;
            }

            var builder = new MigrationCommandListBuilder(Dependencies);

            foreach (var c in seededGeneratedColumns)
            {
                // Weirdly, pg_get_serial_sequence accepts a standard quoted "schema"."table" inside its first
                // parameter string literal, but the second one is a column name that shouldn't be double-quoted...

                var table = Dependencies.SqlGenerationHelper.DelimitIdentifier(c.Table.Name, c.Table.Schema);
                var column = Dependencies.SqlGenerationHelper.DelimitIdentifier(c.Name);
                var unquotedColumn = c.Name.Replace("'", "''");

                // When generating idempotent scripts, migration DDL is enclosed in anonymous DO blocks,
                // where PERFORM must be used instead of SELECT
                var selectOrPerform = options.HasFlag(MigrationsSqlGenerationOptions.Idempotent)
                    ? "PERFORM"
                    : "SELECT";

                // Set the sequence's value to the greater of:
                // 1. Maximum value currently present in the column (i.e. just seeded)
                // 2. Current value of the sequence (the max value above could be out of range of the sequence,
                //    e.g. negative values seeded)
                builder
                    .AppendLine(
                        $"""
{selectOrPerform} setval(
    pg_get_serial_sequence('{table}', '{unquotedColumn}'),
    GREATEST(
        (SELECT MAX({column}) FROM {table}) + 1,
        nextval(pg_get_serial_sequence('{table}', '{unquotedColumn}'))),
    false);
""");
            }

            builder.EndCommand();

            results = results.Concat(builder.GetCommandList()).ToArray();
        }
    }

    /// <inheritdoc />
    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
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

    /// <inheritdoc />
    protected override void Generate(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        if (!terminate && operation.Comment is not null)
        {
            throw new ArgumentException(
                $"When generating migrations SQL for {nameof(CreateTableOperation)}, can't produce unterminated SQL with comments");
        }

        operation.Columns.RemoveAll(c => IsSystemColumn(c.Name));

        builder.Append("CREATE ");

        if (operation[NpgsqlAnnotationNames.UnloggedTable] is true)
        {
            builder.Append("UNLOGGED ");
        }

        builder
            .Append("TABLE ")
            .Append(DelimitIdentifier(operation.Name, operation.Schema))
            .AppendLine(" (");

        using (builder.Indent())
        {
            CreateTableColumns(operation, model, builder);
            CreateTableConstraints(operation, model, builder);
            builder.AppendLine();
        }

        builder.Append(")");

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
                .Append(DelimitIdentifier(parentTableName, parentTableSchema))
                .Append(" (")
                .Append(string.Join(", ", interleavePrefix.Select(c => DelimitIdentifier(c))))
                .Append(")");
        }

        AppendStoreParameters(operation, builder, withLeadingNewline: true);

        // Comment on the table
        if (operation.Comment is not null)
        {
            builder.AppendLine(";");

            builder
                .Append("COMMENT ON TABLE ")
                .Append(DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" IS ")
                .Append(_stringTypeMapping.GenerateSqlLiteral(operation.Comment));
        }

        // Comments on the columns
        foreach (var columnOp in operation.Columns.Where(c => c.Comment is not null))
        {
            var columnComment = columnOp.Comment;
            builder.AppendLine(";");

            builder
                .Append("COMMENT ON COLUMN ")
                .Append(DelimitIdentifier(operation.Name, operation.Schema))
                .Append(".")
                .Append(DelimitIdentifier(columnOp.Name))
                .Append(" IS ")
                .Append(_stringTypeMapping.GenerateSqlLiteral(columnComment));
        }

        if (terminate)
        {
            builder.AppendLine(";");
            EndStatement(builder);
        }
    }

    /// <inheritdoc />
    protected override void Generate(AlterTableOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        var alterTableBaseSql = $"ALTER TABLE {DelimitIdentifier(operation.Name, operation.Schema)}";
        var madeChanges = false;

        // Storage parameters
        madeChanges |= AppendStorageParameterAlterations(operation.OldTable, operation, alterTableBaseSql, builder);

        // Comment
        if (operation.Comment != operation.OldTable.Comment)
        {
            builder
                .Append("COMMENT ON TABLE ")
                .Append(DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" IS ")
                .Append(_stringTypeMapping.GenerateSqlLiteral(operation.Comment));

            builder.AppendLine(";");
            madeChanges = true;
        }

        // Unlogged table (null is equivalent to false)
        var oldUnlogged = operation.OldTable[NpgsqlAnnotationNames.UnloggedTable] is true;
        var newUnlogged = operation[NpgsqlAnnotationNames.UnloggedTable] is true;

        if (oldUnlogged != newUnlogged)
        {
            builder
                .Append(alterTableBaseSql)
                .Append(" SET ")
                .Append(newUnlogged ? "UNLOGGED" : "LOGGED")
                .AppendLine(";");

            madeChanges = true;
        }

        if (madeChanges)
        {
            EndStatement(builder);
        }
    }

    /// <inheritdoc />
    protected override void Generate(
        DropColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        // Never touch system columns
        if (IsSystemColumn(operation.Name))
        {
            return;
        }

        base.Generate(operation, model, builder, terminate);
    }

    /// <inheritdoc />
    protected override void Generate(
        AddColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        if (!terminate && operation.Comment is not null)
        {
            throw new ArgumentException(
                $"When generating migrations SQL for {nameof(AddColumnOperation)}, can't produce unterminated SQL with comments");
        }

        // Never touch system columns
        if (IsSystemColumn(operation.Name))
        {
            return;
        }

        if (operation[NpgsqlAnnotationNames.ValueGenerationStrategy] is NpgsqlValueGenerationStrategy strategy)
        {
            switch (strategy)
            {
                case NpgsqlValueGenerationStrategy.SerialColumn:
                case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                    // NB: This gets added to all added non-nullable columns by MigrationsModelDiffer. We need to suppress
                    // it, here because PG can't have both IDENTITY/SERIAL and a DEFAULT constraint on the same column.
                    operation.DefaultValue = null;
                    break;
            }
        }

        base.Generate(operation, model, builder, terminate: false);

        if (operation.Comment is not null)
        {
            builder.AppendLine(";");

            builder
                .Append("COMMENT ON COLUMN ")
                .Append(DelimitIdentifier(operation.Table, operation.Schema))
                .Append(".")
                .Append(DelimitIdentifier(operation.Name))
                .Append(" IS ")
                .Append(_stringTypeMapping.GenerateSqlLiteral(operation.Comment));
        }

        if (terminate)
        {
            builder.AppendLine(";");
            EndStatement(builder);
        }
    }

    /// <inheritdoc />
    protected override void Generate(AlterColumnOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        // Never touch system columns
        if (IsSystemColumn(operation.Name))
        {
            return;
        }

        var column = model?.GetRelationalModel().FindTable(operation.Table, operation.Schema)
            ?.Columns.FirstOrDefault(c => c.Name == operation.Name);

        ApplyTsVectorColumnSql(operation, model, operation.Name, operation.Schema, operation.Table);

        // Note: OldColumn doesn't have Schema, Table or Name populated (https://github.com/dotnet/efcore/issues/28041), so we take these
        // from the new column (they're identical in any case).
        ApplyTsVectorColumnSql(operation.OldColumn, model, operation.Name, operation.Schema, operation.Table);

        if (operation.ComputedColumnSql != operation.OldColumn.ComputedColumnSql
            || operation.IsStored != operation.OldColumn.IsStored)
        {
            // TODO: The following will fail if the column being altered is part of an index.
            // SqlServer recreates indexes, but wait to see if PostgreSQL will introduce a proper ALTER TABLE ALTER COLUMN
            // that allows us to do this cleanly.
            var dropColumnOperation = new DropColumnOperation
            {
                Schema = operation.Schema,
                Table = operation.Table,
                Name = operation.Name
            };

            if (column is not null)
            {
                dropColumnOperation.AddAnnotations(column.GetAnnotations());
            }

            Generate(dropColumnOperation, model, builder);

            var addColumnOperation = new AddColumnOperation
            {
                Schema = operation.Schema,
                Table = operation.Table,
                Name = operation.Name,
                ClrType = operation.ClrType,
                ColumnType = operation.ColumnType,
                IsUnicode = operation.IsUnicode,
                IsFixedLength = operation.IsFixedLength,
                MaxLength = operation.MaxLength,
                Precision = operation.Precision,
                Scale = operation.Scale,
                IsRowVersion = operation.IsRowVersion,
                IsNullable = operation.IsNullable,
                DefaultValue = operation.DefaultValue,
                DefaultValueSql = operation.DefaultValueSql,
                ComputedColumnSql = operation.ComputedColumnSql,
                IsStored = operation.IsStored,
                Comment = operation.Comment,
                Collation = operation.Collation
            };
            addColumnOperation.AddAnnotations(operation.GetAnnotations());
            Generate(addColumnOperation, model, builder);
            RecreateIndexes(column, operation, builder);
            builder.EndCommand();

            return;
        }

        string? newSequenceName = null;

        var alterBase = $"ALTER TABLE {DelimitIdentifier(operation.Table, operation.Schema)} "
            + $"ALTER COLUMN {DelimitIdentifier(operation.Name)} ";

        // TYPE + COLLATION
        var type = operation.ColumnType ?? GetColumnType(operation.Schema, operation.Table, operation.Name, operation, model)!;
        var oldType = IsOldColumnSupported(model)
            ? operation.OldColumn.ColumnType ?? GetColumnType(operation.Schema, operation.Table, operation.Name, operation.OldColumn, model)
            : null;

        // If a collation was defined on the column specifically, via the standard EF mechanism, it will be
        // available in operation.Collation (as usual).
        // If not, there may be a model-wide default column collation, which gets transmitted via the Npgsql-specific annotation.
        // This mechanism is obsolete, and EF Core's bulk model configuration can be used instead; but we continue to support it for
        // backwards compat.
#pragma warning disable CS0618
        var oldCollation = (string?)(operation.OldColumn.Collation ?? operation.OldColumn[NpgsqlAnnotationNames.DefaultColumnCollation]);
        var newCollation = (string?)(operation.Collation ?? operation[NpgsqlAnnotationNames.DefaultColumnCollation]);
#pragma warning restore CS0618

        if (type != oldType || newCollation != oldCollation)
        {
            builder
                .Append(alterBase)
                .Append("TYPE ")
                .Append(type);

            if (newCollation != oldCollation)
            {
                builder.Append(" COLLATE ").Append(DelimitIdentifier(newCollation ?? "default"));
            }

            builder.AppendLine(";");
        }

        if (operation is { IsNullable: true, OldColumn.IsNullable: false })
        {
            builder
                .Append(alterBase)
                .Append("DROP NOT NULL")
                .AppendLine(";");
        }
        else if (operation is { IsNullable: false, OldColumn.IsNullable: true })
        {
            // The column is being made non-nullable. Generate an update statement before doing that, to convert any existing null values to
            // the default value (otherwise PostgreSQL fails).
            if (operation.DefaultValueSql is not null || operation.DefaultValue is not null)
            {
                string defaultValueSql;
                if (operation.DefaultValueSql is not null)
                {
                    defaultValueSql = operation.DefaultValueSql;
                }
                else
                {
                    Check.DebugAssert(operation.DefaultValue is not null, "operation.DefaultValue is not null");

                    var typeMapping = (type != null
                            ? Dependencies.TypeMappingSource.FindMapping(operation.DefaultValue.GetType(), type)
                            : null)
                        ?? Dependencies.TypeMappingSource.GetMappingForValue(operation.DefaultValue);

                    defaultValueSql = typeMapping.GenerateSqlLiteral(operation.DefaultValue);
                }

                builder
                    .Append("UPDATE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" SET ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" = ")
                    .Append(defaultValueSql)
                    .Append(" WHERE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" IS NULL")
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }

            builder
                .Append(alterBase)
                .Append("SET NOT NULL")
                .AppendLine(";");
        }

        // Compression
        var oldCompressionMethod = (string?)operation.OldColumn[NpgsqlAnnotationNames.CompressionMethod];
        var newCompressionMethod = (string?)operation[NpgsqlAnnotationNames.CompressionMethod];

        if (newCompressionMethod != oldCompressionMethod)
        {
            builder
                .Append(alterBase)
                .Append("SET COMPRESSION ")
                .Append(newCompressionMethod ?? "default");
        }

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
                var sequence = DelimitIdentifier($"{operation.Table}_{operation.Name}_seq", operation.Schema);
                switch (newStrategy)
                {
                    case null:
                        // Drop the serial, converting the column to a regular int
                        builder.AppendLine($"DROP SEQUENCE {sequence} CASCADE;");
                        break;
                    case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                    case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                        // Convert serial column to identity, maintaining the current sequence value
                        var identityTypeClause = newStrategy == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                            ? "ALWAYS"
                            : "BY DEFAULT";
                        var oldSequence = DelimitIdentifier($"{operation.Table}_{operation.Name}_old_seq", operation.Schema);
                        var oldSequenceWithoutSchema = DelimitIdentifier($"{operation.Table}_{operation.Name}_old_seq");
                        builder
                            .AppendLine($"ALTER SEQUENCE {sequence} RENAME TO {oldSequenceWithoutSchema};")
                            .AppendLine($"{alterBase}DROP DEFAULT;")
                            .AppendLine($"{alterBase}ADD GENERATED {identityTypeClause} AS IDENTITY;")
                            // When generating idempotent scripts, migration DDL is enclosed in anonymous DO blocks,
                            // where PERFORM must be used instead of SELECT
                            .Append(Options.HasFlag(MigrationsSqlGenerationOptions.Idempotent) ? "PERFORM" : "SELECT")
                            .AppendLine($" * FROM setval('{sequence}', nextval('{oldSequence}'), false);")
                            .AppendLine($"DROP SEQUENCE {oldSequence};");
                        break;
                    default:
                        throw new NotSupportedException($"Don't know how to migrate serial column to {newStrategy}");
                }
            }
            else if (oldStrategy.IsIdentity())
            {
                switch (newStrategy)
                {
                    case null:
                        // Drop the identity, converting the column to a regular int
                        builder.Append(alterBase).AppendLine("DROP IDENTITY;");
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
            else if (oldStrategy is null)
            {
                switch (newStrategy)
                {
                    case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                    case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                        builder.Append(alterBase).AppendLine("DROP DEFAULT;");
                        builder.Append(alterBase).Append("ADD");
                        IdentityDefinition(operation, builder);
                        builder.AppendLine(";");
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
                                Generate(
                                    new CreateSequenceOperation
                                    {
                                        Schema = operation.Schema,
                                        Name = newSequenceName,
                                        ClrType = operation.ClrType
                                    }, model, builder);

                                builder.Append(alterBase).Append("SET");
                                DefaultValue(null, $@"nextval('{DelimitIdentifier(newSequenceName, operation.Schema)}')", type, builder);
                                builder.AppendLine(";");
                                // Note: we also need to set the sequence ownership, this is done below after the ALTER COLUMN
                                break;
                        }

                        break;
                    default:
                        throw new NotSupportedException($"Don't know how to apply value generation strategy {newStrategy}");
                }
            }
        }

        // Identity sequence options may have changed
        if (oldStrategy.IsIdentity() && newStrategy.IsIdentity())
        {
            var newSequenceOptions = IdentitySequenceOptionsData.Get(operation);
            var oldSequenceOptions = IdentitySequenceOptionsData.Get(operation.OldColumn);

            if (newSequenceOptions.StartValue != oldSequenceOptions.StartValue)
            {
                var startValue = newSequenceOptions.StartValue ?? 1;

                builder
                    .Append(alterBase)
                    .Append("RESTART WITH ")
                    .Append(startValue.ToString(CultureInfo.InvariantCulture))
                    .AppendLine(";");
            }

            if (newSequenceOptions.IncrementBy != oldSequenceOptions.IncrementBy)
            {
                builder
                    .Append(alterBase)
                    .Append("SET INCREMENT BY ")
                    .Append(newSequenceOptions.IncrementBy.ToString(CultureInfo.InvariantCulture))
                    .AppendLine(";");
            }

            if (newSequenceOptions.MinValue != oldSequenceOptions.MinValue)
            {
                builder
                    .Append(alterBase)
                    .Append(
                        newSequenceOptions.MinValue is null
                            ? "SET NO MINVALUE"
                            : "SET MINVALUE " + newSequenceOptions.MinValue)
                    .AppendLine(";");
            }

            if (newSequenceOptions.MaxValue != oldSequenceOptions.MaxValue)
            {
                builder
                    .Append(alterBase)
                    .Append(
                        newSequenceOptions.MaxValue is null
                            ? "SET NO MAXVALUE"
                            : "SET MAXVALUE " + newSequenceOptions.MaxValue)
                    .AppendLine(";");
            }

            if (newSequenceOptions.IsCyclic != oldSequenceOptions.IsCyclic)
            {
                builder
                    .Append(alterBase)
                    .Append(
                        newSequenceOptions.IsCyclic
                            ? "SET CYCLE"
                            : "SET NO CYCLE")
                    .AppendLine(";");
            }

            if (newSequenceOptions.NumbersToCache != oldSequenceOptions.NumbersToCache)
            {
                builder
                    .Append(alterBase)
                    .Append("SET CACHE ")
                    .Append(newSequenceOptions.NumbersToCache.ToString(CultureInfo.InvariantCulture))
                    .AppendLine(";");
            }
        }

        // DEFAULT.
        // Note that defaults values for value-generated columns (identity, serial) are managed above. This is
        // only for regular columns with user-specified default settings.
        if (newStrategy is null
            && (operation.DefaultValueSql != operation.OldColumn.DefaultValueSql
                || !Equals(operation.DefaultValue, operation.OldColumn.DefaultValue)))
        {
            builder.Append(alterBase);
            if (operation.DefaultValue is not null || operation.DefaultValueSql is not null)
            {
                builder.Append("SET");
                DefaultValue(operation.DefaultValue, operation.DefaultValueSql, type, builder);
            }
            else
            {
                builder.Append("DROP DEFAULT");
            }

            builder.AppendLine(";");
        }

        // A sequence has been created because this column was altered to be a serial.
        // Change the sequence's ownership.
        if (newSequenceName is not null)
        {
            builder
                .Append("ALTER SEQUENCE ")
                .Append(DelimitIdentifier(newSequenceName, operation.Schema))
                .Append(" OWNED BY ")
                .Append(DelimitIdentifier(operation.Table, operation.Schema))
                .Append(".")
                .Append(DelimitIdentifier(operation.Name))
                .AppendLine(";");
        }

        // Comment
        if (operation.Comment != operation.OldColumn.Comment)
        {
            builder
                .Append("COMMENT ON COLUMN ")
                .Append(DelimitIdentifier(operation.Table, operation.Schema))
                .Append(".")
                .Append(DelimitIdentifier(operation.Name))
                .Append(" IS ")
                .Append(_stringTypeMapping.GenerateSqlLiteral(operation.Comment))
                .AppendLine(";");
        }

        EndStatement(builder);
    }

    /// <inheritdoc />
    protected override void Generate(RenameIndexOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        if (operation.NewName is not null && operation.NewName != operation.Name)
        {
            Rename(operation.Schema, operation.Name, operation.NewName, "INDEX", builder);
        }

        // N.B. indexes are always stored in the same schema as the table.
        EndStatement(builder);
    }

    /// <inheritdoc />
    protected override void Generate(RenameSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        var name = operation.Name;
        if (operation.NewName is not null && operation.NewName != operation.Name)
        {
            Rename(operation.Schema, operation.Name, operation.NewName, "SEQUENCE", builder);

            name = operation.NewName;
        }

        if (operation.NewSchema is not null && operation.NewSchema != operation.Schema)
        {
            Transfer(operation.NewSchema, operation.Schema, name, "SEQUENCE", builder);
        }

        EndStatement(builder);
    }

    /// <inheritdoc />
    protected override void SequenceOptions(
        string? schema,
        string name,
        SequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool forAlter)
    {
        var intTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(int));
        var longTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(long));

        builder
            .Append(" INCREMENT BY ")
            .Append(intTypeMapping.GenerateSqlLiteral(operation.IncrementBy));

        if (operation.MinValue != null)
        {
            builder
                .Append(" MINVALUE ")
                .Append(longTypeMapping.GenerateSqlLiteral(operation.MinValue));
        }
        else if (forAlter)
        {
            builder
                .Append(" NO MINVALUE");
        }

        if (operation.MaxValue != null)
        {
            builder
                .Append(" MAXVALUE ")
                .Append(longTypeMapping.GenerateSqlLiteral(operation.MaxValue));
        }
        else if (forAlter)
        {
            builder
                .Append(" NO MAXVALUE");
        }

        builder.Append(operation.IsCyclic ? " CYCLE" : " NO CYCLE");
    }

    /// <inheritdoc />
    protected override void Generate(RestartSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        // PostgreSQL has ALTER SEQUENCE ... RESTART WITH x, which resets the current sequence value but does not change its start value
        // in the schema (so a subsequence RESTART without an argument resets it back to its original start value, not to x).
        // It also has ALTER SEQUENCE ... STARTS WITH x, which resets the schema start value but not the current value.
        // So we use both statements to reset both the current value and the schema value.
        if (operation.StartValue.HasValue)
        {
            var longTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(long));

            builder
                .Append("ALTER SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" START WITH ")
                .Append(longTypeMapping.GenerateSqlLiteral(operation.StartValue.Value))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            builder
                .Append("ALTER SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" RESTART")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }
        else
        {
            builder
                .Append("ALTER SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" RESTART")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        EndStatement(builder);
    }

    /// <inheritdoc />
    protected override void Generate(RenameTableOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        var name = operation.Name;
        if (operation.NewName is not null && operation.NewName != operation.Name)
        {
            Rename(operation.Schema, operation.Name, operation.NewName, "TABLE", builder);

            name = operation.NewName;
        }

        if (operation.NewSchema is not null && operation.NewSchema != operation.Schema)
        {
            Transfer(operation.NewSchema, operation.Schema, name, "TABLE", builder);
        }

        EndStatement(builder);
    }

    /// <inheritdoc />
    protected override void Generate(
        CreateIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        builder.Append("CREATE ");

        if (operation.IsUnique)
        {
            builder.Append("UNIQUE ");
        }

        builder.Append("INDEX ");

        var concurrently = operation[NpgsqlAnnotationNames.CreatedConcurrently] as bool? == true;
        if (concurrently)
        {
            builder.Append("CONCURRENTLY ");
        }

        builder
            .Append(DelimitIdentifier(operation.Name))
            .Append(" ON ")
            .Append(DelimitIdentifier(operation.Table, operation.Schema));

        var method = operation[NpgsqlAnnotationNames.IndexMethod] as string;
        if (method?.Length > 0)
        {
            builder.Append(" USING ").Append(method);
        }

        var indexColumns = GetIndexColumns(operation);

        var columnsExpression = operation[NpgsqlAnnotationNames.TsVectorConfig] is string tsVectorConfig
            ? ColumnsToTsVector(operation.Name, indexColumns.Select(i => i.Name), tsVectorConfig, model, operation.Schema, operation.Table)
            : IndexColumnList(indexColumns, method);

        builder
            .Append(" (")
            .Append(columnsExpression)
            .Append(")");

        IndexOptions(operation, model, builder);

        if (terminate)
        {
            builder.AppendLine(";");
            // Concurrent indexes cannot be created within a transaction
            EndStatement(builder, suppressTransaction: concurrently);
        }
    }

    /// <inheritdoc />
    protected override void IndexOptions(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (_postgresVersion.AtLeast(11) && operation[NpgsqlAnnotationNames.IndexInclude] is string[] { Length: > 0 } includeColumns)
        {
            builder
                .Append(" INCLUDE (")
                .Append(ColumnList(includeColumns))
                .Append(")");
        }

        if (operation[NpgsqlAnnotationNames.NullsDistinct] is false)
        {
            builder.Append(" NULLS NOT DISTINCT");
        }

        AppendStoreParameters(operation, builder, withLeadingNewline: false);

        base.IndexOptions(operation, model, builder);
    }

    /// <inheritdoc />
    protected override void Generate(EnsureSchemaOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        if (operation.Name == "public")
        {
            return;
        }

        // PostgreSQL has CREATE SCHEMA IF NOT EXISTS, but that requires CREATE privileges on the database even if the schema already
        // exists. This blocks multi-tenant scenarios where the user has no database privileges.
        // So we procedurally check if the schema exists instead, and create it if not.
        var schemaName = operation.Name.Replace("'", "''");

        // If we're generating an idempotent migration, we're already in a PL/PGSQL DO block; otherwise we need to start one.
        if (!Options.HasFlag(MigrationsSqlGenerationOptions.Idempotent))
        {
            builder
                .AppendLine(@"DO $EF$")
                .AppendLine("BEGIN");
        }

        builder
            .AppendLine($"    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = '{schemaName}') THEN")
            .AppendLine($"        CREATE SCHEMA {DelimitIdentifier(operation.Name)};")
            .AppendLine("    END IF;");

        if (!Options.HasFlag(MigrationsSqlGenerationOptions.Idempotent))
        {
            builder.AppendLine("END $EF$;");
        }

        EndStatement(builder);
    }

    /// <inheritdoc />
    protected virtual void Generate(NpgsqlCreateDatabaseOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        builder
            .Append("CREATE DATABASE ")
            .Append(DelimitIdentifier(operation.Name));

        if (!string.IsNullOrEmpty(operation.Template))
        {
            builder
                .AppendLine()
                .Append("TEMPLATE ")
                .Append(DelimitIdentifier(operation.Template));
        }

        if (!string.IsNullOrEmpty(operation.Tablespace))
        {
            builder
                .AppendLine()
                .Append("TABLESPACE ")
                .Append(DelimitIdentifier(operation.Tablespace));
        }

        if (!string.IsNullOrEmpty(operation.Collation))
        {
            builder
                .AppendLine()
                .Append("LC_COLLATE ")
                .Append(DelimitIdentifier(operation.Collation));
        }

        builder.AppendLine(";");

        EndStatement(builder, suppressTransaction: true);
    }

    /// <inheritdoc />
    public virtual void Generate(NpgsqlDropDatabaseOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        var dbName = DelimitIdentifier(operation.Name);

        if (_postgresVersion.AtLeast(13))
        {
            builder.AppendLine($"DROP DATABASE {dbName} WITH (FORCE);");
        }
        else
        {
            builder
                .AppendLine($"REVOKE CONNECT ON DATABASE {dbName} FROM PUBLIC;")
                .AppendLine($"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE datname = '{operation.Name}';")
                .EndCommand(suppressTransaction: true)
                .AppendLine($"DROP DATABASE {dbName};");
        }

        EndStatement(builder, suppressTransaction: true);
    }

    /// <inheritdoc />
    protected override void Generate(
        AlterDatabaseOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        if (operation.Collation != operation.OldDatabase.Collation)
        {
            throw new NotSupportedException("PostgreSQL does not support altering the collation on an existing database.");
        }

        GenerateCollationStatements(operation, model, builder);
        GenerateEnumStatements(operation, model, builder);
        GenerateRangeStatements(operation, model, builder);

        foreach (var extension in operation.GetPostgresExtensions())
        {
            GenerateCreateExtension(extension, model, builder);
        }

        builder.EndCommand();
    }

    /// <inheritdoc />
    protected virtual void GenerateCreateExtension(
        PostgresExtension extension,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        var schema = extension.Schema ?? model?.GetDefaultSchema();

        // Schemas are normally created (or rather ensured) by the model differ, which scans all tables, sequences
        // and other database objects. However, it isn't aware of extensions, so we always ensure schema on enum creation.
        if (schema is not null)
        {
            Generate(new EnsureSchemaOperation { Name = schema }, model, builder);
        }

        builder
            .Append("CREATE EXTENSION IF NOT EXISTS ")
            .Append(DelimitIdentifier(extension.Name));

        if (extension.Schema is not null)
        {
            builder
                .Append(" SCHEMA ")
                .Append(DelimitIdentifier(extension.Schema));
        }

        if (extension.Version is not null)
        {
            builder
                .Append(" VERSION ")
                .Append(DelimitIdentifier(extension.Version));
        }

        builder.AppendLine(";");
    }

    #region Collation management

    /// <inheritdoc />
    protected virtual void GenerateCollationStatements(AlterDatabaseOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        foreach (var collationToCreate in operation.GetPostgresCollations()
                     .Where(ne => operation.GetOldPostgresCollations().All(oe => oe.Name != ne.Name || oe.Schema != ne.Schema)))
        {
            GenerateCreateCollation(collationToCreate, model, builder);
        }

        foreach (var collationToDrop in operation.GetOldPostgresCollations()
                     .Where(oe => operation.GetPostgresCollations().All(ne => ne.Name != oe.Name || oe.Schema != ne.Schema)))
        {
            GenerateDropCollation(collationToDrop, model, builder);
        }

        foreach (var (newCollation, oldCollation) in operation.GetPostgresCollations()
                     .Join(
                         operation.GetOldPostgresCollations(),
                         e => new { e.Name, e.Schema },
                         e => new { e.Name, e.Schema },
                         (ne, oe) => (New: ne, Old: oe)))
        {
            if (newCollation.LcCollate != oldCollation.LcCollate
                || newCollation.LcCtype != oldCollation.LcCtype
                || newCollation.Provider != oldCollation.Provider
                || newCollation.IsDeterministic != oldCollation.IsDeterministic)
            {
                throw new NotSupportedException("Altering an existing collation is not supported.");
            }
        }
    }

    /// <inheritdoc />
    protected virtual void GenerateCreateCollation(PostgresCollation collation, IModel? model, MigrationCommandListBuilder builder)
    {
        var schema = collation.Schema ?? model?.GetDefaultSchema();

        // Schemas are normally created (or rather ensured) by the model differ, which scans all tables, sequences
        // and other database objects. However, it isn't aware of collation, so we always ensure schema on collation creation.
        if (schema is not null)
        {
            Generate(new EnsureSchemaOperation { Name = schema }, model, builder);
        }

        builder
            .Append("CREATE COLLATION ")
            .Append(DelimitIdentifier(collation.Name, schema))
            .Append(" (")
            .IncrementIndent();

        var def = new List<string>();

        if (collation.LcCollate == collation.LcCtype)
        {
            def.Add($"LOCALE = {_stringTypeMapping.GenerateSqlLiteral(collation.LcCollate)}");
        }
        else
        {
            def.Add($"LC_COLLATE = {_stringTypeMapping.GenerateSqlLiteral(collation.LcCollate)}");
            def.Add($"LC_CTYPE = {_stringTypeMapping.GenerateSqlLiteral(collation.LcCtype)}");
        }

        if (collation.Provider is not null)
        {
            def.Add($"PROVIDER = {collation.Provider}");
        }

        if (collation.IsDeterministic is not null)
        {
            def.Add($"DETERMINISTIC = {collation.IsDeterministic}");
        }

        for (var i = 0; i < def.Count; i++)
        {
            builder
                .Append(def[i] + (i == def.Count - 1 ? null : ","))
                .AppendLine();
        }

        builder
            .DecrementIndent()
            .AppendLine(");");
    }

    /// <inheritdoc />
    protected virtual void GenerateDropCollation(PostgresCollation collation, IModel? model, MigrationCommandListBuilder builder)
    {
        var schema = collation.Schema ?? model?.GetDefaultSchema();

        builder
            .Append("DROP COLLATION ")
            .Append(DelimitIdentifier(collation.Name, schema))
            .AppendLine(";");
    }

    #endregion Collation management

    #region Enum management

    /// <inheritdoc />
    protected virtual void GenerateEnumStatements(AlterDatabaseOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        foreach (var enumTypeToCreate in operation.GetPostgresEnums()
                     .Where(ne => operation.GetOldPostgresEnums().All(oe => oe.Name != ne.Name || oe.Schema != ne.Schema)))
        {
            GenerateCreateEnum(enumTypeToCreate, model, builder);
        }

        foreach (var enumTypeToDrop in operation.GetOldPostgresEnums()
                     .Where(oe => operation.GetPostgresEnums().All(ne => ne.Name != oe.Name || oe.Schema != ne.Schema)))
        {
            GenerateDropEnum(enumTypeToDrop, model, builder);
        }

        foreach (var (newEnum, oldEnum) in operation.GetPostgresEnums().OrderBy(e => e.Schema).ThenBy(e => e.Name)
                     .Join(
                         operation.GetOldPostgresEnums().OrderBy(e => e.Schema).ThenBy(e => e.Name),
                         e => new { e.Name, e.Schema },
                         e => new { e.Name, e.Schema },
                         (ne, oe) => (New: ne, Old: oe)))
        {
            var (oldLabels, newLabels) = (oldEnum.Labels, newEnum.Labels);

            // We only support adding enum values - dropping is unsupported by PostgreSQL, and we don't want to
            // go into rename detection heuristics (users can do that in raw SQL).
            // See https://www.postgresql.org/docs/current/sql-altertype.html

            if (oldLabels.Except(newLabels).FirstOrDefault() is { } removedLabel)
            {
                throw new NotSupportedException(
                    $"Can't remove enum label '{removedLabel}' from enum type '{newEnum}'. "
                    + "Renaming a label is possible via a raw SQL migration (see "
                    + "https://www.postgresql.org/docs/current/sql-altertype.html)");
            }

            for (var newPos = 0; newPos < newLabels.Count; newPos++)
            {
                var newLabel = newLabels[newPos];
                if (oldLabels.Contains(newLabel))
                {
                    continue;
                }

                // We add the new label just after the last one we have in the new labels definition (when the last one is new, it will have
                // just been added).
                // If the new label happens to be the first one, add it before the first old label. Otherwise, if there are no old labels,
                // just append the label (no before/after).
                if (newPos == newLabels.Count - 1)
                {
                    GenerateAddEnumLabel(newEnum, newLabel, beforeLabel: null, afterLabel: null, model, builder);
                }
                else if (newPos > 0)
                {
                    GenerateAddEnumLabel(newEnum, newLabel, beforeLabel: null, afterLabel: newLabels[newPos - 1], model, builder);
                }
                else if (oldLabels.Count > 0)
                {
                    GenerateAddEnumLabel(newEnum, newLabel, beforeLabel: oldLabels[0], afterLabel: null, model, builder);
                }
                else
                {
                    GenerateAddEnumLabel(newEnum, newLabel, beforeLabel: null, afterLabel: null, model, builder);
                }
            }
        }
    }

    /// <inheritdoc />
    protected virtual void GenerateCreateEnum(PostgresEnum enumType, IModel? model, MigrationCommandListBuilder builder)
    {
        var schema = enumType.Schema ?? model?.GetDefaultSchema();

        // Schemas are normally created (or rather ensured) by the model differ, which scans all tables, sequences
        // and other database objects. However, it isn't aware of enums, so we always ensure schema on enum creation.
        if (schema is not null)
        {
            Generate(new EnsureSchemaOperation { Name = schema }, model, builder);
        }

        builder
            .Append("CREATE TYPE ")
            .Append(DelimitIdentifier(enumType.Name, schema))
            .Append(" AS ENUM (");

        var labels = enumType.Labels;
        for (var i = 0; i < labels.Count; i++)
        {
            builder.Append(_stringTypeMapping.GenerateSqlLiteral(labels[i]));
            if (i < labels.Count - 1)
            {
                builder.Append(", ");
            }
        }

        builder.AppendLine(");");
    }

    /// <inheritdoc />
    protected virtual void GenerateDropEnum(PostgresEnum enumType, IModel? model, MigrationCommandListBuilder builder)
    {
        var schema = enumType.Schema ?? model?.GetDefaultSchema();

        builder
            .Append("DROP TYPE ")
            .Append(DelimitIdentifier(enumType.Name, schema))
            .AppendLine(";");
    }

    /// <inheritdoc />
    protected virtual void GenerateAddEnumLabel(
        PostgresEnum enumType,
        string addedLabel,
        string? beforeLabel,
        string? afterLabel,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (beforeLabel is not null && afterLabel is not null)
        {
            throw new UnreachableException("Both beforeLabel and afterLabel can't be specified");
        }

        var schema = enumType.Schema ?? model?.GetDefaultSchema();

        builder
            .Append("ALTER TYPE ")
            .Append(DelimitIdentifier(enumType.Name, schema))
            .Append(" ADD VALUE ")
            .Append(_stringTypeMapping.GenerateSqlLiteral(addedLabel));

        if (beforeLabel is not null)
        {
            builder
                .Append(" BEFORE ")
                .Append(_stringTypeMapping.GenerateSqlLiteral(beforeLabel));
        }
        else if (afterLabel is not null)
        {
            builder
                .Append(" AFTER ")
                .Append(_stringTypeMapping.GenerateSqlLiteral(afterLabel));
        }

        builder.AppendLine(";");

        // Adding an enum label cannot be done in a transaction prior to PG12
        if (_postgresVersion.IsUnder(12))
        {
            EndStatement(builder, suppressTransaction: true);
        }
    }

    #endregion Enum management

    #region Range management

    /// <inheritdoc />
    protected virtual void GenerateRangeStatements(AlterDatabaseOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        foreach (var rangeTypeToCreate in operation.GetPostgresRanges()
                     .Where(ne => operation.GetOldPostgresRanges().All(oe => oe.Name != ne.Name)))
        {
            GenerateCreateRange(rangeTypeToCreate, model, builder);
        }

        foreach (var rangeTypeToDrop in operation.GetOldPostgresRanges()
                     .Where(oe => operation.GetPostgresRanges().All(ne => ne.Name != oe.Name)))
        {
            GenerateDropRange(rangeTypeToDrop, model, builder);
        }

        if (operation.GetPostgresRanges().FirstOrDefault(
                nr =>
                    operation.GetOldPostgresRanges().Any(or => or.Name == nr.Name)
            ) is { } rangeTypeToAlter)
        {
            throw new NotSupportedException($"Altering range type ${rangeTypeToAlter} isn't supported.");
        }
    }

    /// <inheritdoc />
    protected virtual void GenerateCreateRange(PostgresRange rangeType, IModel? model, MigrationCommandListBuilder builder)
    {
        var schema = rangeType.Schema ?? model?.GetDefaultSchema();

        // Schemas are normally created (or rather ensured) by the model differ, which scans all tables, sequences
        // and other database objects. However, it isn't aware of ranges, so we always ensure schema on range creation.
        if (schema is not null)
        {
            Generate(new EnsureSchemaOperation { Name = schema }, model, builder);
        }

        builder
            .Append("CREATE TYPE ")
            .Append(DelimitIdentifier(rangeType.Name, schema))
            .AppendLine(" AS RANGE (")
            .IncrementIndent();

        var def = new List<string> { $"SUBTYPE = {rangeType.Subtype}" };
        if (rangeType.CanonicalFunction is not null)
        {
            def.Add($"CANONICAL = {rangeType.CanonicalFunction}");
        }

        if (rangeType.SubtypeOpClass is not null)
        {
            def.Add($"SUBTYPE_OPCLASS = {rangeType.SubtypeOpClass}");
        }

        if (rangeType.CanonicalFunction is not null)
        {
            def.Add($"COLLATION = {rangeType.Collation}");
        }

        if (rangeType.SubtypeDiff is not null)
        {
            def.Add($"SUBTYPE_DIFF = {rangeType.SubtypeDiff}");
        }

        for (var i = 0; i < def.Count; i++)
        {
            builder
                .Append(def[i] + (i == def.Count - 1 ? null : ","))
                .AppendLine();
        }

        builder
            .DecrementIndent()
            .AppendLine(");");
    }

    /// <inheritdoc />
    protected virtual void GenerateDropRange(PostgresRange rangeType, IModel? model, MigrationCommandListBuilder builder)
    {
        var schema = rangeType.Schema ?? model?.GetDefaultSchema();

        builder
            .Append("DROP TYPE ")
            .Append(DelimitIdentifier(rangeType.Name, schema))
            .AppendLine(";");
    }

    #endregion Range management

    /// <inheritdoc />
    protected override void Generate(
        DropIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        builder
            .Append("DROP INDEX ")
            .Append(DelimitIdentifier(operation.Name, operation.Schema));

        if (terminate)
        {
            builder.AppendLine(";");
            EndStatement(builder);
        }
    }

    /// <inheritdoc />
    protected override void Generate(RenameColumnOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        builder.Append("ALTER TABLE ")
            .Append(DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" RENAME COLUMN ")
            .Append(DelimitIdentifier(operation.Name))
            .Append(" TO ")
            .Append(DelimitIdentifier(operation.NewName))
            .AppendLine(";");

        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="InsertDataOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation"> The operation. </param>
    /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
    /// <param name="builder"> The command builder to use to build the commands. </param>
    /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
    protected override void Generate(
        InsertDataOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        var sqlBuilder = new StringBuilder();
        foreach (var modificationCommand in GenerateModificationCommands(operation, model))
        {
            var overridingSystemValue = modificationCommand.ColumnModifications.Any(
                m =>
                    m.Property?.GetValueGenerationStrategy() == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
            ((NpgsqlUpdateSqlGenerator)Dependencies.UpdateSqlGenerator).AppendInsertOperation(
                sqlBuilder,
                modificationCommand,
                0,
                overridingSystemValue,
                out _);
        }

        builder.Append(sqlBuilder.ToString());

        if (terminate)
        {
            builder.EndCommand();
        }
    }

    /// <inheritdoc />
    protected override void Generate(CreateSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));

        if (_postgresVersion.AtLeast(10))
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

    /// <inheritdoc />
    protected override void ColumnDefinition(
        string? schema,
        string table,
        string name,
        ColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        if (operation.ColumnType is null)
        {
            operation.ColumnType = GetColumnType(schema, table, name, operation, model);
        }

        CheckForOldValueGenerationAnnotation(operation);
        var valueGenerationStrategy = operation[NpgsqlAnnotationNames.ValueGenerationStrategy] as NpgsqlValueGenerationStrategy?;
        if (valueGenerationStrategy == NpgsqlValueGenerationStrategy.SerialColumn)
        {
            if (operation.IsNullable)
            {
                throw new NotSupportedException("SERIAL columns can't be nullable");
            }

            switch (operation.ColumnType)
            {
                case "int":
                case "int4":
                case "integer":
                    operation.ColumnType = "serial";
                    break;
                case "bigint":
                case "int8":
                    operation.ColumnType = "bigserial";
                    break;
                case "smallint":
                case "int2":
                    operation.ColumnType = "smallserial";
                    break;
            }
        }

        ApplyTsVectorColumnSql(operation, model, operation.Name, schema, table);

        if (operation.ComputedColumnSql is not null)
        {
            ComputedColumnDefinition(schema, table, name, operation, model, builder);

            return;
        }

        var columnType = operation.ColumnType ?? GetColumnType(schema, table, name, operation, model)!;
        builder
            .Append(DelimitIdentifier(name))
            .Append(" ")
            .Append(columnType);

        if (operation[NpgsqlAnnotationNames.CompressionMethod] is string compressionMethod)
        {
            builder
                .Append(" COMPRESSION ")
                .Append(DelimitIdentifier(compressionMethod));
        }

        // If a collation was defined on the column specifically, via the standard EF mechanism, it will be
        // available in operation.Collation (as usual).
        // If not, there may be a model-wide default column collation, which gets transmitted via the Npgsql-specific annotation.
        // This mechanism is obsolete, and EF Core's bulk model configuration can be used instead; but we continue to support it for
        // backwards compat.
#pragma warning disable CS0618
        var collation = (string?)(operation.Collation ?? operation[NpgsqlAnnotationNames.DefaultColumnCollation]);
#pragma warning restore CS0618
        if (collation is not null)
        {
            builder
                .Append(" COLLATE ")
                .Append(DelimitIdentifier(collation));
        }

        if (valueGenerationStrategy.IsIdentity())
        {
            IdentityDefinition(operation, builder);
        }
        else
        {
            if (!operation.IsNullable)
            {
                builder.Append(" NOT NULL");
            }

            DefaultValue(operation.DefaultValue, operation.DefaultValueSql, columnType, builder);
        }
    }

    /// <inheritdoc />
    protected override void DefaultValue(
        object? defaultValue,
        string? defaultValueSql,
        string? columnType,
        MigrationCommandListBuilder builder)
    {
        // This is a hacky workaround for https://github.com/dotnet/efcore/issues/32353 - the EF MigrationsModelDiffer generates an empty
        // string as the default value for JSON columns, but that's not valid as a JSON document and rejected by PG's jsonb type. So we
        // replace the empty string with an empty JSON document {}.
        // Note that even after the EF-side issue is fixed, removing this hack is a breaking change as migrations have already been
        // scaffolded with an empty string.
        if (columnType is "jsonb" or "json" && defaultValue is "")
        {
            defaultValue = "{}";
        }

        base.DefaultValue(defaultValue, defaultValueSql, columnType, builder);
    }

    /// <summary>
    ///     Checks for a <see cref="NpgsqlAnnotationNames.TsVectorConfig" /> annotation on the given column, and if found, assigns
    ///     the appropriate SQL to <see cref="ColumnOperation.ComputedColumnSql" />.
    /// </summary>
    protected virtual void ApplyTsVectorColumnSql(ColumnOperation column, IModel? model, string name, string? schema, string table)
    {
        if (column[NpgsqlAnnotationNames.TsVectorConfig] is string tsVectorConfig)
        {
            var tsVectorIncludedColumns = column[NpgsqlAnnotationNames.TsVectorProperties] as string[];
            if (tsVectorIncludedColumns is null)
            {
                throw new InvalidOperationException(
                    $"{nameof(NpgsqlAnnotationNames.TsVectorConfig)} is present in a migration but "
                    + $"{nameof(NpgsqlAnnotationNames.TsVectorProperties)} is absent or empty");
            }

            column.ComputedColumnSql = ColumnsToTsVector(name, tsVectorIncludedColumns, tsVectorConfig, model, schema, table);
            column.IsStored = true;

            column.RemoveAnnotation(NpgsqlAnnotationNames.TsVectorConfig);
        }
    }

    // Note: this definition is only used for creating new identity columns, not for alterations.
    /// <inheritdoc />
    protected virtual void IdentityDefinition(
        ColumnOperation operation,
        MigrationCommandListBuilder builder)
    {
        if (operation[NpgsqlAnnotationNames.ValueGenerationStrategy] is not NpgsqlValueGenerationStrategy strategy
            || !strategy.IsIdentity())
        {
            return;
        }

        builder.Append(
            strategy == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                ? " GENERATED BY DEFAULT AS IDENTITY"
                : " GENERATED ALWAYS AS IDENTITY");

        // Handle sequence options for the identity column
        if (operation[NpgsqlAnnotationNames.IdentityOptions] is string identitySequenceOptions)
        {
            // TODO: Potential for refactoring with regular sequences (i.e. calling SequenceOptions),
            // but some complexity to be worked out around creating/altering/restarting

            var sequenceData = IdentitySequenceOptionsData.Deserialize(identitySequenceOptions);

            var optionsWritten = false;

            var incrementBy = sequenceData.IncrementBy;

            var defaultMinValue = incrementBy > 0 ? 1 : Min(operation.ClrType);
            var defaultMaxValue = incrementBy > 0 ? Max(operation.ClrType) : -1;

            var minValue = sequenceData.MinValue ?? defaultMinValue;
            var maxValue = sequenceData.MaxValue ?? defaultMaxValue;

            var defaultStartValue = incrementBy > 0 ? minValue : maxValue;
            if (sequenceData.StartValue.HasValue && sequenceData.StartValue != defaultStartValue)
            {
                Append("START WITH " + sequenceData.StartValue);
            }

            if (incrementBy != 1)
            {
                Append("INCREMENT BY " + incrementBy);
            }

            if (minValue != defaultMinValue)
            {
                Append("MINVALUE " + minValue);
            }

            if (maxValue != defaultMaxValue)
            {
                Append("MAXVALUE " + maxValue);
            }

            if (sequenceData.IsCyclic)
            {
                Append("CYCLE");
            }

            if (sequenceData.NumbersToCache != 1)
            {
                Append("CACHE " + sequenceData.NumbersToCache);
            }

            if (optionsWritten)
            {
                builder.Append(")");
            }

            void Append(string s)
            {
                builder
                    .Append(optionsWritten ? " " : " (")
                    .Append(s);
                optionsWritten = true;
            }

            // Note: in older versions of PostgreSQL there's a slight variation, see NpgsqlDatabaseModelFactory.
            // This is currently only used by identity, which is only supported on PG 10 anyway.
            long Min(Type type)
            {
                if (type == typeof(int))
                {
                    return int.MinValue;
                }

                if (type == typeof(long))
                {
                    return long.MinValue;
                }

                if (type == typeof(short))
                {
                    return short.MinValue;
                }

                throw new ArgumentOutOfRangeException();
            }

            long Max(Type type)
            {
                if (type == typeof(int))
                {
                    return int.MaxValue;
                }

                if (type == typeof(long))
                {
                    return long.MaxValue;
                }

                if (type == typeof(short))
                {
                    return short.MaxValue;
                }

                throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for a computed column definition for the given column metadata.
    /// </summary>
    /// <param name="schema"> The schema that contains the table, or <c>null</c> to use the default schema. </param>
    /// <param name="table"> The table that contains the column. </param>
    /// <param name="name"> The column name. </param>
    /// <param name="operation"> The column metadata. </param>
    /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
    /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
    protected override void ComputedColumnDefinition(
        string? schema,
        string table,
        string name,
        ColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        if (_postgresVersion < new Version(12, 0))
        {
            throw new NotSupportedException("Computed/generated columns aren't supported in PostgreSQL prior to version 12");
        }

        if (operation.IsStored != true)
        {
            throw new NotSupportedException(
                "Generated columns currently must be stored, specify 'stored: true' in "
                + $"'{nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql)}' in your context's OnModelCreating.");
        }

        builder
            .Append(DelimitIdentifier(name))
            .Append(" ")
            .Append(operation.ColumnType ?? GetColumnType(schema, table, name, operation, model)!);

        if (operation.Collation is not null)
        {
            builder
                .Append(" COLLATE ")
                .Append(DelimitIdentifier(operation.Collation));
        }

        builder
            .Append(" GENERATED ALWAYS AS (")
            .Append(operation.ComputedColumnSql!)
            .Append(") STORED");

        if (!operation.IsNullable)
        {
            builder.Append(" NOT NULL");
        }
    }

#pragma warning disable 618
    // Version 1.0 had a bad strategy for expressing serial columns, which depended on a
    // ValueGeneratedOnAdd annotation. Detect that and throw.
    private static void CheckForOldValueGenerationAnnotation(IAnnotatable annotatable)
    {
        if (annotatable.FindAnnotation(NpgsqlAnnotationNames.ValueGeneratedOnAdd) is not null)
        {
            throw new NotSupportedException(
                "The Npgsql:ValueGeneratedOnAdd annotation has been found in your migrations, but is no longer supported. Please replace it with '.Annotation(\"Npgsql:ValueGenerationStrategy\", NpgsqlValueGenerationStrategy.SerialColumn)' where you want PostgreSQL serial (autoincrement) columns, and remove it in all other cases.");
        }
    }
#pragma warning restore 618

    /// <summary>
    ///     Renames a database object such as a table, index, or sequence.
    /// </summary>
    /// <param name="schema">The current schema of the object to rename.</param>
    /// <param name="name">The current name of the object to rename.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="type">The type of the object (e.g. TABLE, INDEX, SEQUENCE).</param>
    /// <param name="builder">The builder to which operations are appended.</param>
    public virtual void Rename(
        string? schema,
        string name,
        string newName,
        string type,
        MigrationCommandListBuilder builder)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotEmpty(newName, nameof(newName));
        Check.NotEmpty(type, nameof(type));
        Check.NotNull(builder, nameof(builder));

        builder
            .Append("ALTER ")
            .Append(type)
            .Append(" ")
            .Append(DelimitIdentifier(name, schema))
            .Append(" RENAME TO ")
            .Append(DelimitIdentifier(newName))
            .AppendLine(";");
    }

    /// <summary>
    ///     Transfers a database object such as a table, index, or sequence between schemas.
    /// </summary>
    /// <param name="newSchema">The new schema.</param>
    /// <param name="schema">The current schema.</param>
    /// <param name="name">The name of the object to transfer.</param>
    /// <param name="type">The type of the object (e.g. TABLE, INDEX, SEQUENCE).</param>
    /// <param name="builder">The builder to which operations are appended.</param>
    public virtual void Transfer(
        string newSchema,
        string? schema,
        string name,
        string type,
        MigrationCommandListBuilder builder)
    {
        Check.NotEmpty(newSchema, nameof(newSchema));
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(type, nameof(type));
        Check.NotNull(builder, nameof(builder));

        builder
            .Append("ALTER ")
            .Append(type)
            .Append(" ")
            .Append(DelimitIdentifier(name, schema))
            .Append(" SET SCHEMA ")
            .Append(DelimitIdentifier(newSchema))
            .AppendLine(";");
    }

    /// <inheritdoc />
    protected virtual void RecreateIndexes(IColumn? column, MigrationOperation currentOperation, MigrationCommandListBuilder builder)
    {
        foreach (var index in GetIndexesToRebuild())
        {
            Generate(CreateIndexOperation.CreateFrom(index), index.Table.Model.Model, builder, terminate: false);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        IEnumerable<ITableIndex> GetIndexesToRebuild()
        {
            if (column == null)
            {
                yield break;
            }

            var table = column.Table;
            var createIndexOperations = _operations.SkipWhile(o => o != currentOperation).Skip(1)
                .OfType<CreateIndexOperation>().Where(o => o.Table == table.Name && o.Schema == table.Schema).ToList();
            foreach (var index in table.Indexes)
            {
                var indexName = index.Name;
                if (createIndexOperations.Any(o => o.Name == indexName))
                {
                    continue;
                }

                if (index.Columns.Any(c => c == column))
                {
                    yield return index;
                }
                else if (index[NpgsqlAnnotationNames.IndexInclude] is IReadOnlyList<string> includeColumns
                         && includeColumns.Contains(column.Name))
                {
                    yield return index;
                }
            }
        }
    }

    #endregion Utilities

    #region System column utilities

    private bool IsSystemColumn(string name)
        => name == "oid" && _postgresVersion.IsUnder(12) || SystemColumnNames.Contains(name);

    /// <summary>
    ///     Tables in PostgreSQL implicitly have a set of system columns, which are always there.
    ///     We want to allow users to access these columns (i.e. xmin for optimistic concurrency) but
    ///     they should never generate migration operations.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/ddl-system-columns.html
    /// </remarks>
    private static readonly string[] SystemColumnNames = ["tableoid", "xmin", "cmin", "xmax", "cmax", "ctid"];

    #endregion System column utilities

    #region Storage parameter utilities

    private void AppendStoreParameters(Annotatable annotatable, MigrationCommandListBuilder builder, bool withLeadingNewline)
    {
        var storageParameters = GetStorageParameters(annotatable);
        if (storageParameters.Count > 0)
        {
            if (withLeadingNewline)
            {
                builder.AppendLine();
            }
            else
            {
                builder.Append(" ");
            }

            builder
                .Append("WITH (")
                .Append(string.Join(", ", storageParameters.Select(p => $"{p.Key}={p.Value}")))
                .Append(")");
        }
    }

    private Dictionary<string, string> GetStorageParameters(Annotatable annotatable)
        => annotatable.GetAnnotations()
            .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal))
            .ToDictionary(
                a => a.Name.Substring(NpgsqlAnnotationNames.StorageParameterPrefix.Length),
                a => a.Value switch
                {
                    bool b => b ? "true" : "false",
                    string s => $"'{s}'",
                    _ => a.Value!.ToString()!
                });

    // TODO: Call this for AlterIndexOperation when that's added (https://github.com/dotnet/efcore/issues/20692)
    private bool AppendStorageParameterAlterations(
        Annotatable oldAnnotatable,
        Annotatable newAnnotatable,
        string alterBaseSql,
        MigrationCommandListBuilder builder)
    {
        var madeChanges = false;

        var oldStorageParameters = GetStorageParameters(oldAnnotatable);
        var newStorageParameters = GetStorageParameters(newAnnotatable);

        var newOrChanged = newStorageParameters.Where(
                p =>
                    !oldStorageParameters.ContainsKey(p.Key) || oldStorageParameters[p.Key] != p.Value)
            .ToList();

        if (newOrChanged.Count > 0)
        {
            builder
                .Append(alterBaseSql)
                .Append(" SET (")
                .Append(string.Join(", ", newOrChanged.Select(p => $"{p.Key}={p.Value}")))
                .Append(")");

            builder.AppendLine(";");
            madeChanges = true;
        }

        var removed = oldStorageParameters
            .Select(p => p.Key)
            .Where(pn => !newStorageParameters.ContainsKey(pn))
            .ToList();

        if (removed.Count > 0)
        {
            builder
                .Append(alterBaseSql)
                .Append(" RESET (")
                .Append(string.Join(", ", removed))
                .Append(")");

            builder.AppendLine(";");
            madeChanges = true;
        }

        return madeChanges;
    }

    #endregion Storage parameter utilities

    #region Helpers

    private string DelimitIdentifier(string identifier)
        => Dependencies.SqlGenerationHelper.DelimitIdentifier(identifier);

    private string DelimitIdentifier(string name, string? schema)
        => Dependencies.SqlGenerationHelper.DelimitIdentifier(name, schema);

    private string IndexColumnList(IndexColumn[] columns, string? method)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < columns.Length; i++)
        {
            var column = columns[i];

            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(DelimitIdentifier(column.Name));

            if (!string.IsNullOrEmpty(column.Collation))
            {
                builder.Append(" COLLATE ").Append(DelimitIdentifier(column.Collation));
            }

            if (!string.IsNullOrEmpty(column.Operator))
            {
                var delimitedOperator = TryParseSchema(column.Operator, out var name, out var schema)
                    ? DelimitIdentifier(name, schema)
                    : DelimitIdentifier(column.Operator);

                builder.Append(" ").Append(delimitedOperator);
            }

            // Of the built-in access methods, only btree (the default) supports
            // sorting, thus we only want to emit sort options for btree indexes.
            if (method is null or "btree")
            {
                if (column.IsDescending)
                {
                    builder.Append(" DESC");
                }

                if (column.NullSortOrder != NullSortOrder.Unspecified)
                {
                    builder.Append(" NULLS ");

                    switch (column.NullSortOrder)
                    {
                        case NullSortOrder.NullsFirst:
                            builder.Append("FIRST");
                            break;
                        case NullSortOrder.NullsLast:
                            builder.Append("LAST");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        return builder.ToString();
    }

    private string ColumnsToTsVector(
        string columnOrIndexName,
        IEnumerable<string> columnNames,
        string tsVectorConfig,
        IModel? model,
        string? schema,
        string table)
    {
        var columns = columnNames
            .Select(columnName => model?.GetRelationalModel().FindTable(table, schema)?.Columns.FirstOrDefault(c => c.Name == columnName))
            .ToArray();

        IEnumerable<IGrouping<string, IColumn>> columnGroups = columns
            .GroupBy(
                c => c?.StoreType switch
                {
                    "json" => "json",
                    "jsonb" => "jsonb",
                    null => "null",
                    _ => "text"

                    // Note: we currently don't support array_to_tsvector since it doesn't accept a search configuration
                })!;

        var tsVectorConfigLiteral = _stringTypeMapping.GenerateSqlLiteral(tsVectorConfig);

        var builder = new StringBuilder();

        foreach (var columnGroup in columnGroups)
        {
            if (builder.Length > 0)
            {
                builder.Append(" || ");
            }

            builder.Append(
                columnGroup.Key switch
                {
                    "text" => $"to_tsvector({tsVectorConfigLiteral}, {string.Join(" || ' ' || ", columnGroup.Select(TextColumn))})",
                    "json" => string.Join(
                        " || ", columnGroup.Select(
                            c =>
                                $"""json_to_tsvector({tsVectorConfigLiteral}, {JsonColumn(c)}, '"all"')""")),
                    "jsonb" => string.Join(
                        " || ", columnGroup.Select(
                            c =>
                                $"""jsonb_to_tsvector({tsVectorConfigLiteral}, {JsonColumn(c)}, '"all"')""")),
                    "null" => throw new InvalidOperationException(
                        $"Column or index {columnOrIndexName} refers to unknown column in tsvector definition"),
                    _ => throw new ArgumentOutOfRangeException()
                });
        }

        return builder.ToString();

        string TextColumn(IColumn column)
            => column.IsNullable ? $"coalesce({DelimitIdentifier(column.Name)}, '')" : DelimitIdentifier(column.Name);

        string JsonColumn(IColumn column)
            => column.IsNullable ? $"coalesce({DelimitIdentifier(column.Name)}, '{{}}')" : DelimitIdentifier(column.Name);
    }

    private static bool TryParseSchema(string identifier, out string name, out string? schema)
    {
        var index = identifier.IndexOf('.');

        if (index >= 0)
        {
            schema = identifier.Substring(0, index);
            name = identifier.Substring(index + 1);
            return true;
        }

        name = identifier;
        schema = default;
        return false;
    }

    private static IndexColumn[] GetIndexColumns(CreateIndexOperation operation)
    {
        var collations = operation[RelationalAnnotationNames.Collation] as string[];

        var operators = operation[NpgsqlAnnotationNames.IndexOperators] as string[];

        // We used to have our own annotation-based descending index mechanism, this got replaced with IsDescending in EF Core 7.0.
        var isDescendingValues = operation.IsDescending;
        var legacySortOrders = operation[NpgsqlAnnotationNames.IndexSortOrder] as SortOrder[];

        var nullSortOrders = operation[NpgsqlAnnotationNames.IndexNullSortOrder] as NullSortOrder[];

        var columns = new IndexColumn[operation.Columns.Length];

        for (var i = 0; i < columns.Length; i++)
        {
            var name = operation.Columns[i];
            var @operator = i < operators?.Length ? operators[i] : null;
            var collation = i < collations?.Length ? collations[i] : null;
            var isColumnDescending = isDescendingValues is not null
                ? isDescendingValues.Length == 0 || isDescendingValues[i]
                : i < legacySortOrders?.Length && legacySortOrders[i] == SortOrder.Descending;
            var nullSortOrder = i < nullSortOrders?.Length ? nullSortOrders[i] : NullSortOrder.Unspecified;

            columns[i] = new IndexColumn(name, @operator, collation, isColumnDescending, nullSortOrder);
        }

        return columns;
    }

    private readonly struct IndexColumn(string name, string? @operator, string? collation, bool isDescending, NullSortOrder nullSortOrder)
    {
        public string Name { get; } = name;
        public string? Operator { get; } = @operator;
        public string? Collation { get; } = collation;
        public bool IsDescending { get; } = isDescending;
        public NullSortOrder NullSortOrder { get; } = nullSortOrder;
    }

    #endregion
}
