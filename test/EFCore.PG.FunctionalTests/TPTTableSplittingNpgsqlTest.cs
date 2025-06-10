namespace Microsoft.EntityFrameworkCore;

public class TPTTableSplittingNpgsqlTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : TPTTableSplittingTestBase(fixture, testOutputHelper)
{
    public override Task Can_insert_dependent_with_just_one_parent()
        // This scenario is not valid for TPT
        => Task.CompletedTask;

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
