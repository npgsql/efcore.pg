namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPTFiltersInheritanceQueryNpgsqlFixture : TPTInheritanceQueryNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
