using Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;
namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class TPTFiltersInheritanceBulkUpdatesNpgsqlFixture : TPTInheritanceBulkUpdatesNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
