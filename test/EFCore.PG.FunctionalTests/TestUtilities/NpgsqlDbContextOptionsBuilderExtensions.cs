using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class NpgsqlDbContextOptionsBuilderExtensions
{
    public static NpgsqlDbContextOptionsBuilder ApplyConfiguration(this NpgsqlDbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);

        optionsBuilder.ExecutionStrategy(d => new TestNpgsqlRetryingExecutionStrategy(d));

        optionsBuilder.CommandTimeout(NpgsqlTestStore.CommandTimeout);

        return optionsBuilder;
    }
}
