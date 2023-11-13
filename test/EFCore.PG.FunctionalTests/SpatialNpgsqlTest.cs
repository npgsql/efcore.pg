using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

[RequiresPostgis]
public class SpatialNpgsqlTest : SpatialTestBase<SpatialNpgsqlFixture>
{
    public SpatialNpgsqlTest(SpatialNpgsqlFixture fixture)
        : base(fixture)
    {
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
