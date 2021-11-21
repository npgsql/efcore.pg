using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;

public class NpgsqlDesignTimeServices : IDesignTimeServices
{
    public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        serviceCollection.AddEntityFrameworkNpgsql();
        new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)
            .TryAdd<IAnnotationCodeGenerator, NpgsqlAnnotationCodeGenerator>()
            .TryAdd<IDatabaseModelFactory, NpgsqlDatabaseModelFactory>()
            .TryAdd<IProviderConfigurationCodeGenerator, NpgsqlCodeGenerator>()
            .TryAddCoreServices();
    }
}