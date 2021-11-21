using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class SharedTypeQueryNpgsqlTest : SharedTypeQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

    public override Task Can_use_shared_type_entity_type_in_query_filter_with_from_sql(bool async)
        => Task.CompletedTask; // https://github.com/dotnet/efcore/issues/25661
}