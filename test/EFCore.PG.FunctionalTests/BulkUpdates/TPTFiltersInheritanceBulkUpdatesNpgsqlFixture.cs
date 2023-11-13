namespace Npgsql.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class TPTFiltersInheritanceBulkUpdatesNpgsqlFixture : TPTInheritanceBulkUpdatesNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
