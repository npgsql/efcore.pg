using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class FieldsOnlyLoadNpgsqlTest(FieldsOnlyLoadNpgsqlTest.FieldsOnlyLoadNpgsqlFixture fixture)
    : FieldsOnlyLoadTestBase<FieldsOnlyLoadNpgsqlTest.FieldsOnlyLoadNpgsqlFixture>(fixture)
{
    public class FieldsOnlyLoadNpgsqlFixture : FieldsOnlyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
