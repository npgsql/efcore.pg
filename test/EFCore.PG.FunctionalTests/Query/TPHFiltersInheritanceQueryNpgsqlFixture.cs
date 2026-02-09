using Microsoft.EntityFrameworkCore.Query.Inheritance;

namespace Microsoft.EntityFrameworkCore.Query;

public class TPHFiltersInheritanceQueryNpgsqlFixture : TPHInheritanceQueryNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
