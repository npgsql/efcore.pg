using Microsoft.EntityFrameworkCore.Infrastructure;

using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// pg_trgm module specific extension methods for <see cref="NpgsqlDbContextOptionsBuilder"/>.
    /// </summary>
    public static class NpgsqlTrgmDbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Enable pg_trgm module methods and operators.
        /// </summary>
        /// <param name="optionsBuilder">The build being used to configure Postgres.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static NpgsqlDbContextOptionsBuilder UseTrgm(
            this NpgsqlDbContextOptionsBuilder optionsBuilder)
        {
            var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

            var extension = coreOptionsBuilder.Options.FindExtension<NpgsqlTrgmOptionsExtension>()
                ?? new NpgsqlTrgmOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}
