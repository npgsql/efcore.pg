using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

// ReSharper disable once UnusedMember.Global
public class MappingQueryNpgsqlTest : MappingQueryTestBase<MappingQueryNpgsqlTest.MappingQueryNpgsqlFixture>
{
    public MappingQueryNpgsqlTest(MappingQueryNpgsqlFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    public class MappingQueryNpgsqlFixture : MappingQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlNorthwindTestStoreFactory.Instance;

        protected override string DatabaseSchema { get; } = "public";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<MappedCustomer>(
                e =>
                {
                    e.Property(c => c.CompanyName2).Metadata.SetColumnName("CompanyName");
                    e.Metadata.SetTableName("Customers");
                    e.Metadata.SetSchema("public");
                });

            modelBuilder.Entity<MappedEmployee>()
                .Property(c => c.EmployeeID)
                .HasColumnType("int");
        }
    }
}
