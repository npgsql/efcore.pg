using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal
{
    public class NpgsqlNetTopologySuiteOptionsExtension : IDbContextOptionsExtension
    {
        bool _isGeographyDefault;
        string _logFragment;

        public NpgsqlNetTopologySuiteOptionsExtension() {}

        protected NpgsqlNetTopologySuiteOptionsExtension([NotNull] NpgsqlNetTopologySuiteOptionsExtension copyFrom)
            => _isGeographyDefault = copyFrom._isGeographyDefault;

        protected virtual NpgsqlNetTopologySuiteOptionsExtension Clone() => new NpgsqlNetTopologySuiteOptionsExtension(this);

        public virtual bool ApplyServices(IServiceCollection services)
        {
            services.AddEntityFrameworkNpgsqlNetTopologySuite();

            return false;
        }

        public virtual bool IsGeographyDefault => _isGeographyDefault;

        public virtual NpgsqlNetTopologySuiteOptionsExtension WithGeographyDefault(bool isGeographyDefault = true)
        {
            var clone = Clone();

            clone._isGeographyDefault = isGeographyDefault;

            return clone;
        }

        public virtual long GetServiceProviderHashCode() => _isGeographyDefault ? 541 : 0;

        public virtual void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            Check.NotNull(debugInfo, nameof(debugInfo));

            debugInfo["NpgsqlNetTopologySuite:" + nameof(IsGeographyDefault)]
                = (_isGeographyDefault ? 541 : 0).ToString(CultureInfo.InvariantCulture);
        }

        public virtual void Validate(IDbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            var internalServiceProvider = options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider;
            if (internalServiceProvider != null)
            {
                using (var scope = internalServiceProvider.CreateScope())
                {
                    if (scope.ServiceProvider.GetService<IEnumerable<IRelationalTypeMappingSourcePlugin>>()
                            ?.Any(s => s is NpgsqlNetTopologySuiteTypeMappingSourcePlugin) != true)
                    {
                        throw new InvalidOperationException($"{nameof(NpgsqlNetTopologySuiteDbContextOptionsBuilderExtensions.UseNetTopologySuite)} requires {nameof(NpgsqlNetTopologySuiteServiceCollectionExtensions.AddEntityFrameworkNpgsqlNetTopologySuite)} to be called on the internal service provider used.");
                    }
                }
            }
        }

        [NotNull]
        public virtual string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder("using NetTopologySuite");
                    if (_isGeographyDefault)
                        builder.Append(" (geography by default)");
                    builder.Append(' ');

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }
    }
}
