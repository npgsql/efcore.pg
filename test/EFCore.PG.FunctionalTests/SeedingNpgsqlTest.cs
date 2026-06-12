namespace Microsoft.EntityFrameworkCore;

public class SeedingNpgsqlTest : SeedingTestBase
{
    protected override TestStore TestStore
        => NpgsqlTestStore.Create("SeedingTest");

    protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
        => new SeedingNpgsqlContext(testId);

    protected class SeedingNpgsqlContext(string testId) : SeedingContext(testId)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(NpgsqlTestStore.CreateConnectionString($"Seeds{TestId}"));
    }
}
