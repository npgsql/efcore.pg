namespace Microsoft.EntityFrameworkCore.Query
{
    public class FiltersInheritanceNpgsqlFixture : InheritanceNpgsqlFixture
    {
        protected override bool EnableFilters => true;
        protected override string DatabaseName => "FiltersInheritanceSqlServerTest";
    }
}
