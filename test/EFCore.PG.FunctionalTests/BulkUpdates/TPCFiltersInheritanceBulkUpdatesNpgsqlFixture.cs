namespace Npgsql.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class TPCFiltersInheritanceBulkUpdatesNpgsqlFixture : TPCInheritanceBulkUpdatesNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
