namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class TPCManyToManyNoTrackingQueryNpgsqlTest : TPCManyToManyNoTrackingQueryRelationalTestBase<TPCManyToManyQueryNpgsqlFixture>
{
    public TPCManyToManyNoTrackingQueryNpgsqlTest(TPCManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
