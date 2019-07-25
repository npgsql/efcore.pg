using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class SeedingNpgsqlTest : SeedingTestBase
    {
        protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
        {
            var context = new SeedingNpgsqlContext(testId);

            context.Database.EnsureClean();

            return context;
        }

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
}
