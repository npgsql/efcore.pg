namespace Microsoft.EntityFrameworkCore.Query;

public class TPTInheritanceQueryNpgsqlFixture : TPTInheritanceQueryFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
