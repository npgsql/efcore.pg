namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocManyToManyQueryNpgsqlTest(NonSharedFixture fixture) : AdHocManyToManyQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
