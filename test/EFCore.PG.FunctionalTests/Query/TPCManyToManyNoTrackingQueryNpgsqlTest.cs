namespace Microsoft.EntityFrameworkCore.Query;

public class TPCManyToManyNoTrackingQueryNpgsqlTest : TPCManyToManyNoTrackingQueryRelationalTestBase<TPCManyToManyQueryNpgsqlFixture>
{
    public TPCManyToManyNoTrackingQueryNpgsqlTest(TPCManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
