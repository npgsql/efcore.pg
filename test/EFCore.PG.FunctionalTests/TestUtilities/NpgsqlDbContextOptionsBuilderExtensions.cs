using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public static class NpgsqlDbContextOptionsBuilderExtensions
    {
        public static NpgsqlDbContextOptionsBuilder ApplyConfiguration(this NpgsqlDbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);

            optionsBuilder.CommandTimeout(NpgsqlTestStore.CommandTimeout);

            return optionsBuilder;
        }
    }
}
