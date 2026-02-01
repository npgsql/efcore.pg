namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocQueryFiltersQueryNpgsqlTest(NonSharedFixture fixture)
    : AdHocQueryFiltersQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
