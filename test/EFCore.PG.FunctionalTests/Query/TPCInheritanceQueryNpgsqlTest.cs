namespace Microsoft.EntityFrameworkCore.Query;

public class TPCInheritanceQueryNpgsqlTest(TPCInheritanceQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : TPCInheritanceQueryTestBase<TPCInheritanceQueryNpgsqlFixture>(fixture, testOutputHelper)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
