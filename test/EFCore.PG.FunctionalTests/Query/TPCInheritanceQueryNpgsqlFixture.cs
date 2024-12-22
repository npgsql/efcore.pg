namespace Microsoft.EntityFrameworkCore.Query;

public class TPCInheritanceQueryNpgsqlFixture : TPCInheritanceQueryFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
