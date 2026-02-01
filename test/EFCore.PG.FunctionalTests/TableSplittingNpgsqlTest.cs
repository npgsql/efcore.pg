using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

namespace Microsoft.EntityFrameworkCore;

[MinimumPostgresVersion(12, 0)] // Test suite uses computed columns
public class TableSplittingNpgsqlTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : TableSplittingTestBase(fixture, testOutputHelper)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public override async Task ExecuteUpdate_works_for_table_sharing(bool async)
    {
        await base.ExecuteUpdate_works_for_table_sharing(async);

        AssertSql(
            """
@p='1'

UPDATE "Vehicles" AS v
SET "SeatingCapacity" = @p
""",
            //
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Vehicles" AS v
    WHERE v."SeatingCapacity" <> 1)
""");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Engine>().ToTable("Vehicles")
            .Property(e => e.Computed).HasComputedColumnSql("1", stored: true);
    }
}
