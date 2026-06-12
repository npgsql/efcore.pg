namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPTFiltersInheritanceQueryNpgsqlTest : TPTFiltersInheritanceQueryTestBase<TPTFiltersInheritanceQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public TPTFiltersInheritanceQueryNpgsqlTest(
        TPTFiltersInheritanceQueryNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
