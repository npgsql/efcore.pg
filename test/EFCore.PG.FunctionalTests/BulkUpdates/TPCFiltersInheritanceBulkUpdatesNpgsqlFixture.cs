namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class TPCFiltersInheritanceBulkUpdatesNpgsqlFixture : TPCInheritanceBulkUpdatesNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
