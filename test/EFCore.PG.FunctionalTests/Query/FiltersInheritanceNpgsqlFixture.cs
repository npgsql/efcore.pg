namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class FiltersInheritanceNpgsqlFixture : InheritanceNpgsqlFixture
    {
        protected override bool EnableFilters => true;
        protected override string DatabaseName => "FiltersInheritanceSqlServerTest";
    }
}
