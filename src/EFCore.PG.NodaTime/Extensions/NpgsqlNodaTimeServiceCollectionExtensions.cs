using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class NpgsqlNodaTimeServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services required for NodaTime support in the Npgsql provider for Entity Framework.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddEntityFrameworkNpgsqlNodaTime(
            [NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAddProviderSpecificServices(
                    x => x
                        .TryAddSingletonEnumerable<IRelationalTypeMappingSourcePlugin, NpgsqlNodaTimeTypeMappingSourcePlugin>()
                        .TryAddSingletonEnumerable<IMethodCallTranslatorPlugin, NpgsqlNodaTimeMethodCallTranslatorPlugin>()
                        .TryAddSingletonEnumerable<IMemberTranslatorPlugin, NpgsqlNodaTimeMemberTranslatorPlugin>()
                        .TryAddSingletonEnumerable<IEvaluatableExpressionFilterPlugin, NpgsqlNodaTimeEvaluatableExpressionFilterPlugin>());

            return serviceCollection;
        }
    }
}
