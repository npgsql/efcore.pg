using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class SimpleQueryNpgsqlTest : SimpleQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/27278")]
    public override Task Aggregate_over_subquery_in_group_by_projection(bool async)
        => base.Aggregate_over_subquery_in_group_by_projection(async);
}
