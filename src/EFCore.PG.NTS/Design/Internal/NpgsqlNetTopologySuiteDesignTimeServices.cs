using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal
{
    public class NpgsqlNetTopologySuiteDesignTimeServices : IDesignTimeServices
    {
        public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
            => serviceCollection
                .AddSingleton<IRelationalTypeMappingSourcePlugin, NpgsqlNetTopologySuiteTypeMappingSourcePlugin>()
                .AddSingleton<IProviderCodeGeneratorPlugin, NpgsqlNetTopologySuiteCodeGeneratorPlugin>()
                .TryAddSingleton<INpgsqlNetTopologySuiteOptions, NpgsqlNetTopologySuiteOptions>();

    }
}
