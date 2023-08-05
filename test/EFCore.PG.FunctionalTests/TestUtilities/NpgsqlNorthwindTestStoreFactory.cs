namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

public class NpgsqlNorthwindTestStoreFactory : NpgsqlTestStoreFactory
{
    public const string Name = "Northwind";
    public static readonly string NorthwindConnectionString = NpgsqlTestStore.CreateConnectionString(Name);
    public new static NpgsqlNorthwindTestStoreFactory Instance { get; } = new();

    protected NpgsqlNorthwindTestStoreFactory()
    {
    }

    public override TestStore GetOrCreate(string storeName)
        => NpgsqlTestStore.GetOrCreate(Name, TestEnvironment.IsCockroachDB ? "NorthwindCRDB.sql" : "Northwind.sql",
            !TestEnvironment.IsCockroachDB && TestEnvironment.PostgresVersion >= new Version(12, 0)
                ? @"CREATE COLLATION IF NOT EXISTS ""some-case-insensitive-collation"" (LOCALE = 'en-u-ks-primary', PROVIDER = icu, DETERMINISTIC = False);"
                : null);
}
