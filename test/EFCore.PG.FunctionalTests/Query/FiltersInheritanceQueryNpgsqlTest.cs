namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class FiltersInheritanceQueryNpgsqlTest : FiltersInheritanceQueryTestBase<FiltersInheritanceQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public FiltersInheritanceQueryNpgsqlTest(FiltersInheritanceQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
        => Fixture.TestSqlLoggerFactory.Clear();
}