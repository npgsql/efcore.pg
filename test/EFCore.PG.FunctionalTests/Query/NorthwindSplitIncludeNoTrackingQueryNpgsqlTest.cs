namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindSplitIncludeNoTrackingQueryNpgsqlTest : NorthwindSplitIncludeNoTrackingQueryTestBase<
    NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindSplitIncludeNoTrackingQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        // TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
    }
}
