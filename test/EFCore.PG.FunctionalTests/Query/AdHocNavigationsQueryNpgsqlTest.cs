namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocNavigationsQueryNpgsqlTest(NonSharedFixture fixture) : AdHocNavigationsQueryRelationalTestBase(fixture)
{
    // Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone', only UTC is supported.
    public override Task Reference_include_on_derived_type_with_sibling_works()
        => Assert.ThrowsAsync<DbUpdateException>(() => base.Reference_include_on_derived_type_with_sibling_works());

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
