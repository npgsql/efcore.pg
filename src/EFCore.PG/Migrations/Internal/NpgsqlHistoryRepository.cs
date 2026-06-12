using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

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
