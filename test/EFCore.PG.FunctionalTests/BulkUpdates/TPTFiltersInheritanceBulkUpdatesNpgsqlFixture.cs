namespace Npgsql.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class TPTFiltersInheritanceBulkUpdatesNpgsqlFixture : TPTInheritanceBulkUpdatesNpgsqlFixture
{
    protected override bool EnableFilters => true;
}
