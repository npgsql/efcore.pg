namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class FiltersInheritanceQueryNpgsqlTest : FiltersInheritanceQueryTestBase<TPHFiltersInheritanceQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public FiltersInheritanceQueryNpgsqlTest(TPHFiltersInheritanceQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }
}
