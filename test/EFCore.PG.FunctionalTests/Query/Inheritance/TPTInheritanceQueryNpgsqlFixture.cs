namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPTInheritanceQueryNpgsqlFixture : TPTInheritanceQueryFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
