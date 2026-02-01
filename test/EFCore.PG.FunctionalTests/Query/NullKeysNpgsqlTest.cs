namespace Microsoft.EntityFrameworkCore.Query;

public class NullKeysNpgsqlTest(NullKeysNpgsqlTest.NullKeysNpgsqlFixture fixture)
    : NullKeysTestBase<NullKeysNpgsqlTest.NullKeysNpgsqlFixture>(fixture)
{
    public class NullKeysNpgsqlFixture : NullKeysFixtureBase
    {
        protected override string StoreName { get; } = "StringsContext";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
