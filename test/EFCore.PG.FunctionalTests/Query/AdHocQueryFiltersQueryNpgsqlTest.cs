namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocQueryFiltersQueryNpgsqlTest : AdHocQueryFiltersQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
