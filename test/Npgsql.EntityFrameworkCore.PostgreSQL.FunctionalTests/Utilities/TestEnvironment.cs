using System.IO;
using Microsoft.Extensions.Configuration;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities
{
    // TODO: Clean this up and adapt to our use case... Specifically how to manage build server,
    // environment vars...?
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

            Config = configBuilder.Build()
                .GetSection("Test:Npgsql");
        }

        private const string DefaultConnectionString = "Server=localhost;Username=npgsql_tests;Password=npgsql_tests;PersistSecurityInfo=true";

        public static string DefaultConnection => Config["DefaultConnection"] ?? DefaultConnectionString;

        public static bool? GetFlag(string key)
        {
            bool flag;
            return bool.TryParse(Config[key], out flag) ? flag : (bool?)null;
        }

        public static int? GetInt(string key)
        {
            int value;
            return int.TryParse(Config[key], out value) ? value : (int?)null;
        }
    }
}
