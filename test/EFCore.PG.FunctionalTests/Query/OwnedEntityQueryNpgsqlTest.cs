namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedEntityQueryNpgsqlTest(NonSharedFixture fixture) : OwnedEntityQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
