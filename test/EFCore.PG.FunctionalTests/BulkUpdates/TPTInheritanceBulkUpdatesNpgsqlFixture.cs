using Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;
namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class TPTInheritanceBulkUpdatesNpgsqlFixture : TPTInheritanceBulkUpdatesFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
