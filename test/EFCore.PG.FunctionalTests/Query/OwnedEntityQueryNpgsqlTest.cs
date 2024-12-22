namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedEntityQueryNpgsqlTest : OwnedEntityQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
