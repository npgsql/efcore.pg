using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal
{
    public class NpgsqlUnaccentOptionsExtension : IDbContextOptionsExtension
    {
        public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

        public virtual void ApplyServices(IServiceCollection services) => services.AddEntityFrameworkNpgsqlUnaccent();

        public virtual void Validate(IDbContextOptions options)
        {
            var internalServiceProvider = options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider;
            if (internalServiceProvider != null)
            {
                using var scope = internalServiceProvider.CreateScope();
                var plugins = scope.ServiceProvider.GetService<IEnumerable<IMethodCallTranslatorPlugin>>();
                if (plugins is null || !plugins.Any(s => s is NpgsqlUnaccentMethodCallTranslatorPlugin))
                {
                    throw new InvalidOperationException($"{nameof(NpgsqlUnaccentDbContextOptionsBuilderExtensions.UseUnaccent)} requires {nameof(NpgsqlUnaccentServiceCollectionExtensions.AddEntityFrameworkNpgsqlUnaccent)} to be called on the internal service provider used.");
                }
            }
        }

        sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            public override bool IsDatabaseProvider => false;

            public override long GetServiceProviderHashCode() => 0;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
                => debugInfo["Npgsql:" + nameof(NpgsqlUnaccentDbContextOptionsBuilderExtensions.UseUnaccent)] = "1";

            public override string LogFragment => "using Unaccent ";
        }
    }
}
