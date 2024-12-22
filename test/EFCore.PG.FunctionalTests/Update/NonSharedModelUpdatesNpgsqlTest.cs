namespace Microsoft.EntityFrameworkCore.Update;

public class NonSharedModelUpdatesNpgsqlTest : NonSharedModelUpdatesTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
