using System.Globalization;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class TestEnvironment
{
    public static IConfiguration Config { get; }

    public static string ConnectionString { get; }

    // Keep a reference to prevent GC from collecting (and finalizing/stopping) the container while tests are running.
    private static readonly PostgreSqlContainer? _postgreSqlContainer;

    static TestEnvironment()
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: true)
            .AddJsonFile("config.test.json", optional: true)
            .AddEnvironmentVariables();

        Config = configBuilder.Build()
            .GetSection("Test:Npgsql");

        if (Config["DefaultConnection"] is { } connectionString)
        {
            Console.WriteLine("Using connection string configured via Test:Npgsql:DefaultConnection: " + connectionString);

            ConnectionString = connectionString;
        }
        else
        {
            Console.WriteLine("No connection string configured via Test:Npgsql:DefaultConnection, starting up testcontainer...");

            _postgreSqlContainer = new PostgreSqlBuilder("postgres:latest")
                .WithCommand("-c", "max_connections=200")
                .Build();
            _postgreSqlContainer.StartAsync().GetAwaiter().GetResult();
            ConnectionString = _postgreSqlContainer.GetConnectionString();

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                _postgreSqlContainer.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
    }

    private static Version? _postgresVersion;

    public static Version PostgresVersion
    {
        get
        {
            if (_postgresVersion is not null)
            {
                return _postgresVersion;
            }

            using var conn = new NpgsqlConnection(NpgsqlTestStore.CreateConnectionString("postgres"));
            conn.Open();
            return _postgresVersion = conn.PostgreSqlVersion;
        }
    }

    private static bool? _isPostgisAvailable;

    public static bool IsPostgisAvailable
    {
        get
        {
            if (_isPostgisAvailable.HasValue)
            {
                return _isPostgisAvailable.Value;
            }

            using var conn = new NpgsqlConnection(NpgsqlTestStore.CreateConnectionString("postgres"));
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM pg_available_extensions WHERE \"name\" = 'postgis' LIMIT 1)";
            _isPostgisAvailable = (bool)cmd.ExecuteScalar()!;
            return _isPostgisAvailable.Value;
        }
    }
}
