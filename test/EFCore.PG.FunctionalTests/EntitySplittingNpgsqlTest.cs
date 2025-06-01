namespace Microsoft.EntityFrameworkCore;

public class EntitySplittingNpgsqlTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : EntitySplittingTestBase(fixture, testOutputHelper)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
