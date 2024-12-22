namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocManyToManyQueryNpgsqlTest : AdHocManyToManyQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
