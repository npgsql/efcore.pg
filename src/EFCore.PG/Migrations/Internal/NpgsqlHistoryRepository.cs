namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlHistoryRepository : HistoryRepository, IHistoryRepository
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
            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            return
                $"""
SELECT EXISTS (
    SELECT 1 FROM pg_catalog.pg_class c
    JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
    WHERE n.nspname={stringTypeMapping.GenerateSqlLiteral(TableSchema ?? "public")} AND
          c.relname={stringTypeMapping.GenerateSqlLiteral(TableName)}
)
""";
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

    bool IHistoryRepository.CreateIfNotExists()
    {
        // In PG, doing CREATE TABLE IF NOT EXISTS isn't concurrency-safe, and can result a "duplicate table" error or in a unique
        // constraint violation (duplicate key value violates unique constraint "pg_type_typname_nsp_index").
        // We catch this and report that the table wasn't created.
        try
        {
            return Dependencies.MigrationCommandExecutor.ExecuteNonQuery(
                    GetCreateIfNotExistsCommands(), Dependencies.Connection, new MigrationExecutionState(), commitTransaction: true)
                != 0;
        }
        catch (PostgresException e) when (e.SqlState is "23505" or "42P07")
        {
            return false;
        }
    }

    async Task<bool> IHistoryRepository.CreateIfNotExistsAsync(CancellationToken cancellationToken)
    {
        // In PG, doing CREATE TABLE IF NOT EXISTS isn't concurrency-safe, and can result a "duplicate table" error or in a unique
        // constraint violation (duplicate key value violates unique constraint "pg_type_typname_nsp_index").
        // We catch this and report that the table wasn't created.
        try
        {
            return (await Dependencies.MigrationCommandExecutor.ExecuteNonQueryAsync(
                    GetCreateIfNotExistsCommands(), Dependencies.Connection, new MigrationExecutionState(), commitTransaction: true,
                    cancellationToken: cancellationToken).ConfigureAwait(false))
                != 0;
        }
        catch (PostgresException e) when (e.SqlState is "23505" or "42P07")
        {
            return false;
        }
    }

    private IReadOnlyList<MigrationCommand> GetCreateIfNotExistsCommands()
        => Dependencies.MigrationsSqlGenerator.Generate([new SqlOperation
        {
            Sql = GetCreateIfNotExistsScript(),
            SuppressTransaction = true
        }]);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetCreateIfNotExistsScript()
    {
        var script = GetCreateScript();
        return script.Replace("CREATE TABLE", "CREATE TABLE IF NOT EXISTS");
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
