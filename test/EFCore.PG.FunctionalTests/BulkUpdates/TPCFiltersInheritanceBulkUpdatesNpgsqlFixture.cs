namespace Npgsql.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class TPCFiltersInheritanceBulkUpdatesNpgsqlFixture : TPCInheritanceBulkUpdatesNpgsqlFixture
{
    protected override bool EnableFilters => true;
}
