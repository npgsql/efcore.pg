using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal
{
    public class NpgsqlNodaTimeOptionsExtension : IDbContextOptionsExtension
    {
        public virtual string LogFragment => "using NodaTime ";

        public virtual bool ApplyServices(IServiceCollection services)
        {
            services.AddEntityFrameworkNpgsqlNodaTime();

            return false;
        }

        public virtual long GetServiceProviderHashCode() => 0;

        public void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }

        public virtual void Validate(IDbContextOptions options)
        {
            var internalServiceProvider = options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider;
            if (internalServiceProvider != null)
            {
                using (var scope = internalServiceProvider.CreateScope())
                {
                    if (scope.ServiceProvider.GetService<IEnumerable<IRelationalTypeMappingSourcePlugin>>()
                            ?.Any(s => s is NpgsqlNodaTimeTypeMappingSourcePlugin) != true)
                    {
                        throw new InvalidOperationException($"{nameof(NpgsqlNodaTimeDbContextOptionsBuilderExtensions.UseNodaTime)} requires {nameof(NpgsqlNodaTimeServiceCollectionExtensions.AddEntityFrameworkNpgsqlNodaTime)} to be called on the internal service provider used.");
                    }
                }
            }
        }
    }
}
