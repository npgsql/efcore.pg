using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class FieldsOnlyLoadNpgsqlTest : FieldsOnlyLoadTestBase<FieldsOnlyLoadNpgsqlTest.FieldsOnlyLoadNpgsqlFixture>
{
    public FieldsOnlyLoadNpgsqlTest(FieldsOnlyLoadNpgsqlFixture fixture)
        : base(fixture)
    {
    }

    public class FieldsOnlyLoadNpgsqlFixture : FieldsOnlyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
