using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class F1NpgsqlFixture : F1RelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Chassis>().ForNpgsqlUseXminAsConcurrencyToken();
            modelBuilder.Entity<Driver>().ForNpgsqlUseXminAsConcurrencyToken();
            modelBuilder.Entity<Team>().ForNpgsqlUseXminAsConcurrencyToken();
        }
    }
}
