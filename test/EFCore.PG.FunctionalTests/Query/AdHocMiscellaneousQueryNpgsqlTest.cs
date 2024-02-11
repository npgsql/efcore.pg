using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class AdHocMiscellaneousQueryNpgsqlTest : AdHocMiscellaneousQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override void Seed2951(Context2951 context)
        => context.Database.ExecuteSqlRaw(
            """
CREATE TABLE "ZeroKey" ("Id" int);
INSERT INTO "ZeroKey" VALUES (NULL)
""");

    // https://github.com/dotnet/efcore/pull/32542/files#r1485633978
    public override Task Nested_queries_does_not_cause_concurrency_exception_sync(bool tracking)
        => Assert.ThrowsAsync<NpgsqlOperationInProgressException>(
            () => base.Nested_queries_does_not_cause_concurrency_exception_sync(tracking));

    // https://github.com/dotnet/efcore/pull/32542/files#r1485633978
    public override Task Select_nested_projection()
        => Assert.ThrowsAsync<NpgsqlOperationInProgressException>(
            () => base.Select_nested_projection());

    // Writes DateTime with Kind=Unspecified to timestamptz
    public override Task SelectMany_where_Select(bool async)
        => Task.CompletedTask;

    // Writes DateTime with Kind=Unspecified to timestamptz
    public override Task Subquery_first_member_compared_to_null(bool async)
        => Task.CompletedTask;

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/27995/files#r874038747")]
    public override Task StoreType_for_UDF_used(bool async)
        => base.StoreType_for_UDF_used(async);
}
