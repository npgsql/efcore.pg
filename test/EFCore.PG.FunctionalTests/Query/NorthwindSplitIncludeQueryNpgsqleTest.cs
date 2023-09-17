using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindSplitIncludeQueryNpgsqlTest : NorthwindSplitIncludeQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindSplitIncludeQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        // TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
    }

    [SkipForCockroachDb("https://github.com/dotnet/efcore/issues/26808")]
    public override Task Include_collection_skip_no_order_by(bool async)
    {
        return base.Include_collection_skip_no_order_by(async);
    }

    [SkipForCockroachDb("https://github.com/dotnet/efcore/issues/26808")]
    public override Task Include_collection_skip_take_no_order_by(bool async)
    {
        return base.Include_collection_skip_take_no_order_by(async);
    }

    [SkipForCockroachDb("https://github.com/dotnet/efcore/issues/26808")]
    public override Task Include_collection_take_no_order_by(bool async)
    {
        return base.Include_collection_take_no_order_by(async);
    }

    [SkipForCockroachDb("https://github.com/dotnet/efcore/issues/26808")]
    public override Task SelectMany_Include_collection_GroupBy_Select(bool async)
    {
        return base.SelectMany_Include_collection_GroupBy_Select(async);
    }
}
