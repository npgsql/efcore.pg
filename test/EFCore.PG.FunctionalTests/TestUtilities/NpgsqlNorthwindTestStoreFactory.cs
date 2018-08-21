using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class NpgsqlNorthwindTestStoreFactory : NpgsqlTestStoreFactory
    {
        public const string Name = "Northwind";
        public static readonly string NorthwindConnectionString = NpgsqlTestStore.CreateConnectionString(Name);
        public new static NpgsqlNorthwindTestStoreFactory Instance { get; } = new NpgsqlNorthwindTestStoreFactory();

        protected NpgsqlNorthwindTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => NpgsqlTestStore.GetOrCreate(Name, "Northwind.sql");
    }
}
