namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class ComplexNavigationsCollectionsSplitQueryNpgsqlTest : ComplexNavigationsCollectionsSplitQueryRelationalTestBase<
    ComplexNavigationsQueryNpgsqlFixture>
{
    public ComplexNavigationsCollectionsSplitQueryNpgsqlTest(
        ComplexNavigationsQueryNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
