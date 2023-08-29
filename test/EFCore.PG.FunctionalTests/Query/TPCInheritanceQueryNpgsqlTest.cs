namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class TPCInheritanceQueryNpgsqlTest : TPCInheritanceQueryTestBase<TPCInheritanceQueryNpgsqlFixture>
{
    public TPCInheritanceQueryNpgsqlTest(TPCInheritanceQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
