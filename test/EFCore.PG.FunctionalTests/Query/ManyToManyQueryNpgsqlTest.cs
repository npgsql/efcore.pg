namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

internal class ManyToManyQueryNpgsqlTest : ManyToManyQueryRelationalTestBase<ManyToManyQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public ManyToManyQueryNpgsqlTest(ManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
