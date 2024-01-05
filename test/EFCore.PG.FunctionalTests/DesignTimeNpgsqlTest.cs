using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class DesignTimeNpgsqlTest(DesignTimeNpgsqlTest.DesignTimeNpgsqlFixture fixture)
    : DesignTimeTestBase<DesignTimeNpgsqlTest.DesignTimeNpgsqlFixture>(fixture)
{
    protected override Assembly ProviderAssembly
        => typeof(NpgsqlDesignTimeServices).Assembly;

    public class DesignTimeNpgsqlFixture : DesignTimeFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
