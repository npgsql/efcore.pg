using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

// Migrator is EF-pubternal, but overriding it is the only way to force Npgsql to ReloadTypes() after executing a migration which adds
// types to the database
#pragma warning disable EF1001

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlMigrator : Migrator
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IRelationalConnection _connection;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlMigrator(
        IMigrationsAssembly migrationsAssembly,
        IHistoryRepository historyRepository,
        IDatabaseCreator databaseCreator,
        IMigrationsSqlGenerator migrationsSqlGenerator,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IMigrationCommandExecutor migrationCommandExecutor,
        IRelationalConnection connection,
        ISqlGenerationHelper sqlGenerationHelper,
        ICurrentDbContext currentContext,
        IModelRuntimeInitializer modelRuntimeInitializer,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger,
        IRelationalCommandDiagnosticsLogger commandLogger,
        IDatabaseProvider databaseProvider,
        IMigrationsModelDiffer migrationsModelDiffer,
        IDesignTimeModel designTimeModel,
        IDbContextOptions contextOptions,
        IExecutionStrategy executionStrategy)
        : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder,
            migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, modelRuntimeInitializer, logger,
            commandLogger, databaseProvider, migrationsModelDiffer, designTimeModel, contextOptions, executionStrategy)
    {
        _historyRepository = historyRepository;
        _connection = connection;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Migrate(string? targetMigration)
    {
        var appliedMigrations = _historyRepository.GetAppliedMigrations();

        base.Migrate(targetMigration);

        PopulateMigrations(
            appliedMigrations.Select(t => t.MigrationId),
            targetMigration,
            out var migratorData);

        if (migratorData.RevertedMigrations.Count + migratorData.AppliedMigrations.Count == 0)
        {
            return;
        }

        // If a PostgreSQL extension, enum or range was added, we want Npgsql to reload all types at the ADO.NET level.
        var migrations = migratorData.AppliedMigrations.Count > 0 ? migratorData.AppliedMigrations : migratorData.RevertedMigrations;
        var reloadTypes = migrations
            .SelectMany(m => m.UpOperations)
            .OfType<AlterDatabaseOperation>()
            .Any(o => o.GetPostgresExtensions().Any() || o.GetPostgresEnums().Any() || o.GetPostgresRanges().Any());

        if (reloadTypes && _connection.DbConnection is NpgsqlConnection npgsqlConnection)
        {
            _connection.Open();
            try
            {
                npgsqlConnection.ReloadTypes();
            }
            finally
            {
                _connection.Close();
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override async Task MigrateAsync(string? targetMigration, CancellationToken cancellationToken = default)
    {
        var appliedMigrations = await _historyRepository.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false);

        await base.MigrateAsync(targetMigration, cancellationToken).ConfigureAwait(false);

        PopulateMigrations(
            appliedMigrations.Select(t => t.MigrationId),
            targetMigration,
            out var migratorData);

        if (migratorData.RevertedMigrations.Count + migratorData.AppliedMigrations.Count == 0)
        {
            return;
        }

        // If a PostgreSQL extension, enum or range was added, we want Npgsql to reload all types at the ADO.NET level.
        var migrations = migratorData.AppliedMigrations.Count > 0 ? migratorData.AppliedMigrations : migratorData.RevertedMigrations;
        var reloadTypes = migrations
            .SelectMany(m => m.UpOperations)
            .OfType<AlterDatabaseOperation>()
            .Any(o => o.GetPostgresExtensions().Any() || o.GetPostgresEnums().Any() || o.GetPostgresRanges().Any());

        if (reloadTypes && _connection.DbConnection is NpgsqlConnection npgsqlConnection)
        {
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await npgsqlConnection.ReloadTypesAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await _connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
