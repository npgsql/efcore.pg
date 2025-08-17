namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedTableSplitting;

public class OwnedTableSplittingRelationshipsNpgsqlFixture : OwnedTableSplittingRelationalFixtureBase, ITestSqlLoggerFactory
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
