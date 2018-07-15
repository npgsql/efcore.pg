using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class MappingQueryNpgsqlTest : MappingQueryTestBase<MappingQueryNpgsqlTest.MappingQueryNpgsqlFixture>
    {
        public MappingQueryNpgsqlTest(MappingQueryNpgsqlFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;

        public class MappingQueryNpgsqlFixture : MappingQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlNorthwindTestStoreFactory.Instance;

            protected override string DatabaseSchema { get; } = "public";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<MappedCustomer>(
                    e =>
                    {
                        e.Property(c => c.CompanyName2).Metadata.Npgsql().ColumnName = "CompanyName";
                        e.Metadata.Npgsql().TableName = "Customers";
                        e.Metadata.Npgsql().Schema = "public";
                    });

                modelBuilder.Entity<MappedEmployee>()
                    .Property(c => c.EmployeeID)
                    .HasColumnType("int");
            }
        }
    }
}
