namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPCManyToManyQueryNpgsqlTest : TPCManyToManyQueryRelationalTestBase<TPCManyToManyQueryNpgsqlFixture>
{
    public TPCManyToManyQueryNpgsqlTest(TPCManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
