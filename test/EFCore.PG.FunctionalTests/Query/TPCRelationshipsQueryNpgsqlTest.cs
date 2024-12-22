namespace Microsoft.EntityFrameworkCore.Query;

public class TPCRelationshipsQueryNpgsqlTest
    : TPCRelationshipsQueryTestBase<TPCRelationshipsQueryNpgsqlTest.TPCRelationshipsQueryNpgsqlFixture>
{
    public TPCRelationshipsQueryNpgsqlTest(
        TPCRelationshipsQueryNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public class TPCRelationshipsQueryNpgsqlFixture : TPCRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
