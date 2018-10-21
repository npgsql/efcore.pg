using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal
{
    /// <inheritdoc />
    public class NpgsqlOptions : INpgsqlOptions
    {
        /// <inheritdoc />
        public virtual Version PostgresVersion { get; private set; }

        /// <inheritdoc />
        public virtual bool ReverseNullOrderingEnabled { get; private set; }

        /// <inheritdoc />
        [NotNull]
        public virtual IReadOnlyList<RangeMappingInfo> RangeMappings { get; private set; }

        /// <inheritdoc />
        [NotNull]
        public virtual IReadOnlyList<NpgsqlEntityFrameworkPlugin> Plugins { get; private set; }

        public NpgsqlOptions()
        {
            RangeMappings = new RangeMappingInfo[0];
            Plugins = new NpgsqlEntityFrameworkPlugin[0];
        }

        /// <inheritdoc />
        public void Initialize(IDbContextOptions options)
        {
            var npgsqlOptions = options.FindExtension<NpgsqlOptionsExtension>() ?? new NpgsqlOptionsExtension();

            PostgresVersion = npgsqlOptions.PostgresVersion;
            ReverseNullOrderingEnabled = npgsqlOptions.ReverseNullOrdering;
            Plugins = npgsqlOptions.Plugins;
            RangeMappings = npgsqlOptions.RangeMappings;
        }

        /// <inheritdoc />
        public void Validate(IDbContextOptions options)
        {
            var npgsqlOptions = options.FindExtension<NpgsqlOptionsExtension>() ?? new NpgsqlOptionsExtension();

            if (ReverseNullOrderingEnabled != npgsqlOptions.ReverseNullOrdering)
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(NpgsqlDbContextOptionsBuilder.ReverseNullOrdering),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }
    }
}
