namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPCManyToManyNoTrackingQueryNpgsqlTest : TPCManyToManyNoTrackingQueryRelationalTestBase<TPCManyToManyQueryNpgsqlFixture>
{
    public TPCManyToManyNoTrackingQueryNpgsqlTest(TPCManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
