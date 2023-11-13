namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class TPHFiltersInheritanceQueryNpgsqlFixture : TPHInheritanceQueryNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
