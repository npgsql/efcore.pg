using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal
{
    /// <summary>
    /// Enables configuring Npgsql-specific design-time services.
    /// Tools will automatically discover implementations of this interface that are in the startup assembly.
    /// </summary>
    [UsedImplicitly]
    public class NpgsqlDesignTimeServices : IDesignTimeServices
    {
        public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
            => serviceCollection
                .AddSingleton<LoggingDefinitions, NpgsqlLoggingDefinitions>()
                .AddSingleton<IRelationalTypeMappingSource, NpgsqlTypeMappingSource>()
                .AddSingleton<IDatabaseModelFactory, NpgsqlDatabaseModelFactory>()
                .AddSingleton<IProviderConfigurationCodeGenerator, NpgsqlCodeGenerator>()
                .AddSingleton<IAnnotationCodeGenerator, NpgsqlAnnotationCodeGenerator>()
                .AddSingleton<ISqlGenerationHelper, NpgsqlSqlGenerationHelper>()
                .AddSingleton<RelationalSqlGenerationHelperDependencies>();
    }
}
