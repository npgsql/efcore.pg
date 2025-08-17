namespace Microsoft.EntityFrameworkCore;

public class ComplexTypesTrackingNpgsqlTest(ComplexTypesTrackingNpgsqlTest.NpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexTypesTrackingRelationalTestBase<ComplexTypesTrackingNpgsqlTest.NpgsqlFixture>(fixture, testOutputHelper)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    // 'timestamp with time zone' literal cannot be generated for Unspecified DateTime: a UTC DateTime is required
    public override Task Can_track_entity_with_complex_property_bag_collections(EntityState state, bool async)
        => Task.CompletedTask;

    public class NpgsqlFixture : RelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
