namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class ComplexNavigationsCollectionsQueryNpgsqlTest : ComplexNavigationsCollectionsQueryRelationalTestBase<
    ComplexNavigationsQueryNpgsqlFixture>
{
    public ComplexNavigationsCollectionsQueryNpgsqlTest(
        ComplexNavigationsQueryNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
