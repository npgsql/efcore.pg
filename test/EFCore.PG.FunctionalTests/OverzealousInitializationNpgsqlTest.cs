namespace Microsoft.EntityFrameworkCore;

public class OverzealousInitializationNpgsqlTest(OverzealousInitializationNpgsqlTest.OverzealousInitializationNpgsqlFixture fixture)
    : OverzealousInitializationTestBase<OverzealousInitializationNpgsqlTest.OverzealousInitializationNpgsqlFixture>(fixture)
{
    public class OverzealousInitializationNpgsqlFixture : OverzealousInitializationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
