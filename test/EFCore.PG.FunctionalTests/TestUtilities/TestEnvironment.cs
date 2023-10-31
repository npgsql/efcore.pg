using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

public static class TestEnvironment
{
    public static IConfiguration Config { get; }

    static TestEnvironment()
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: true)
            .AddJsonFile("config.test.json", optional: true)
            .AddEnvironmentVariables();

        var SectionName = "Test:Npgsql";
        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_COCKROACH_DB")))
            SectionName = "Test:CockroachDB";


        Config = configBuilder.Build()
            .GetSection(SectionName);

        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
    }

    private const string DefaultConnectionString = "Server=localhost;Username=npgsql_tests;Password=npgsql_tests;Port=5432";

    public static string DefaultConnection => Config["DefaultConnection"] ?? DefaultConnectionString;

    private static Version _postgresVersion;

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
            _isPostgisAvailable = (bool)cmd.ExecuteScalar();
            return _isPostgisAvailable.Value;
        }
    }
}
