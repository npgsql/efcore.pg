using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;

[UsedImplicitly]
public class NpgsqlNodaTimeDesignTimeServices : IDesignTimeServices
{
    public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        => serviceCollection
            .AddSingleton<IRelationalTypeMappingSourcePlugin, NpgsqlNodaTimeTypeMappingSourcePlugin>()
            .AddSingleton<IProviderCodeGeneratorPlugin, NpgsqlNodaTimeCodeGeneratorPlugin>();
}