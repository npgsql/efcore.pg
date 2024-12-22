namespace Microsoft.EntityFrameworkCore.Query;

public class TPTFiltersInheritanceQuerySqlServerTest : TPTFiltersInheritanceQueryTestBase<TPTFiltersInheritanceQuerySqlServerFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public TPTFiltersInheritanceQuerySqlServerTest(
        TPTFiltersInheritanceQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
