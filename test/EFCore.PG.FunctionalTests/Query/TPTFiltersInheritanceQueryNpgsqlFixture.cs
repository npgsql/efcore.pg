using Microsoft.EntityFrameworkCore.Query.Inheritance;
namespace Microsoft.EntityFrameworkCore.Query;

public class TPTFiltersInheritanceQuerySqlServerFixture : TPTInheritanceQueryNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
