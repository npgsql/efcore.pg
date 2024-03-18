using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class CompositeKeyEndToEndNpgsqlTest(CompositeKeyEndToEndNpgsqlTest.CompositeKeyEndToEndNpgsqlFixture fixture)
    : CompositeKeyEndToEndTestBase<CompositeKeyEndToEndNpgsqlTest.CompositeKeyEndToEndNpgsqlFixture>(fixture)
{
    public class CompositeKeyEndToEndNpgsqlFixture : CompositeKeyEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
