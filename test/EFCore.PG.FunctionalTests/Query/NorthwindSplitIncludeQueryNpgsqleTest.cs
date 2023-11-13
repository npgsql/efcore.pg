namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindSplitIncludeQueryNpgsqlTest : NorthwindSplitIncludeQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindSplitIncludeQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        // TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
    }
}
