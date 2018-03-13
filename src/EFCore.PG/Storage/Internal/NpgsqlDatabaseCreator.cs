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
using System.Collections.Generic;
using System.Data;
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
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
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

            Exists(retryOnNotExists: true);
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

            await ExistsAsync(retryOnNotExists: true, cancellationToken: cancellationToken);
        }

        private bool Exists(bool retryOnNotExists)
          => Dependencies.ExecutionStrategyFactory.Create().Execute(
              DateTime.UtcNow + RetryTimeout, giveUp =>
              {
                  while (true)
                  {
                      try
                      {
                          using (new TransactionScope(TransactionScopeOption.Suppress))
                          {
                              _connection.Open(errorsExpected: true);
                              _connection.Close();
                          }
                          return true;
                      }
                      catch (PostgresException e)
                      {
                          if (!retryOnNotExists
                                && IsDoesNotExist(e))
                          {
                              return false;
                          }

                          if (DateTime.UtcNow > giveUp
                                || !RetryOnExistsFailure(e))
                          {
                              throw;
                          }

                          Thread.Sleep(RetryDelay);
                      }
                  }
              });

        private Task<bool> ExistsAsync(bool retryOnNotExists, CancellationToken cancellationToken)
            => Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(
                DateTime.UtcNow + RetryTimeout, async (giveUp, ct) =>
                {
                    while (true)
                    {
                        try
                        {
                            using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                            {
                                await _connection.OpenAsync(ct, errorsExpected: true);

                                _connection.Close();
                            }
                            return true;
                        }
                        catch (PostgresException e)
                        {
                            if (!retryOnNotExists
                                && IsDoesNotExist(e))
                            {
                                return false;
                            }

                            if (DateTime.UtcNow > giveUp
                                || !RetryOnExistsFailure(e))
                            {
                                throw;
                            }

                            await Task.Delay(RetryDelay, ct);
                        }
                    }
                }, cancellationToken);

        protected override bool HasTables()
            => (bool)CreateHasTablesCommand().ExecuteScalar(_connection);

        protected override async Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
            => (bool)(await CreateHasTablesCommand().ExecuteScalarAsync(_connection, cancellationToken: cancellationToken));

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
                    Template = Dependencies.Model.Npgsql().DatabaseTemplate,
                    Tablespace = Dependencies.Model.Npgsql().Tablespace
                }
            });

        public override bool Exists()
        {
            try
            {
                // When checking whether a database exists, pooling must be off, otherwise we may
                // attempt to reuse a pooled connection, which may be broken (this happened in the tests).
                var unpooledCsb = new NpgsqlConnectionStringBuilder(_connection.ConnectionString) { Pooling = false };
                var unpooledConn = ((NpgsqlConnection)_connection.DbConnection).CloneWith(unpooledCsb.ToString());
                using (unpooledConn)
                {
                    unpooledConn.Open();
                    unpooledConn.Close();
                }

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
                var unpooledConn = ((NpgsqlConnection)_connection.DbConnection).CloneWith(unpooledCsb.ToString());
                using (unpooledConn)
                {
                    await unpooledConn.OpenAsync(cancellationToken);
                    unpooledConn.Close();
                }

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
        private static bool IsDoesNotExist(PostgresException exception) => exception.SqlState == "3D000";

        // See Issue #985(EFCore)
        private bool RetryOnExistsFailure(PostgresException exception)
        {
            if (exception.SqlState == "3D000")
            {
                ClearPool();
                return true;
            }
            return false;
        }

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
            var operations = Dependencies.ModelDiffer.GetDifferences(null, Dependencies.Model);
            var commands = Dependencies.MigrationsSqlGenerator.Generate(operations, Dependencies.Model);

            // Adding a PostgreSQL extension might define new types (e.g. hstore), which we
            // Npgsql to reload
            var reloadTypes = operations.Any(o => o is AlterDatabaseOperation && PostgresExtension.GetPostgresExtensions(o).Any());

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
            var operations = Dependencies.ModelDiffer.GetDifferences(null, Dependencies.Model);
            var commands = Dependencies.MigrationsSqlGenerator.Generate(operations, Dependencies.Model);

            // Adding a PostgreSQL extension might define new types (e.g. hstore), which we
            // Npgsql to reload
            var reloadTypes = operations.Any(o => o is AlterDatabaseOperation && PostgresExtension.GetPostgresExtensions(o).Any());

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

            // TODO: Not async
            if (reloadTypes)
            {
                var npgsqlConn = (NpgsqlConnection)_connection.DbConnection;
                if (npgsqlConn.FullState == ConnectionState.Open)
                    npgsqlConn.ReloadTypes();
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
        private static void ClearAllPools() => NpgsqlConnection.ClearAllPools();

        // Clear connection pool for the database connection since after the 'create database' call, a previously
        // invalid connection may now be valid.
        private void ClearPool() => NpgsqlConnection.ClearPool((NpgsqlConnection)_connection.DbConnection);
    }
}
