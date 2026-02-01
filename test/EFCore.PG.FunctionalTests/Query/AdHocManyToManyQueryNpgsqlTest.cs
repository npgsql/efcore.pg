namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocManyToManyQueryNpgsqlTest(NonSharedFixture fixture) : AdHocManyToManyQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
