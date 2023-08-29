namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class TPCFiltersInheritanceQueryNpgsqlFixture : TPCInheritanceQueryNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
