namespace Microsoft.EntityFrameworkCore.Types;

public abstract class NpgsqlTypeFixture<T> : RelationalTypeFixtureBase<T>
{
    protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
}
