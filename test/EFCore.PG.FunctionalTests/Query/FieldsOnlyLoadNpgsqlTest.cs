namespace Microsoft.EntityFrameworkCore.Query;

public class FieldsOnlyLoadNpgsqlTest(FieldsOnlyLoadNpgsqlTest.FieldsOnlyLoadNpgsqlFixture fixture)
    : FieldsOnlyLoadTestBase<FieldsOnlyLoadNpgsqlTest.FieldsOnlyLoadNpgsqlFixture>(fixture)
{
    public class FieldsOnlyLoadNpgsqlFixture : FieldsOnlyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
