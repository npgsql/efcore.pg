namespace Microsoft.EntityFrameworkCore.Update;

public class NonSharedModelUpdatesNpgsqlTest(NonSharedFixture fixture) : NonSharedModelUpdatesTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
