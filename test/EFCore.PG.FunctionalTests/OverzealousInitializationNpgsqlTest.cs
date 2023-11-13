using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class OverzealousInitializationNpgsqlTest
    : OverzealousInitializationTestBase<OverzealousInitializationNpgsqlTest.OverzealousInitializationNpgsqlFixture>
{
    public OverzealousInitializationNpgsqlTest(OverzealousInitializationNpgsqlFixture fixture)
        : base(fixture)
    {
    }

    public class OverzealousInitializationNpgsqlFixture : OverzealousInitializationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
