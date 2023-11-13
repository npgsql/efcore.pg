namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindJoinQueryNpgsqlTest : NorthwindJoinQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindJoinQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // #2759
    public override Task Join_local_collection_int_closure_is_cached_correctly(bool async)
        => base.Join_local_collection_int_closure_is_cached_correctly(async);
    // => Assert.ThrowsAsync<InvalidOperationException>(() => base.Join_local_collection_int_closure_is_cached_correctly(async));

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
