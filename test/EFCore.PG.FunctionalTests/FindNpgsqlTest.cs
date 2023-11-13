using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public abstract class FindNpgsqlTest : FindTestBase<FindNpgsqlTest.FindNpgsqlFixture>
{
    protected FindNpgsqlTest(FindNpgsqlFixture fixture)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public class FindNpgsqlTestSet : FindNpgsqlTest
    {
        public FindNpgsqlTestSet(FindNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaSetFinder();
    }

    public class FindNpgsqlTestContext : FindNpgsqlTest
    {
        public FindNpgsqlTestContext(FindNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaContextFinder();
    }

    public class FindNpgsqlTestNonGeneric : FindNpgsqlTest
    {
        public FindNpgsqlTestNonGeneric(FindNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaNonGenericContextFinder();
    }

    public class FindNpgsqlFixture : FindFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
