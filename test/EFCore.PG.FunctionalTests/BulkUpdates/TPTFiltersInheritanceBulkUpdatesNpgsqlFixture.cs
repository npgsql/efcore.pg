namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class TPTFiltersInheritanceBulkUpdatesNpgsqlFixture : TPTInheritanceBulkUpdatesNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
