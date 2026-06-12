namespace Microsoft.EntityFrameworkCore.Update;

public class NonSharedModelUpdatesNpgsqlTest(NonSharedFixture fixture) : NonSharedModelUpdatesTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
