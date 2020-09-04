using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlDatabaseCreator : RelationalDatabaseCreator
    {
        readonly INpgsqlRelationalConnection _connection;
        readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

        public NpgsqlDatabaseCreator(
            [NotNull] RelationalDatabaseCreatorDependencies dependencies,
            [NotNull] INpgsqlRelationalConnection connection,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(dependencies)
        {
            _connection = connection;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        public virtual TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        public virtual TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

        public override void Create()
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                try
                {
                    Dependencies.MigrationCommandExecutor
                        .ExecuteNonQuery(CreateCreateOperations(), masterConnection);
                }
                catch (PostgresException e) when (
                    e.SqlState == "23505" && e.ConstraintName == "pg_database_datname_index"
                )
                {
                    // This occurs when two connections are trying to create the same database concurrently
                    // (happens in the tests). Simply ignore the error.
                }

                ClearPool();
            }

            Exists();
        }

        public override async Task CreateAsync(CancellationToken cancellationToken = default)
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                try
                {
                    await Dependencies.MigrationCommandExecutor
                        .ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken);
                }
                catch (PostgresException e) when (
                    e.SqlState == "23505" && e.ConstraintName == "pg_database_datname_index"
                )
                {
                    // This occurs when two connections are trying to create the same database concurrently
                    // (happens in the tests). Simply ignore the error.
                }

                ClearPool();
            }

            await ExistsAsync(cancellationToken);
        }

        public override bool HasTables()
            => Dependencies.ExecutionStrategyFactory
                .Create()
                .Execute(
                    _connection,
                    connection => (bool)CreateHasTablesCommand()
                                      .ExecuteScalar(
                                          new RelationalCommandParameterObject(
                                              connection,
                                              null,
                                              null,
                                              Dependencies.CurrentContext.Context,
                                              Dependencies.CommandLogger)));

        public override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
            => Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(
                _connection,
                async (connection, ct) => (bool)await CreateHasTablesCommand()
                                              .ExecuteScalarAsync(
                                                  new RelationalCommandParameterObject(
                                                      connection,
                                                      null,
                                                      null,
                                                      Dependencies.CurrentContext.Context,
                                                      Dependencies.CommandLogger),
                                                  cancellationToken: ct), cancellationToken);

        IRelationalCommand CreateHasTablesCommand()
            => _rawSqlCommandBuilder
                .Build(@"
                    SELECT CASE WHEN COUNT(*) = 0 THEN FALSE ELSE TRUE END
                    FROM information_schema.tables
                    WHERE table_type = 'BASE TABLE' AND table_schema NOT IN ('pg_catalog', 'information_schema')
                ");

        IReadOnlyList<MigrationCommand> CreateCreateOperations()
            => Dependencies.MigrationsSqlGenerator.Generate(new[]
            {
                new NpgsqlCreateDatabaseOperation
                {
                    Name = _connection.DbConnection.Database,
                    Template = Dependencies.Model.GetDatabaseTemplate(),
                    Collation = Dependencies.Model.GetCollation(),
                    Tablespace = Dependencies.Model.GetTablespace()
                }
            });

        public override bool Exists()
        {
            try
            {
                // When checking whether a database exists, pooling must be off, otherwise we may
                // attempt to reuse a pooled connection, which may be broken (this happened in the tests).
                var unpooledCsb = new NpgsqlConnectionStringBuilder(_connection.ConnectionString) { Pooling = false };
                using var unpooledConn = ((NpgsqlConnection)_connection.DbConnection).CloneWith(unpooledCsb.ToString());
                using var _ = new TransactionScope(TransactionScopeOption.Suppress);
                unpooledConn.Open();
                return true;
            }
            catch (PostgresException e)
            {
                if (IsDoesNotExist(e))
                {
                    return false;
                }

                throw;
            }
            catch (NpgsqlException e) when (
                e.InnerException is IOException &&
                e.InnerException.InnerException is SocketException &&
                ((SocketException)e.InnerException.InnerException).SocketErrorCode == SocketError.ConnectionReset
            )
            {
                // Pretty awful hack around #104
                return false;
            }
        }

        public override async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // When checking whether a database exists, pooling must be off, otherwise we may
                // attempt to reuse a pooled connection, which may be broken (this happened in the tests).
                var unpooledCsb = new NpgsqlConnectionStringBuilder(_connection.ConnectionString) { Pooling = false };
                using var unpooledConn = ((NpgsqlConnection)_connection.DbConnection).CloneWith(unpooledCsb.ToString());
                using var _ = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
                await unpooledConn.OpenAsync(cancellationToken);
                return true;
            }
            catch (PostgresException e)
            {
                if (IsDoesNotExist(e))
                {
                    return false;
                }

                throw;
            }
            catch (NpgsqlException e) when (
                e.InnerException is IOException &&
                e.InnerException.InnerException is SocketException &&
                ((SocketException)e.InnerException.InnerException).SocketErrorCode == SocketError.ConnectionReset
            )
            {
                // Pretty awful hack around #104
                return false;
            }
        }

        // Login failed is thrown when database does not exist (See Issue #776)
        static bool IsDoesNotExist(PostgresException exception) => exception.SqlState == "3D000";

        public override void Delete()
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateDropCommands(), masterConnection);
            }
        }

        public override async Task DeleteAsync(CancellationToken cancellationToken = default)
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken);
            }
        }

        public override void CreateTables()
        {
            var operations = Dependencies.ModelDiffer.GetDifferences(null, Dependencies.Model.GetRelationalModel());
            var commands = Dependencies.MigrationsSqlGenerator.Generate(operations, Dependencies.Model);

            // If a PostgreSQL extension, enum or range was added, we want Npgsql to reload all types at the ADO.NET level.
            var reloadTypes =
                operations.OfType<AlterDatabaseOperation>()
                          .Any(o =>
                              o.GetPostgresExtensions().Any() ||
                              o.GetPostgresEnums().Any() ||
                              o.GetPostgresRanges().Any());

            try
            {
                Dependencies.MigrationCommandExecutor.ExecuteNonQuery(commands, _connection);
            }
            catch (PostgresException e) when (
                e.SqlState == "23505" && e.ConstraintName == "pg_type_typname_nsp_index"
            )
            {
                // This occurs when two connections are trying to create the same database concurrently
                // (happens in the tests). Simply ignore the error.
            }

            if (reloadTypes)
            {
                _connection.Open();
                try
                {
                    ((NpgsqlConnection)_connection.DbConnection).ReloadTypes();
                }
                catch
                {
                    _connection.Close();
                }
            }
        }

        public override async Task CreateTablesAsync(CancellationToken cancellationToken = default)
        {
            var operations = Dependencies.ModelDiffer.GetDifferences(null, Dependencies.Model.GetRelationalModel());
            var commands = Dependencies.MigrationsSqlGenerator.Generate(operations, Dependencies.Model);

            // If a PostgreSQL extension, enum or range was added, we want Npgsql to reload all types at the ADO.NET level.
            var reloadTypes =
                operations.OfType<AlterDatabaseOperation>()
                          .Any(o =>
                              o.GetPostgresExtensions().Any() ||
                              o.GetPostgresEnums().Any() ||
                              o.GetPostgresRanges().Any());

            try
            {
                await Dependencies.MigrationCommandExecutor.ExecuteNonQueryAsync(commands, _connection,
                    cancellationToken);
            }
            catch (PostgresException e) when (
                e.SqlState == "23505" && e.ConstraintName == "pg_type_typname_nsp_index"
            )
            {
                // This occurs when two connections are trying to create the same database concurrently
                // (happens in the tests). Simply ignore the error.
            }

            if (reloadTypes)
            {
                await _connection.OpenAsync(cancellationToken);
                try
                {
                    // TODO: Not async
                    ((NpgsqlConnection)_connection.DbConnection).ReloadTypes();
                }
                catch
                {
                    _connection.Close();
                }
            }
        }

        IReadOnlyList<MigrationCommand> CreateDropCommands()
        {
            var operations = new MigrationOperation[]
            {
                // TODO Check DbConnection.Database always gives us what we want
                // Issue #775
                new NpgsqlDropDatabaseOperation { Name = _connection.DbConnection.Database }
            };

            return Dependencies.MigrationsSqlGenerator.Generate(operations);
        }

        // Clear connection pools in case there are active connections that are pooled
        static void ClearAllPools() => NpgsqlConnection.ClearAllPools();

        // Clear connection pool for the database connection since after the 'create database' call, a previously
        // invalid connection may now be valid.
        void ClearPool() => NpgsqlConnection.ClearPool((NpgsqlConnection)_connection.DbConnection);
    }
}
