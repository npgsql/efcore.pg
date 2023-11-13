namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindAsNoTrackingQueryNpgsqlTest : NorthwindAsNoTrackingQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindAsNoTrackingQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
    }
}
