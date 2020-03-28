namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class FiltersInheritanceQueryNpgsqlFixture : InheritanceQueryNpgsqlFixture
    {
        protected override bool EnableFilters => true;
    }
}
