using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;

namespace Microsoft.EntityFrameworkCore;

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
