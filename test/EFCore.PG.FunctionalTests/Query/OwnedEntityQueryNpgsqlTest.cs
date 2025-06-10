namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedEntityQueryNpgsqlTest(NonSharedFixture fixture) : OwnedEntityQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
