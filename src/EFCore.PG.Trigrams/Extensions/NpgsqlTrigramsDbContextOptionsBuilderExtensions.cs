using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// pg_trgm module specific extension methods for <see cref="NpgsqlDbContextOptionsBuilder"/>.
    /// </summary>
    public static class NpgsqlTrigramsDbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Enable pg_trgm module methods and operators.
        /// </summary>
        /// <param name="optionsBuilder">The build being used to configure Postgres.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static NpgsqlDbContextOptionsBuilder UseTrigrams(
            this NpgsqlDbContextOptionsBuilder optionsBuilder)
        {
            var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

            var extension = coreOptionsBuilder.Options.FindExtension<NpgsqlTrigramsOptionsExtension>()
                ?? new NpgsqlTrigramsOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}
