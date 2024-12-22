namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class TPHFiltersInheritanceBulkUpdatesNpgsqlFixture : TPHInheritanceBulkUpdatesNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
