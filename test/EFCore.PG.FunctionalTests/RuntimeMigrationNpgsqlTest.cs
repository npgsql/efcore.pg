using System.Data.Common;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class RuntimeMigrationNpgsqlTest(RuntimeMigrationNpgsqlTest.RuntimeMigrationNpgsqlFixture fixture)
    : RuntimeMigrationTestBase<RuntimeMigrationNpgsqlTest.RuntimeMigrationNpgsqlFixture>(fixture)
{
    protected override Assembly ProviderAssembly
        => typeof(NpgsqlDesignTimeServices).Assembly;

    protected override List<string> GetTableNames(DbConnection connection)
    {
        var tables = new List<string>();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE' AND table_schema = 'public' AND table_name != '__EFMigrationsHistory'";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    public class RuntimeMigrationNpgsqlFixture : RuntimeMigrationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
