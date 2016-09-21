using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Npgsql;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities
{
    public class NpgsqlTestStore : RelationalTestStore
    {
        public const int CommandTimeout = 90;

#if NETCOREAPP1_0
        static string BaseDirectory => AppContext.BaseDirectory;
#else
        static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
#endif

        public static NpgsqlTestStore GetOrCreateShared(string name, bool useTransaction, Action initializeDatabase)
            => new NpgsqlTestStore(name).CreateShared(initializeDatabase, useTransaction);

        public static NpgsqlTestStore GetOrCreateShared(string name, Action initializeDatabase)
            => GetOrCreateShared(name, true, initializeDatabase);

        /// <summary>
        ///     A non-transactional, transient, isolated test database. Use this in the case
        ///     where transactions are not appropriate.
        /// </summary>
        public static Task<NpgsqlTestStore> CreateScratchAsync(bool createDatabase = true)
            => new NpgsqlTestStore(GetScratchDbName()).CreateTransientAsync(createDatabase);

        public static NpgsqlTestStore CreateScratch(bool createDatabase = true)
            => new NpgsqlTestStore(GetScratchDbName()).CreateTransient(createDatabase);

        string _connectionString;
        NpgsqlConnection _connection;
        NpgsqlTransaction _transaction;
        readonly string _name;
        bool _deleteDatabase;

        public override string ConnectionString => _connectionString;

        // Use async static factory method
        NpgsqlTestStore(string name)
        {
            _name = name;
        }

        static string GetScratchDbName()
        {
            string name;
            do
            {
                name = "Scratch_" + Guid.NewGuid();
            }
            while (DatabaseExists(name));

            return name;
        }

        NpgsqlTestStore CreateShared(Action initializeDatabase, bool useTransaction)
        {
            CreateShared(typeof(NpgsqlTestStore).Name + _name, initializeDatabase);

            _connectionString = CreateConnectionString(_name);
            _connection = new NpgsqlConnection(_connectionString);

            if (useTransaction)
            {
                _connection.Open();

                _transaction = _connection.BeginTransaction();
            }

            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// In PostgreSQL (unlike other DBs) a connection is always to a single database - you can't switch
        /// databases retaining the same connection. Therefore, a single SQL script can't drop and create the database
        /// like with SqlServer, for example.
        /// </remarks>
        public static void CreateDatabase(string name, string scriptPath = null, bool recreateIfAlreadyExists = false)
        {
            // If a script is specified we always drop and recreate an existing database
            if (scriptPath != null)
                recreateIfAlreadyExists = true;

            using (var master = new NpgsqlConnection(CreateAdminConnectionString()))
            {
                master.Open();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;

                    var exists = DatabaseExists(name);
                    if (exists && (recreateIfAlreadyExists || !TablesExist(name)))
                    {
                        command.CommandText = GetDisconnectDatabaseSql(name);
                        command.ExecuteNonQuery();
                        command.CommandText = GetDropDatabaseSql(name);
                        command.ExecuteNonQuery();
                        NpgsqlConnection.ClearAllPools();
                    }

                    if (!exists || recreateIfAlreadyExists)
                    {
                        command.CommandText = $@"CREATE DATABASE ""{name}""";
                        command.ExecuteNonQuery();
                    }
                }
            }

            if (scriptPath != null)
            {
                // HACK: Probe for script file as current dir
                // is different between k build and VS run.
                if (File.Exists(@"..\..\" + scriptPath))
                {
                    //executing in VS - so path is relative to bin\<config> dir
                    scriptPath = @"..\..\" + scriptPath;
                }
                else
                {
                    scriptPath = Path.Combine(BaseDirectory, scriptPath);
                }

                using (var conn = new NpgsqlConnection(CreateConnectionString(name)))
                {
                    conn.Open();
                    using (var command = new NpgsqlCommand("", conn))
                        ExecuteScript(scriptPath, command);
                }
            }
        }

        static void ExecuteScript(string scriptPath, NpgsqlCommand scriptCommand)
        {
            var script = File.ReadAllText(scriptPath);
            foreach (var batch in new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromMilliseconds(1000.0))
                .Split(script).Where(b => !string.IsNullOrEmpty(b)))
            {
                scriptCommand.CommandText = batch;

                scriptCommand.ExecuteNonQuery();
            }
        }

        async Task<NpgsqlTestStore> CreateTransientAsync(bool createDatabase)
        {
            _connectionString = CreateConnectionString(_name);
            _connection = new NpgsqlConnection(_connectionString);

            if (createDatabase)
            {
                using (var master = new NpgsqlConnection(CreateAdminConnectionString()))
                {
                    await master.OpenAsync();
                    using (var command = master.CreateCommand())
                    {
                        command.CommandTimeout = CommandTimeout;
                        command.CommandText = $@"{Environment.NewLine}CREATE DATABASE ""{_name}""";
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await _connection.OpenAsync();
            }

            _deleteDatabase = true;
            return this;
        }

        NpgsqlTestStore CreateTransient(bool createDatabase)
        {
            _connectionString = CreateConnectionString(_name);
            _connection = new NpgsqlConnection(_connectionString);

            if (createDatabase)
            {
                using (var master = new NpgsqlConnection(CreateAdminConnectionString()))
                {
                    master.Open();
                    using (var command = master.CreateCommand())
                    {
                        command.CommandTimeout = CommandTimeout;
                        command.CommandText = $@"{Environment.NewLine}CREATE DATABASE ""{_name}""";
                        command.ExecuteNonQuery();
                    }
                }
                _connection.Open();
            }

            _deleteDatabase = true;
            return this;
        }

        static bool DatabaseExists(string name)
        {
            using (var master = new NpgsqlConnection(CreateAdminConnectionString()))
            {
                master.Open();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandText = $@"SELECT COUNT(*) FROM pg_database WHERE datname = '{name}'";

                    return (long)command.ExecuteScalar() > 0;
                }
            }
        }

        static bool TablesExist(string name)
        {
            using (var connection = new NpgsqlConnection(CreateConnectionString(name)))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;

                    command.CommandText =
                        @"SELECT CASE WHEN COUNT(*) = 0 THEN FALSE ELSE TRUE END
                          FROM information_schema.tables
                          WHERE table_type = 'BASE TABLE' AND table_schema NOT IN('pg_catalog', 'information_schema')";

                    return (bool)command.ExecuteScalar();
                }
            }
        }

        void DeleteDatabase(string name)
        {
            using (var master = new NpgsqlConnection(CreateAdminConnectionString()))
            {
                master.Open();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout; // Query will take a few seconds if (and only if) there are active connections

                    command.CommandText = GetDisconnectDatabaseSql(name);
                    command.ExecuteNonQuery();
                    command.CommandText = GetDropDatabaseSql(name);
                    command.ExecuteNonQuery();

                    NpgsqlConnection.ClearAllPools();
                }
            }
        }

        // Kill all connection to the database
        // TODO: Pre-9.2 PG has column name procid instead of pid
        static string GetDisconnectDatabaseSql(string name) => $@"
REVOKE CONNECT ON DATABASE ""{name}"" FROM PUBLIC;
SELECT pg_terminate_backend (pg_stat_activity.pid)
   FROM pg_stat_activity
   WHERE datname = '{name}'";

        static string GetDropDatabaseSql(string name) => $@"DROP DATABASE ""{name}""";

        public override DbConnection Connection => _connection;

        public override DbTransaction Transaction => _transaction;

        public async Task<T> ExecuteScalarAsync<T>(string sql, CancellationToken cancellationToken, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return (T)await command.ExecuteScalarAsync(cancellationToken);
            }
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return command.ExecuteNonQuery();
            }
        }

        public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return command.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    var results = Enumerable.Empty<T>();

                    while (await dataReader.ReadAsync())
                    {
                        results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
                    }

                    return results;
                }
            }
        }

        private DbCommand CreateCommand(string commandText, object[] parameters)
        {
            var command = _connection.CreateCommand();

            if (_transaction != null)
            {
                command.Transaction = _transaction;
            }

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            for (var i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue("p" + i, parameters[i]);
            }

            return command;
        }

        public override void Dispose()
        {
            _transaction?.Dispose();

            _connection.Dispose();

            if (_deleteDatabase)
            {
                DeleteDatabase(_name);
            }
        }

        public static string CreateConnectionString(string name)
            => new NpgsqlConnectionStringBuilder(TestEnvironment.DefaultConnection) {
                Database = name
            }.ConnectionString;

        static string CreateAdminConnectionString() => CreateConnectionString("postgres");
    }
}
