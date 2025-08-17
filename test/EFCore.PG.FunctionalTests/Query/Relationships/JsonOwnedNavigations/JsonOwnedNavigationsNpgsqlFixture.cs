namespace Microsoft.EntityFrameworkCore.Query.Relationships.JsonOwnedNavigations;

public class JsonOwnedNavigationsNpgsqlFixture : JsonOwnedNavigationsRelationalFixtureBase, ITestSqlLoggerFactory
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
