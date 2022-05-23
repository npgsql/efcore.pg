namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class TPCFiltersInheritanceQueryNpgsqlFixture : TPCInheritanceQueryNpgsqlFixture
{
    protected override bool EnableFilters
        => true;
}
