using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class NpgsqlTestStore : RelationalTestStore
{
    private readonly string? _scriptPath;
    private readonly string? _additionalSql;

    private const string Northwind = "Northwind";

    public const int CommandTimeout = 600;

    public static readonly string NorthwindConnectionString = CreateConnectionString(Northwind);

    public static async Task<NpgsqlTestStore> GetNorthwindStoreAsync()
        => (NpgsqlTestStore)await NpgsqlNorthwindTestStoreFactory.Instance
            .GetOrCreate(NpgsqlNorthwindTestStoreFactory.Name).InitializeAsync(null, (Func<DbContext>?)null);

    public static Task<NpgsqlTestStore> GetOrCreateInitializedAsync(string name)
        => new NpgsqlTestStore(name).InitializeNpgsqlAsync(null, (Func<DbContext>?)null, null);

    public static NpgsqlTestStore GetOrCreate(
        string name,
        string? scriptPath = null,
        string? additionalSql = null,
        string? connectionStringOptions = null)
        => new(name, scriptPath, additionalSql, connectionStringOptions);

    public static NpgsqlTestStore Create(string name, string? connectionStringOptions = null)
        => new(name, connectionStringOptions: connectionStringOptions, shared: false);

    public static Task<NpgsqlTestStore> CreateInitializedAsync(string name)
        => new NpgsqlTestStore(name, shared: false).InitializeNpgsqlAsync(null, (Func<DbContext>?)null, null);

    public NpgsqlTestStore(
        string name,
        string? scriptPath = null,
        string? additionalSql = null,
        string? connectionStringOptions = null,
        bool shared = true)
        : base(name, shared, CreateConnection(name, connectionStringOptions))
    {
        Name = name;

        if (scriptPath is not null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            _scriptPath = Path.Combine(Path.GetDirectoryName(typeof(NpgsqlTestStore).GetTypeInfo().Assembly.Location)!, scriptPath);
        }

        _additionalSql = additionalSql;
    }

    private static NpgsqlConnection CreateConnection(string name, string? connectionStringOptions)
        => new(CreateConnectionString(name, connectionStringOptions));

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task<NpgsqlTestStore> InitializeNpgsqlAsync(
        IServiceProvider? serviceProvider,
        Func<DbContext>? createContext,
        Func<DbContext, Task>? seed)
        => (NpgsqlTestStore)await InitializeAsync(serviceProvider, createContext, seed);

    // ReSharper disable once UnusedMember.Global
    public async Task<NpgsqlTestStore> InitializeNpgsqlAsync(
        IServiceProvider serviceProvider,
        Func<NpgsqlTestStore, DbContext> createContext,
        Func<DbContext, Task> seed)
        => await InitializeNpgsqlAsync(serviceProvider, () => createContext(this), seed);

    protected override async Task InitializeAsync(Func<DbContext> createContext, Func<DbContext, Task>? seed, Func<DbContext, Task>? clean)
    {
        if (await CreateDatabaseAsync(clean))
        {
            if (_scriptPath is not null)
            {
                ExecuteScript(_scriptPath);

                if (_additionalSql is not null)
                {
                    Execute(Connection, command => command.ExecuteNonQuery(), _additionalSql);
                }
            }
            else
            {
                await using var context = createContext();
                await context.Database.EnsureCreatedResilientlyAsync();

                if (_additionalSql is not null)
                {
                    Execute(Connection, command => command.ExecuteNonQuery(), _additionalSql);
                }

                if (seed is not null)
                {
                    await seed(context);
                }
            }
        }
    }

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
    {
        Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsBuilder = b => b.ApplyConfiguration()
            .CommandTimeout(CommandTimeout)
            // The tests are written with the assumption that NULLs are sorted first (SQL Server and .NET behavior), but PostgreSQL
            // sorts NULLs last by default. This configures the provider to emit NULLS FIRST.
            .ReverseNullOrdering();

        return UseConnectionString
            ? builder.UseNpgsql(ConnectionString, npgsqlOptionsBuilder)
            : builder.UseNpgsql(Connection, npgsqlOptionsBuilder);
    }

    private async Task<bool> CreateDatabaseAsync(Func<DbContext, Task>? clean)
    {
        await using var master = new NpgsqlConnection(CreateAdminConnectionString());

        if (await DatabaseExistsAsync(Name))
        {
            if (_scriptPath is not null)
            {
                return false;
            }

            await using var context = new DbContext(
                AddProviderOptions(new DbContextOptionsBuilder().EnableServiceProviderCaching(false)).Options);
            clean?.Invoke(context);
            await CleanAsync(context);
            return true;
        }

        await ExecuteNonQueryAsync(master, GetCreateDatabaseStatement(Name));
        await WaitForExistsAsync((NpgsqlConnection)Connection);

        return true;
    }

    private static async Task WaitForExistsAsync(NpgsqlConnection connection)
    {
        var retryCount = 0;
        while (true)
        {
            try
            {
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }

                NpgsqlConnection.ClearPool(connection);

                await connection.OpenAsync();
                await connection.CloseAsync();
                return;
            }
            catch (PostgresException e)
            {
                if (++retryCount >= 30
                    || e.SqlState != "08001" && e.SqlState != "08000" && e.SqlState != "08006")
                {
                    throw;
                }

                await Task.Delay(100);
            }
        }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public void ExecuteScript(string scriptPath)
    {
        var script = File.ReadAllText(scriptPath);
        Execute(
            Connection, command =>
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

    private static string GetCreateDatabaseStatement(string name)
        => $"""
            CREATE DATABASE "{name}"
            """;

    private static async Task<bool> DatabaseExistsAsync(string name)
    {
        await using var master = new NpgsqlConnection(CreateAdminConnectionString());

        return await ExecuteScalarAsync<long>(master, $@"SELECT COUNT(*) FROM pg_database WHERE datname = '{name}'") > 0;
    }

    public async Task DeleteDatabaseAsync()
    {
        if (!await DatabaseExistsAsync(Name))
        {
            return;
        }

        await using var master = new NpgsqlConnection(CreateAdminConnectionString());

        await ExecuteNonQueryAsync(master, GetDisconnectDatabaseSql(Name));
        await ExecuteNonQueryAsync(master, GetDropDatabaseSql(Name));

        NpgsqlConnection.ClearAllPools();
    }

    // Kill all connection to the database
    private static string GetDisconnectDatabaseSql(string name)
        => $"""
REVOKE CONNECT ON DATABASE "{name}" FROM PUBLIC;
SELECT pg_terminate_backend (pg_stat_activity.pid)
   FROM pg_stat_activity
   WHERE datname = '{name}'
""";

    private static string GetDropDatabaseSql(string name)
        => $"""
            DROP DATABASE "{name}"
            """;

    public override void OpenConnection()
        => Connection.Open();

    public override Task OpenConnectionAsync()
        => Connection.OpenAsync();

    // ReSharper disable once UnusedMember.Global
    public T ExecuteScalar<T>(string sql, params object[] parameters)
        => ExecuteScalar<T>(Connection, sql, parameters);

    private static T ExecuteScalar<T>(DbConnection connection, string sql, params object[] parameters)
        => Execute(connection, command => (T)command.ExecuteScalar()!, sql, false, parameters);

    // ReSharper disable once UnusedMember.Global
    public Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
        => ExecuteScalarAsync<T>(Connection, sql, parameters);

    private static Task<T> ExecuteScalarAsync<T>(DbConnection connection, string sql, object[]? parameters = null)
        => ExecuteAsync(connection, async command => (T)(await command.ExecuteScalarAsync())!, sql, false, parameters);

    // ReSharper disable once UnusedMethodReturnValue.Global
    public int ExecuteNonQuery(string sql, params object[] parameters)
        => ExecuteNonQuery(Connection, sql, parameters);

    private static int ExecuteNonQuery(DbConnection connection, string sql, object[]? parameters = null)
        => Execute(connection, command => command.ExecuteNonQuery(), sql, false, parameters);

    // ReSharper disable once UnusedMember.Global
    public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
        => ExecuteNonQueryAsync(Connection, sql, parameters);

    private static Task<int> ExecuteNonQueryAsync(DbConnection connection, string sql, object[]? parameters = null)
        => ExecuteAsync(connection, command => command.ExecuteNonQueryAsync(), sql, false, parameters);

    // ReSharper disable once UnusedMember.Global
    public IEnumerable<T> Query<T>(string sql, params object[] parameters)
        => Query<T>(Connection, sql, parameters);

    private static IEnumerable<T> Query<T>(DbConnection connection, string sql, object[]? parameters = null)
        => Execute(
            connection, command =>
            {
                using var dataReader = command.ExecuteReader();

                var results = Enumerable.Empty<T>();
                while (dataReader.Read())
                {
                    results = results.Concat([dataReader.GetFieldValue<T>(0)]);
                }

                return results;
            }, sql, false, parameters);

    // ReSharper disable once UnusedMember.Global
    public Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
        => QueryAsync<T>(Connection, sql, parameters);

    private static Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql, object[]? parameters = null)
        => ExecuteAsync(
            connection, async command =>
            {
                await using var dataReader = await command.ExecuteReaderAsync();

                var results = Enumerable.Empty<T>();
                while (await dataReader.ReadAsync())
                {
                    results = results.Concat([await dataReader.GetFieldValueAsync<T>(0)]);
                }

                return results;
            }, sql, false, parameters);

    private static T Execute<T>(
        DbConnection connection,
        Func<DbCommand, T> execute,
        string sql,
        bool useTransaction = false,
        object[]? parameters = null)
        => ExecuteCommand(connection, execute, sql, useTransaction, parameters);

    private static T ExecuteCommand<T>(
        DbConnection connection,
        Func<DbCommand, T> execute,
        string sql,
        bool useTransaction,
        object[]? parameters)
    {
        if (connection.State != ConnectionState.Closed)
        {
            connection.Close();
        }

        connection.Open();
        try
        {
            using var transaction = useTransaction ? connection.BeginTransaction() : null;

            T result;
            using (var command = CreateCommand(connection, sql, parameters))
            {
                command.Transaction = transaction;
                result = execute(command);
            }

            transaction?.Commit();

            return result;
        }
        finally
        {
            if (connection.State == ConnectionState.Closed
                && connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }
    }

    private static Task<T> ExecuteAsync<T>(
        DbConnection connection,
        Func<DbCommand, Task<T>> executeAsync,
        string sql,
        bool useTransaction = false,
        IReadOnlyList<object>? parameters = null)
        => ExecuteCommandAsync(connection, executeAsync, sql, useTransaction, parameters);

    private static async Task<T> ExecuteCommandAsync<T>(
        DbConnection connection,
        Func<DbCommand, Task<T>> executeAsync,
        string sql,
        bool useTransaction,
        IReadOnlyList<object>? parameters)
    {
        if (connection.State != ConnectionState.Closed)
        {
            await connection.CloseAsync();
        }

        await connection.OpenAsync();
        try
        {
            await using var transaction = useTransaction ? await connection.BeginTransactionAsync() : null;

            T result;
            await using (var command = CreateCommand(connection, sql, parameters))
            {
                result = await executeAsync(command);
            }

            if (transaction is not null)
            {
                await transaction.CommitAsync();
            }

            return result;
        }
        finally
        {
            if (connection.State == ConnectionState.Closed
                && connection.State != ConnectionState.Closed)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static DbCommand CreateCommand(
        DbConnection connection,
        string commandText,
        IReadOnlyList<object>? parameters = null)
    {
        var command = (NpgsqlCommand)connection.CreateCommand();

        command.CommandText = commandText;
        command.CommandTimeout = CommandTimeout;

        if (parameters is not null)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                command.Parameters.AddWithValue("p" + i, parameters[i]);
            }
        }

        return command;
    }

    public static string CreateConnectionString(string name, string? options = null)
    {
        var builder = new NpgsqlConnectionStringBuilder(TestEnvironment.DefaultConnection) { Database = name };

        if (options is not null)
        {
            builder.Options = options;
        }

        return builder.ConnectionString;
    }

    private static string CreateAdminConnectionString()
        => CreateConnectionString("postgres");

    public override Task CleanAsync(DbContext context)
    {
        context.Database.EnsureClean();
        return Task.CompletedTask;
    }
}
