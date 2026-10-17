using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlHistoryRepository : HistoryRepository
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlHistoryRepository(HistoryRepositoryDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override LockReleaseBehavior LockReleaseBehavior => LockReleaseBehavior.Transaction;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IMigrationsDatabaseLock AcquireDatabaseLock()
    {
        Dependencies.MigrationsLogger.AcquiringMigrationLock();

        Dependencies.RawSqlCommandBuilder
            .Build($"LOCK TABLE {Dependencies.SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema)} IN ACCESS EXCLUSIVE MODE")
            .ExecuteNonQuery(CreateRelationalCommandParameters());

        return new NpgsqlMigrationDatabaseLock(this);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override async Task<IMigrationsDatabaseLock> AcquireDatabaseLockAsync(CancellationToken cancellationToken = default)
    {
        await Dependencies.RawSqlCommandBuilder
            .Build($"LOCK TABLE {Dependencies.SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema)} IN ACCESS EXCLUSIVE MODE")
            .ExecuteNonQueryAsync(CreateRelationalCommandParameters(), cancellationToken)
            .ConfigureAwait(false);

        return new NpgsqlMigrationDatabaseLock(this);
    }

    private RelationalCommandParameterObject CreateRelationalCommandParameters()
        => new(
            Dependencies.Connection,
            null,
            null,
            Dependencies.CurrentContext.Context,
            Dependencies.CommandLogger, CommandSource.Migrations);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string ExistsSql
    {
        get
        {
            var tableName = Dependencies.TypeMappingSource.GetMapping(typeof(string))
                .GenerateSqlLiteral(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema));

            return $"SELECT pg_catalog.to_regclass({tableName}) IS NOT NULL";
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool InterpretExistsResult(object? value)
        => (bool?)value == true;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IReadOnlyList<MigrationCommand> GetCreateCommands()
    {
        // TODO: This is all a hack around https://github.com/dotnet/efcore/issues/34991: we have provider-specific conventions which add
        // enums and extensions to the model, and the default EF logic causes them to be created at this point, when the history table is
        // being created. This causes problems when:
        // (a) we create an enum here when creating the history table, and then try to create it again when the actual migration
        //     runs (#3324), and
        // (b) we shouldn't be creating extensions at this early point either, and doing so can cause issues (e.g. #3496).
        //
        // So we filter out any extension/enum migration operations.
        // Note that the approach in EF is to remove specific conventions (e.g. DbSetFindingConvention), but we don't want to hardcode
        // specific conventions here; for example, the NetTopologySuite plugin has its NpgsqlNetTopologySuiteExtensionAddingConvention
        // which adds PostGIS. So we just filter out the annotations on the operations themselves.
#pragma warning disable EF1001 // Internal EF Core API usage.
        var model = EnsureModel();
#pragma warning restore EF1001 // Internal EF Core API usage.

        var operations = Dependencies.ModelDiffer.GetDifferences(null, model.GetRelationalModel());

        foreach (var operation in operations)
        {
            if (operation is not AlterDatabaseOperation alterDatabaseOperation)
            {
                continue;
            }

            foreach (var annotation in alterDatabaseOperation.GetAnnotations())
            {
                if (annotation.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal)
                    || annotation.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal))
                {
                    alterDatabaseOperation.RemoveAnnotation(annotation.Name);
                }
            }
        }

        return Dependencies.MigrationsSqlGenerator.Generate(operations, model);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetCreateIfNotExistsScript()
    {
        var tableName = Dependencies.TypeMappingSource.GetMapping(typeof(string))
            .GenerateSqlLiteral(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema));

        // CREATE TABLE IF NOT EXISTS isn't concurrency-safe: concurrent creation can raise duplicate table/object or catalog
        // uniqueness errors. Handle these inside PL/pgSQL exception blocks so only the failed attempt is rolled back.
        // Catching in .NET would leave an enclosing transaction aborted and still log a failed command in EF.
        var builder = new IndentedStringBuilder().AppendLine("BEGIN");

        using (builder.Indent())
        {
            builder.AppendLine($"IF pg_catalog.to_regclass({tableName}) IS NULL THEN");

            using (builder.Indent())
            {
                // Handle commands separately so a schema creation race doesn't skip table creation.
                foreach (var command in GetCreateCommands())
                {
                    builder.AppendLine("BEGIN");
                    using (builder.Indent())
                    {
                        // Preserve newlines inside quoted identifiers.
                        builder.AppendLine(command.CommandText.Replace("CREATE TABLE", "CREATE TABLE IF NOT EXISTS").TrimEnd());
                    }

                    builder.AppendLine("EXCEPTION WHEN duplicate_schema OR duplicate_table OR duplicate_object OR unique_violation THEN");
                    using (builder.Indent())
                    {
                        // Refresh catalog caches after waiting for a concurrent creator; otherwise table creation can still see
                        // a cached missing schema.
                        builder.AppendLine("PERFORM 1 FROM pg_catalog.pg_class LIMIT 1;");
                    }

                    builder.AppendLine("END;");
                }
            }

            builder.AppendLine("END IF;");
        }

        var body = builder.Append("END").ToString();
        var delimiter = "$EFHistory$";
        for (var suffix = 1; body.Contains(delimiter, StringComparison.Ordinal); suffix++)
        {
            delimiter = $"$EFHistory{suffix}$";
        }

        return $"DO {delimiter}{Environment.NewLine}{body} {delimiter};{Environment.NewLine}";
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetBeginIfNotExistsScript(string migrationId)
        => $"""

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM {SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema)} WHERE "{MigrationIdColumnName}" = '{migrationId}') THEN
""";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetBeginIfExistsScript(string migrationId)
        => $"""
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM {SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema)} WHERE "{MigrationIdColumnName}" = '{migrationId}') THEN
""";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetEndIfScript()
        => """
    END IF;
END $EF$;
""";

    private sealed class NpgsqlMigrationDatabaseLock(IHistoryRepository historyRepository) : IMigrationsDatabaseLock
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IHistoryRepository HistoryRepository => historyRepository;

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
            => default;
    }
}
