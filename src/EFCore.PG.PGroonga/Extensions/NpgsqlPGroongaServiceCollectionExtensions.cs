using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Npgsql.EntityFrameworkCore.PostgreSQL.PGroonga extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class NpgsqlPGroongaServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services required for PGroonga support in the Npgsql provider for Entity Framework.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddEntityFrameworkNpgsqlPGroonga(
            [NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAddProviderSpecificServices(
                    x => x
                        .TryAddSingletonEnumerable<IMethodCallTranslatorPlugin, NpgsqlPGroongaMethodCallTranslatorPlugin>());

            return serviceCollection;
        }
    }
}
