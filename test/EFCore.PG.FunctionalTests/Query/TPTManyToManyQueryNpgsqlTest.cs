namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class TPTManyToManyQueryNpgsqlTest : TPTManyToManyQueryRelationalTestBase<TPTManyToManyQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public TPTManyToManyQueryNpgsqlTest(TPTManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // TODO: #1232
    // protected override bool CanExecuteQueryString => true;
}
