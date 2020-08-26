using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    [MinimumPostgresVersion(12, 0)] // Test suite uses computed columns
    public class TableSplittingNpgsqlTest : TableSplittingTestBase
    {
        public TableSplittingNpgsqlTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Engine>().ToTable("Vehicles")
                .Property(e => e.Computed).HasComputedColumnSql("1", stored: true);
        }
    }
}
