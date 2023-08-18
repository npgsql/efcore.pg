namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class TPTManyToManyNoTrackingQueryNpgsqlTest : TPTManyToManyNoTrackingQueryRelationalTestBase<TPTManyToManyQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public TPTManyToManyNoTrackingQueryNpgsqlTest(TPTManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // TODO: #1232
    // protected override bool CanExecuteQueryString => true;
}
