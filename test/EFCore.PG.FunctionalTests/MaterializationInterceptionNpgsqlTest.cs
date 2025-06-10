namespace Microsoft.EntityFrameworkCore;

public class MaterializationInterceptionNpgsqlTest(NonSharedFixture fixture) :
    MaterializationInterceptionTestBase<MaterializationInterceptionNpgsqlTest.NpgsqlLibraryContext>(fixture)
{
    public class NpgsqlLibraryContext(DbContextOptions options) : LibraryContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity30244>().OwnsMany(e => e.Settings);

            // #2548
            // modelBuilder.Entity<TestEntity30244>().OwnsMany(e => e.Settings, b => b.ToJson());
        }
    }

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
