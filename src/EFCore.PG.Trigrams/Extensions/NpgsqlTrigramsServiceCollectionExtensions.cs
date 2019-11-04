using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// pg_trgm module extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class NpgsqlTrigramsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services required for pg_trgm support in the Npgsql provider for Entity Framework.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddEntityFrameworkNpgsqlTrigrams(
            this IServiceCollection serviceCollection)
        {
            new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAddProviderSpecificServices(
                    x => x.TryAddSingletonEnumerable<IMethodCallTranslatorPlugin, NpgsqlTrigramsMethodCallTranslatorPlugin>());

            return serviceCollection;
        }
    }
}
