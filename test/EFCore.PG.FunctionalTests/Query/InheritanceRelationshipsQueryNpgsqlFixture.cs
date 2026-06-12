namespace Microsoft.EntityFrameworkCore.Query;

public class InheritanceRelationshipsQueryNpgsqlFixture : InheritanceRelationshipsQueryRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
