namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class FiltersInheritanceNpgsqlFixture : InheritanceNpgsqlFixture
    {
        protected override bool EnableFilters => true;
    }
}
