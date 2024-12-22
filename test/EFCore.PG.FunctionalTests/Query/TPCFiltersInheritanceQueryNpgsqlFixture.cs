namespace Microsoft.EntityFrameworkCore.Query;

public class TPCFiltersInheritanceQueryNpgsqlFixture : TPCInheritanceQueryNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
