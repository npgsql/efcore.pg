using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class AdHocNavigationsQueryNpgsqlTest : AdHocNavigationsQueryRelationalTestBase
{
    // Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone', only UTC is supported.
    public override Task Reference_include_on_derived_type_with_sibling_works()
        => Assert.ThrowsAsync<DbUpdateException>(() => base.Reference_include_on_derived_type_with_sibling_works());

    // https://github.com/dotnet/efcore/pull/32542/files#r1485618022
    public override Task Nested_include_queries_do_not_populate_navigation_twice()
        => Assert.ThrowsAsync<NpgsqlOperationInProgressException>(() => base.Nested_include_queries_do_not_populate_navigation_twice());

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
