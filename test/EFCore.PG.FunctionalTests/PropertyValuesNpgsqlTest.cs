using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class PropertyValuesNpgsqlTest : PropertyValuesTestBase<PropertyValuesNpgsqlTest.PropertyValuesNpgsqlFixture>
    {
        public PropertyValuesNpgsqlTest(PropertyValuesNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class PropertyValuesNpgsqlFixture : PropertyValuesFixtureBase
        {
            protected override string StoreName { get; } = "PropertyValues";

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<Building>()
                    .Property(b => b.Value).HasColumnType("decimal(18,2)");

                modelBuilder.Entity<CurrentEmployee>()
                    .Property(ce => ce.LeaveBalance).HasColumnType("decimal(18,2)");
            }
        }
    }
}
