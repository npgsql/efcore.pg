namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class TPTInheritanceBulkUpdatesNpgsqlFixture : TPTInheritanceBulkUpdatesFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
