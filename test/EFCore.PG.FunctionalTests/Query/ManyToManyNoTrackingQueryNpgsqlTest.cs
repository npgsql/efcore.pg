namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

internal class ManyToManyNoTrackingQueryNpgsqlTest
    : ManyToManyNoTrackingQueryRelationalTestBase<ManyToManyQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public ManyToManyNoTrackingQueryNpgsqlTest(ManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // TODO: #1232
    // protected override bool CanExecuteQueryString => true;
}
