using System.Net.Sockets;
using System.Transactions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlDatabaseCreator(
        RelationalDatabaseCreatorDependencies dependencies,
        INpgsqlRelationalConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder)
    : RelationalDatabaseCreator(dependencies)
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Create()
    {
        using (var masterConnection = connection.CreateAdminConnection())
        {
            try
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateCreateOperations(), masterConnection);
            }
            catch (PostgresException e) when (e is { SqlState: "23505", ConstraintName: "pg_database_datname_index" })
            {
                // This occurs when two connections are trying to create the same database concurrently
                // (happens in the tests). Simply ignore the error.
            }

            ClearPool();
        }

        Exists();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override async Task CreateAsync(CancellationToken cancellationToken = default)
    {
        var masterConnection = connection.CreateAdminConnection();
        await using (masterConnection.ConfigureAwait(false))
        {
            try
            {
                await Dependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (PostgresException e) when (e is { SqlState: "23505", ConstraintName: "pg_database_datname_index" })
            {
                // This occurs when two connections are trying to create the same database concurrently
                // (happens in the tests). Simply ignore the error.
            }

            ClearPool();
        }

        await ExistsAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool HasTables()
        => Dependencies.ExecutionStrategy
            .Execute(
                connection,
                connection => (bool)CreateHasTablesCommand()
                    .ExecuteScalar(
                        new RelationalCommandParameterObject(
                            connection,
                            null,
                            null,
                            Dependencies.CurrentContext.Context,
                            Dependencies.CommandLogger))!);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
        => Dependencies.ExecutionStrategy.ExecuteAsync(
            connection,
            async (connection, ct) => (bool)(await CreateHasTablesCommand()
                .ExecuteScalarAsync(
                    new RelationalCommandParameterObject(
                        connection,
                        null,
                        null,
                        Dependencies.CurrentContext.Context,
                        Dependencies.CommandLogger),
                    cancellationToken: ct).ConfigureAwait(false))!, cancellationToken);

    private IRelationalCommand CreateHasTablesCommand()
        => rawSqlCommandBuilder
            .Build(
                """
SELECT CASE WHEN COUNT(*) = 0 THEN FALSE ELSE TRUE END
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
WHERE
        cls.relkind IN ('r', 'v', 'm', 'f', 'p') AND
        ns.nspname NOT IN ('pg_catalog', 'information_schema') AND
        -- Exclude tables which are members of PG extensions
        NOT EXISTS (
            SELECT 1 FROM pg_depend WHERE
                classid=(
                    SELECT cls.oid
                    FROM pg_class AS cls
                             JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
                    WHERE relname='pg_class' AND ns.nspname='pg_catalog'
                ) AND
                objid=cls.oid AND
                deptype IN ('e', 'x')
        )
""");

    private IReadOnlyList<MigrationCommand> CreateCreateOperations()
    {
        var designTimeModel = Dependencies.CurrentContext.Context.GetService<IDesignTimeModel>().Model;

        return Dependencies.MigrationsSqlGenerator.Generate(
            new[]
            {
                new NpgsqlCreateDatabaseOperation
                {
                    Name = connection.DbConnection.Database,
                    Template = designTimeModel.GetDatabaseTemplate(),
                    Collation = designTimeModel.GetCollation(),
                    Tablespace = designTimeModel.GetTablespace()
                }
            });
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Exists()
        => Dependencies.ExecutionStrategy.Execute(() =>
        {
            using var _ = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
            var opened = false;

            try
            {
                connection.Open(errorsExpected: true);
                opened = true;

                rawSqlCommandBuilder
                    .Build("SELECT 1")
                    .ExecuteNonQuery(
                        new RelationalCommandParameterObject(
                            connection,
                            parameterValues: null,
                            readerColumns: null,
                            Dependencies.CurrentContext.Context,
                            Dependencies.CommandLogger,
                            CommandSource.Migrations));

                return true;
            }
            catch (Exception e) when (IsDoesNotExist(e))
            {
                return false;
            }
            finally
            {
                if (opened)
                {
                    connection.Close();
                }
            }
        });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        => Dependencies.ExecutionStrategy.ExecuteAsync(async ct =>
        {
            using var _ = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
            var opened = false;

            try
            {
                await connection.OpenAsync(cancellationToken, errorsExpected: true).ConfigureAwait(false);
                opened = true;

                await rawSqlCommandBuilder
                    .Build("SELECT 1")
                    .ExecuteNonQueryAsync(
                        new RelationalCommandParameterObject(
                            connection,
                            parameterValues: null,
                            readerColumns: null,
                            Dependencies.CurrentContext.Context,
                            Dependencies.CommandLogger,
                            CommandSource.Migrations),
                        cancellationToken)
                    .ConfigureAwait(false);

                return true;
            }
            catch (Exception e) when (IsDoesNotExist(e))
            {
                return false;
            }
            finally
            {
                if (opened)
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }
        }, cancellationToken);

    private static bool IsDoesNotExist(Exception exception)
        => exception switch
        {
            // Login failed is thrown when database does not exist (See Issue #776)
            PostgresException { SqlState: "3D000" }
                => true,

            // This can happen when Npgsql attempts to connect to multiple hosts
            NpgsqlException { InnerException: AggregateException ae } when ae.InnerExceptions.Any(ie => ie is PostgresException { SqlState: "3D000" })
                => true,

            // Pretty awful hack around #104
            NpgsqlException { InnerException: IOException { InnerException: SocketException { SocketErrorCode: SocketError.ConnectionReset } } }
                => true,

            _ => false
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Delete()
    {
        ClearAllPools();

        using (var masterConnection = connection.CreateAdminConnection())
        {
            Dependencies.MigrationCommandExecutor
                .ExecuteNonQuery(CreateDropCommands(), masterConnection);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        ClearAllPools();

        var masterConnection = connection.CreateAdminConnection();
        await using (masterConnection)
        {
            await Dependencies.MigrationCommandExecutor
                .ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void CreateTables()
    {
        var designTimeModel = Dependencies.CurrentContext.Context.GetService<IDesignTimeModel>().Model;
        var operations = Dependencies.ModelDiffer.GetDifferences(null, designTimeModel.GetRelationalModel());
        var commands = Dependencies.MigrationsSqlGenerator.Generate(operations, designTimeModel);

        // If a PostgreSQL extension, enum or range was added, we want Npgsql to reload all types at the ADO.NET level.
        var reloadTypes =
            operations.OfType<AlterDatabaseOperation>()
                .Any(
                    o =>
                        o.GetPostgresExtensions().Any() || o.GetPostgresEnums().Any() || o.GetPostgresRanges().Any());

        try
        {
            Dependencies.MigrationCommandExecutor.ExecuteNonQuery(commands, connection);
        }
        catch (PostgresException e) when (e is { SqlState: "23505", ConstraintName: "pg_type_typname_nsp_index" })
        {
            // This occurs when two connections are trying to create the same database concurrently
            // (happens in the tests). Simply ignore the error.
        }

        if (reloadTypes && connection.DbConnection is NpgsqlConnection npgsqlConnection)
        {
            npgsqlConnection.Open();
            try
            {
                npgsqlConnection.ReloadTypes();
            }
            catch
            {
                npgsqlConnection.Close();
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override async Task CreateTablesAsync(CancellationToken cancellationToken = default)
    {
        var designTimeModel = Dependencies.CurrentContext.Context.GetService<IDesignTimeModel>().Model;
        var operations = Dependencies.ModelDiffer.GetDifferences(null, designTimeModel.GetRelationalModel());
        var commands = Dependencies.MigrationsSqlGenerator.Generate(operations, designTimeModel);

        try
        {
            await Dependencies.MigrationCommandExecutor.ExecuteNonQueryAsync(commands, connection, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (PostgresException e) when (e is { SqlState: "23505", ConstraintName: "pg_type_typname_nsp_index" })
        {
            // This occurs when two connections are trying to create the same database concurrently
            // (happens in the tests). Simply ignore the error.
        }

        // If a PostgreSQL extension, enum or range was added, we want Npgsql to reload all types at the ADO.NET level.
        var reloadTypes = operations
            .OfType<AlterDatabaseOperation>()
            .Any(o => o.GetPostgresExtensions().Any() || o.GetPostgresEnums().Any() || o.GetPostgresRanges().Any());

        if (reloadTypes && connection.DbConnection is NpgsqlConnection npgsqlConnection)
        {
            await npgsqlConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await npgsqlConnection.ReloadTypesAsync().ConfigureAwait(false);
            }
            catch
            {
                await npgsqlConnection.CloseAsync().ConfigureAwait(false);
            }
        }
    }

    private IReadOnlyList<MigrationCommand> CreateDropCommands()
    {
        var operations = new MigrationOperation[]
        {
            // TODO Check DbConnection.Database always gives us what we want
            // Issue #775
            new NpgsqlDropDatabaseOperation { Name = connection.DbConnection.Database }
        };

        return Dependencies.MigrationsSqlGenerator.Generate(operations);
    }

    // Clear connection pools in case there are active connections that are pooled
    private static void ClearAllPools()
        => NpgsqlConnection.ClearAllPools();

    // Clear connection pool for the database connection since after the 'create database' call, a previously
    // invalid connection may now be valid.
    private void ClearPool()
        => NpgsqlConnection.ClearPool((NpgsqlConnection)connection.DbConnection);
}
