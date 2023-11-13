using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class SeedingNpgsqlTest : SeedingTestBase
{
    protected override TestStore TestStore
        => NpgsqlTestStore.Create("SeedingTest");

    protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
        => new SeedingNpgsqlContext(testId);

    protected class SeedingNpgsqlContext : SeedingContext
    {
        public SeedingNpgsqlContext(string testId)
            : base(testId)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(NpgsqlTestStore.CreateConnectionString($"Seeds{TestId}"));
    }
}
