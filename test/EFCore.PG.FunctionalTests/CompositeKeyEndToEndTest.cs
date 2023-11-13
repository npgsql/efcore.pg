using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class CompositeKeyEndToEndNpgsqlTest : CompositeKeyEndToEndTestBase<CompositeKeyEndToEndNpgsqlTest.CompositeKeyEndToEndNpgsqlFixture>
{
    public CompositeKeyEndToEndNpgsqlTest(CompositeKeyEndToEndNpgsqlFixture fixture)
        : base(fixture)
    {
    }

    public class CompositeKeyEndToEndNpgsqlFixture : CompositeKeyEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
