namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public class NavigationsNpgsqlFixture : NavigationsRelationalFixtureBase, ITestSqlLoggerFactory
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
