using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// unaccent module specific extension methods for <see cref="NpgsqlDbContextOptionsBuilder"/>.
    /// </summary>
    public static class NpgsqlUnaccentDbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Enable unaccent module methods and operators.
        /// </summary>
        /// <param name="optionsBuilder">The build being used to configure Postgres.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static NpgsqlDbContextOptionsBuilder UseUnaccent(
            this NpgsqlDbContextOptionsBuilder optionsBuilder)
        {
            var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

            var extension = coreOptionsBuilder.Options.FindExtension<NpgsqlUnaccentOptionsExtension>()
                ?? new NpgsqlUnaccentOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}
