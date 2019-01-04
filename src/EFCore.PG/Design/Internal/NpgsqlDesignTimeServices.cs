using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal
{
    [UsedImplicitly]
    public class NpgsqlDesignTimeServices : IDesignTimeServices
    {
        public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
            => serviceCollection
                .AddSingleton<IRelationalTypeMappingSource, NpgsqlTypeMappingSource>()
                .AddSingleton<IDatabaseModelFactory, NpgsqlDatabaseModelFactory>()
                .AddSingleton<IProviderConfigurationCodeGenerator, NpgsqlCodeGenerator>()
                .AddSingleton<IAnnotationCodeGenerator, NpgsqlAnnotationCodeGenerator>()
                .AddSingleton<ISqlGenerationHelper, NpgsqlSqlGenerationHelper>()
                .AddSingleton<RelationalSqlGenerationHelperDependencies>();
    }
}
