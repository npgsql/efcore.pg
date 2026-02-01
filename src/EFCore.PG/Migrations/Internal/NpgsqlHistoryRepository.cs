using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlHistoryRepository : HistoryRepository, IHistoryRepository
{
    private IModel? _model;

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
        => throw new UnreachableException(
            "We should not be checking for the existence of the history table, but rather creating it and catching exceptions (see below)");

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
        // being created.
        var model = EnsureModel();

        var operations = Dependencies.ModelDiffer.GetDifferences(null, model.GetRelationalModel());
        var commandList = Dependencies.MigrationsSqlGenerator.Generate(operations, model);
        return commandList;
    }

    private IModel EnsureModel()
    {
        if (_model == null)
        {
            var conventionSet = Dependencies.ConventionSetBuilder.CreateConventionSet();

            conventionSet.Remove(typeof(DbSetFindingConvention));
            conventionSet.Remove(typeof(RelationalDbFunctionAttributeConvention));
            // TODO: this whole method exists only so we can remove this convention (https://github.com/dotnet/efcore/issues/34991)
            conventionSet.Remove(typeof(NpgsqlPostgresModelFinalizingConvention));

            var modelBuilder = new ModelBuilder(conventionSet);
            modelBuilder.Entity<HistoryRow>(
                x =>
                {
                    ConfigureTable(x);
                    x.ToTable(TableName, TableSchema);
                });

            _model = Dependencies.ModelRuntimeInitializer.Initialize(
                (IModel)modelBuilder.Model, designTime: true, validationLogger: null);
        }

        return _model;
    }

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
        catch (PostgresException e) when (e.SqlState is PostgresErrorCodes.UniqueViolation
                                              or PostgresErrorCodes.DuplicateTable
                                              or PostgresErrorCodes.DuplicateObject)
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
        catch (PostgresException e) when (e.SqlState is PostgresErrorCodes.UniqueViolation
                                              or PostgresErrorCodes.DuplicateTable
                                              or PostgresErrorCodes.DuplicateObject)
        {
            return false;
        }
    }

    private IReadOnlyList<MigrationCommand> GetCreateIfNotExistsCommands()
        => Dependencies.MigrationsSqlGenerator.Generate([new SqlOperation
        {
            Sql = GetCreateIfNotExistsScript()
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

    /// <summary>
    ///     Calls the base implementation, but catches "table not found" exceptions; we do this rather than try to detect whether the
    ///     migration table already exists (see <see cref="ExistsAsync" /> override below), since it's difficult to reliably check if the
    ///     migration history table exists or not (because user may set PG <c>search_path</c>, which determines unqualified tables
    ///     references when creating, selecting).
    /// </summary>
    public override IReadOnlyList<HistoryRow> GetAppliedMigrations()
    {
        try
        {
            return base.GetAppliedMigrations();
        }
        catch (PostgresException e) when (e.SqlState is PostgresErrorCodes.InvalidCatalogName or PostgresErrorCodes.UndefinedTable)
        {
            return [];
        }
    }

    /// <summary>
    ///     Calls the base implementation, but catches "table not found" exceptions; we do this rather than try to detect whether the
    ///     migration table already exists (see <see cref="ExistsAsync" /> override below), since it's difficult to reliably check if the
    ///     migration history table exists or not (because user may set PG <c>search_path</c>, which determines unqualified tables
    ///     references when creating, selecting).
    /// </summary>
    public override async Task<IReadOnlyList<HistoryRow>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (PostgresException e) when (e.SqlState is PostgresErrorCodes.InvalidCatalogName or PostgresErrorCodes.UndefinedTable)
        {
            return [];
        }
    }

    /// <summary>
    ///     Always returns <see langword="true" /> for PostgreSQL - it's difficult to reliably check if the migration history table
    ///     exists or not (because user may set PG <c>search_path</c>, which determines unqualified tables references when creating,
    ///     selecting). So we instead catch the "table doesn't exist" exceptions instead.
    /// </summary>
    public override bool Exists()
        => true;

    /// <summary>
    ///     Always returns <see langword="true" /> for PostgreSQL - it's difficult to reliably check if the migration history table
    ///     exists or not (because user may set PG <c>search_path</c>, which determines unqualified tables references when creating,
    ///     selecting). So we instead catch the "table doesn't exist" exceptions instead.
    /// </summary>
    public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(true);

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
