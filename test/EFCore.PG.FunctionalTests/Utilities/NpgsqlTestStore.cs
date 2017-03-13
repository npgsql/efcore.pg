using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities
{
    public class NpgsqlTestStore : RelationalTestStore
    {
        public const int CommandTimeout = 90;

#if NETCOREAPP1_1
        static string BaseDirectory => AppContext.BaseDirectory;
#else
        static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
#endif

        public static NpgsqlTestStore GetOrCreateShared(string name, Action initializeDatabase, bool cleanDatabase = true)
            => new NpgsqlTestStore(name, cleanDatabase).CreateShared(initializeDatabase);

        public static NpgsqlTestStore Create(string name)
            => new NpgsqlTestStore(name).CreateTransient(true, false);

        public static NpgsqlTestStore CreateScratch(bool createDatabase = true)
            => new NpgsqlTestStore(GetScratchDbName()).CreateTransient(createDatabase, true);

        string _connectionString;
        NpgsqlConnection _connection;
        readonly bool _cleanDatabase;
        bool _deleteDatabase;

        public string Name { get; }
        public override string ConnectionString => _connectionString;

        NpgsqlTestStore(string name, bool cleanDatabase = true)
        {
            Name = name;
            _cleanDatabase = cleanDatabase;
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

        NpgsqlTestStore CreateShared(Action initializeDatabase)
        {
            _connectionString = CreateConnectionString(Name);
            _connection = new NpgsqlConnection(_connectionString);

            CreateShared(typeof(NpgsqlTestStore).Name + Name,
                () =>
                {
                    if (CreateDatabase())
                    {
                        initializeDatabase?.Invoke();
                    }
                });

            return this;
        }

        bool CreateDatabase()
        {
            using (var master = new NpgsqlConnection(CreateAdminConnectionString()))
            {
                if (DatabaseExists(Name))
                {
                    if (!_cleanDatabase)
                    {
                        return false;
                    }

                    Clean(Name);
                }
                else
                {
                    ExecuteNonQuery(master, GetCreateDatabaseStatement(Name));
                }
            }

            return true;
        }

        public static void ExecuteScript(string databaseName, string scriptPath)
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

            var script = File.ReadAllText(scriptPath);
            using (var connection = new NpgsqlConnection(CreateConnectionString(databaseName)))
            {
                Execute(connection, command =>
                {
                    foreach (var batch in
                        new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromMilliseconds(1000.0))
                            .Split(script).Where(b => !string.IsNullOrEmpty(b)))
                    {
                        command.CommandText = batch;
                        command.ExecuteNonQuery();
                    }
                    return 0;
                }, "");
            }
        }

        NpgsqlTestStore CreateTransient(bool createDatabase, bool deleteDatabase)
        {
            _connectionString = CreateConnectionString(Name);
            _connection = new NpgsqlConnection(_connectionString);

            if (createDatabase)
            {
                CreateDatabase();

                OpenConnection();
            }
            else if (DatabaseExists(Name))
            {
                DeleteDatabase(Name);
            }

            _deleteDatabase = deleteDatabase;
            return this;
        }

        static void Clean(string name)
        {
            var options = new DbContextOptionsBuilder()
                .UseNpgsql(CreateConnectionString(name), b => b.ApplyConfiguration())
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkNpgsql()
                        .BuildServiceProvider())
                .Options;

            using (var context = new DbContext(options))
            {
                context.Database.EnsureClean();
            }
        }

        static string GetCreateDatabaseStatement(string name)
            => $@"CREATE DATABASE ""{name}""";

        static bool DatabaseExists(string name)
        {
            using (var master = new NpgsqlConnection(CreateAdminConnectionString()))
                return ExecuteScalar<long>(master, $@"SELECT COUNT(*) FROM pg_database WHERE datname = '{name}'") > 0;
        }

        void DeleteDatabase(string name)
        {
            if (!DatabaseExists(name))
                return;
            using (var master = new NpgsqlConnection(CreateAdminConnectionString()))
            {
                ExecuteNonQuery(master, GetDisconnectDatabaseSql(name));
                ExecuteNonQuery(master, GetDropDatabaseSql(name));

                NpgsqlConnection.ClearAllPools();
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

        public static IExecutionStrategy GetExecutionStrategy()
            => (IExecutionStrategy)NoopExecutionStrategy.Instance;

        public override DbConnection Connection => _connection;

        public override DbTransaction Transaction => null;

        public override void OpenConnection()
        {
            GetExecutionStrategy().Execute(connection => connection.Open(), _connection);
        }

        public Task OpenConnectionAsync()
        {
            return GetExecutionStrategy().ExecuteAsync(connection => connection.OpenAsync(), _connection);
        }

        public T ExecuteScalar<T>(string sql, params object[] parameters)
            => ExecuteScalar<T>(_connection, sql, parameters);

        private static T ExecuteScalar<T>(NpgsqlConnection connection, string sql, params object[] parameters)
            => Execute(connection, command => (T)command.ExecuteScalar(), sql, false, parameters);

        public Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
            => ExecuteScalarAsync<T>(_connection, sql, parameters);

        private static Task<T> ExecuteScalarAsync<T>(NpgsqlConnection connection, string sql, object[] parameters = null)
            => ExecuteAsync(connection, async command => (T)await command.ExecuteScalarAsync(), sql, false, parameters);

        public int ExecuteNonQuery(string sql, params object[] parameters)
            => ExecuteNonQuery(_connection, sql, parameters);

        private static int ExecuteNonQuery(NpgsqlConnection connection, string sql, object[] parameters = null)
            => Execute(connection, command => command.ExecuteNonQuery(), sql, false, parameters);

        public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
            => ExecuteNonQueryAsync(_connection, sql, parameters);

        private static Task<int> ExecuteNonQueryAsync(NpgsqlConnection connection, string sql, object[] parameters = null)
            => ExecuteAsync(connection, command => command.ExecuteNonQueryAsync(), sql, false, parameters);

        public IEnumerable<T> Query<T>(string sql, params object[] parameters)
            => Query<T>(_connection, sql, parameters);

        private static IEnumerable<T> Query<T>(NpgsqlConnection connection, string sql, object[] parameters = null)
            => Execute(connection, command =>
            {
                using (var dataReader = command.ExecuteReader())
                {
                    var results = Enumerable.Empty<T>();
                    while (dataReader.Read())
                    {
                        results = results.Concat(new[] { dataReader.GetFieldValue<T>(0) });
                    }
                    return results;
                }
            }, sql, false, parameters);

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
            => QueryAsync<T>(_connection, sql, parameters);

        private static Task<IEnumerable<T>> QueryAsync<T>(NpgsqlConnection connection, string sql, object[] parameters = null)
            => ExecuteAsync(connection, async command =>
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
            }, sql, false, parameters);

        private static T Execute<T>(
                NpgsqlConnection connection, Func<DbCommand, T> execute, string sql, bool useTransaction = false, object[] parameters = null)
            => GetExecutionStrategy().Execute(state =>
            {
                if (state.connection.State != ConnectionState.Closed)
                {
                    state.connection.Close();
                }
                state.connection.Open();
                try
                {
                    using (var transaction = useTransaction ? state.connection.BeginTransaction() : null)
                    {
                        T result;
                        using (var command = CreateCommand(state.connection, sql, parameters))
                        {
                            command.Transaction = transaction;
                            result = execute(command);
                        }
                        transaction?.Commit();

                        return result;
                    }
                }
                finally
                {
                    if (state.State == ConnectionState.Closed
                        && state.connection.State != ConnectionState.Closed)
                    {
                        state.connection.Close();
                    }
                }
            }, new { connection, connection.State });

        private static Task<T> ExecuteAsync<T>(
                NpgsqlConnection connection, Func<DbCommand, Task<T>> executeAsync, string sql, bool useTransaction, object[] parameters = null)
            => GetExecutionStrategy().ExecuteAsync(async state =>
            {
                if (state.connection.State != ConnectionState.Closed)
                {
                    state.connection.Close();
                }
                await state.connection.OpenAsync();
                try
                {
                    using (var transaction = useTransaction ? state.connection.BeginTransaction() : null)
                    {
                        T result;
                        using (var command = CreateCommand(state.connection, sql, parameters))
                        {
                            result = await executeAsync(command);
                        }
                        transaction?.Commit();

                        return result;
                    }
                }
                finally
                {
                    if (state.State == ConnectionState.Closed
                        && state.connection.State != ConnectionState.Closed)
                    {
                        state.connection.Close();
                    }
                }
            }, new { connection, connection.State });


        static DbCommand CreateCommand(NpgsqlConnection connection, string commandText, object[] parameters = null)
        {
            var command = connection.CreateCommand();

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    command.Parameters.AddWithValue("p" + i, parameters[i]);
                }
            }

            return command;
        }

        public override void Dispose()
        {
            _connection.Dispose();

            if (_deleteDatabase)
            {
                DeleteDatabase(Name);
            }
        }

        public static string CreateConnectionString(string name)
            => new NpgsqlConnectionStringBuilder(TestEnvironment.DefaultConnection) {
                Database = name
            }.ConnectionString;

        static string CreateAdminConnectionString() => CreateConnectionString("postgres");
    }
}
