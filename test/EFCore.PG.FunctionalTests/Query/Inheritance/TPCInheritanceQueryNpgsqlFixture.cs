namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPCInheritanceQueryNpgsqlFixture : TPCInheritanceQueryFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
