using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite extension methods for <see cref="IServiceCollection" />.
/// </summary>
public static class NpgsqlNetTopologySuiteServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required for NetTopologySuite support in the Npgsql provider for Entity Framework.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEntityFrameworkNpgsqlNetTopologySuite(
        this IServiceCollection serviceCollection)
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<ISingletonOptions, INpgsqlNetTopologySuiteOptions>(p => p.GetRequiredService<INpgsqlNetTopologySuiteOptions>())
            .TryAdd<IRelationalTypeMappingSourcePlugin, NpgsqlNetTopologySuiteTypeMappingSourcePlugin>()
            .TryAdd<IMethodCallTranslatorPlugin, NpgsqlNetTopologySuiteMethodCallTranslatorPlugin>()
            .TryAdd<IAggregateMethodCallTranslatorPlugin, NpgsqlNetTopologySuiteAggregateMethodCallTranslatorPlugin>()
            .TryAdd<IMemberTranslatorPlugin, NpgsqlNetTopologySuiteMemberTranslatorPlugin>()
            .TryAdd<IConventionSetPlugin, NpgsqlNetTopologySuiteConventionSetPlugin>()
            .TryAddProviderSpecificServices(
                x => x.TryAddSingleton<INpgsqlNetTopologySuiteOptions, NpgsqlNetTopologySuiteOptions>());


        return serviceCollection;
    }
}