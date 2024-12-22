namespace Microsoft.EntityFrameworkCore;

public class EntitySplittingNpgsqlTest(ITestOutputHelper testOutputHelper) : EntitySplittingTestBase(testOutputHelper)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
