using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// fuzzystrmatch module specific extension methods for <see cref="NpgsqlDbContextOptionsBuilder"/>.
    /// </summary>
    public static class NpgsqlFuzzyStringMatchDbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Enable fuzzystrmatch module methods.
        /// </summary>
        /// <param name="optionsBuilder">The build being used to configure Postgres.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static NpgsqlDbContextOptionsBuilder UseFuzzyStringMatch(
            this NpgsqlDbContextOptionsBuilder optionsBuilder)
        {
            var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;
            var extension = coreOptionsBuilder.Options.FindExtension<NpgsqlFuzzyStringMatchOptionsExtension>()
                ?? new NpgsqlFuzzyStringMatchOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}
