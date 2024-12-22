namespace Microsoft.EntityFrameworkCore;

public class KeysWithConvertersNpgsqlTest(KeysWithConvertersNpgsqlTest.KeysWithConvertersNpgsqlFixture fixture)
    : KeysWithConvertersTestBase<
        KeysWithConvertersNpgsqlTest.KeysWithConvertersNpgsqlFixture>(fixture)
{
    public class KeysWithConvertersNpgsqlFixture : KeysWithConvertersFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder.UseNpgsql(b => b.MinBatchSize(1));
    }
}
