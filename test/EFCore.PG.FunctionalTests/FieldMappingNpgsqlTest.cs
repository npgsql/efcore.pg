namespace Microsoft.EntityFrameworkCore;

public class FieldMappingNpgsqlTest(FieldMappingNpgsqlTest.FieldMappingNpgsqlFixture fixture)
    : FieldMappingTestBase<FieldMappingNpgsqlTest.FieldMappingNpgsqlFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class FieldMappingNpgsqlFixture : FieldMappingFixtureBase
    {
        protected override string StoreName { get; } = "FieldMapping";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
