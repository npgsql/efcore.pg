namespace Microsoft.EntityFrameworkCore;

public abstract class FindNpgsqlTest : FindTestBase<FindNpgsqlTest.FindNpgsqlFixture>
{
    protected FindNpgsqlTest(FindNpgsqlFixture fixture)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public class FindNpgsqlTestSet(FindNpgsqlFixture fixture) : FindNpgsqlTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaSetFinder();
    }

    public class FindNpgsqlTestContext(FindNpgsqlFixture fixture) : FindNpgsqlTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaContextFinder();
    }

    public class FindNpgsqlTestNonGeneric(FindNpgsqlFixture fixture) : FindNpgsqlTest(fixture)
    {
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
