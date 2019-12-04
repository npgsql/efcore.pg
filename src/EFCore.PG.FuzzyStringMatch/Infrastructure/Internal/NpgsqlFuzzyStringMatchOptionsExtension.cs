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
    public class NpgsqlFuzzyStringMatchOptionsExtension : IDbContextOptionsExtension
    {
        public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

        public virtual void ApplyServices(IServiceCollection services) => services.AddEntityFrameworkNpgsqlFuzzyStringMatch();

        public virtual void Validate(IDbContextOptions options)
        {
            var internalServiceProvider = options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider;
            if (internalServiceProvider != null)
            {
                using var scope = internalServiceProvider.CreateScope();
                var plugins = scope.ServiceProvider.GetService<IEnumerable<IMethodCallTranslatorPlugin>>();
                if (plugins is null || !plugins.Any(s => s is NpgsqlFuzzyStringMatchMethodCallTranslatorPlugin))
                {
                    throw new InvalidOperationException($"{nameof(NpgsqlFuzzyStringMatchDbContextOptionsBuilderExtensions.UseFuzzyStringMatch)} requires {nameof(NpgsqlFuzzyStringMatchServiceCollectionExtensions.AddEntityFrameworkNpgsqlFuzzyStringMatch)} to be called on the internal service provider used.");
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
                => debugInfo["Npgsql:" + nameof(NpgsqlFuzzyStringMatchDbContextOptionsBuilderExtensions.UseFuzzyStringMatch)] = "1";

            public override string LogFragment => "using FuzzyStringMatch ";
        }
    }
}
