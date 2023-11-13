using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class QueryFilterFuncletizationNpgsqlTest
    : QueryFilterFuncletizationTestBase<QueryFilterFuncletizationNpgsqlTest.QueryFilterFuncletizationNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public QueryFilterFuncletizationNpgsqlTest(
        QueryFilterFuncletizationNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    public override void DbContext_list_is_parameterized()
    {
        using var context = CreateContext();

        // The standard EF Core implementation expands the query filter-referenced list client side, and so generates an NRE
        // when the list is null. We translate to server-side with PostgresAnyExpression, so no exception is thrown.
        // Assert.Throws<NullReferenceException>(() => context.Set<ListFilter>().ToList());

        context.TenantIds = new List<int>();
        var query = context.Set<ListFilter>().ToList();
        Assert.Empty(query);

        context.TenantIds = new List<int> { 1 };
        query = context.Set<ListFilter>().ToList();
        Assert.Single(query);

        context.TenantIds = new List<int> { 2, 3 };
        query = context.Set<ListFilter>().ToList();
        Assert.Equal(2, query.Count);
    }

    public class QueryFilterFuncletizationNpgsqlFixture : QueryFilterFuncletizationRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
