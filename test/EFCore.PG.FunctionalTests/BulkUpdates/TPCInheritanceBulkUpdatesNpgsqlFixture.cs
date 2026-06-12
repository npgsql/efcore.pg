using Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;
namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class TPCInheritanceBulkUpdatesNpgsqlFixture : TPCInheritanceBulkUpdatesFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public override bool UseGeneratedKeys
        => false;
}
