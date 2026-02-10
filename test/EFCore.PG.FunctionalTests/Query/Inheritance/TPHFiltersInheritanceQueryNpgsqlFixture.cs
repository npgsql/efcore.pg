namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPHFiltersInheritanceQueryNpgsqlFixture : TPHInheritanceQueryNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
