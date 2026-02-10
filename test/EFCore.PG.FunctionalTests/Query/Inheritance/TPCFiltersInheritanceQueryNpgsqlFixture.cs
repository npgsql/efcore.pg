namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPCFiltersInheritanceQueryNpgsqlFixture : TPCInheritanceQueryNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
