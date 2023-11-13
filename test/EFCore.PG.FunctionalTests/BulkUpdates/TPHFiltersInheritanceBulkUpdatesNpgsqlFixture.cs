namespace Npgsql.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class TPHFiltersInheritanceBulkUpdatesNpgsqlFixture : TPHInheritanceBulkUpdatesNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
