using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal
{
    public class NpgsqlOptions : INpgsqlOptions
    {
        public void Initialize(IDbContextOptions options)
        {
            var npgsqlOptions = options.FindExtension<NpgsqlOptionsExtension>() ?? new NpgsqlOptionsExtension();

            NullFirstOrderingEnabled = npgsqlOptions.NullFirstOrdering ?? false;
            Plugins = npgsqlOptions.Plugins;
        }

        public void Validate(IDbContextOptions options)
        {
            var npgsqlOptions = options.FindExtension<NpgsqlOptionsExtension>() ?? new NpgsqlOptionsExtension();

            if (NullFirstOrderingEnabled != (npgsqlOptions.NullFirstOrdering ?? false))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(NpgsqlDbContextOptionsBuilder.OrderNullsFirst),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }

        public virtual bool NullFirstOrderingEnabled { get; private set; }

        public virtual IReadOnlyList<IEntityFrameworkNpgsqlPlugin> Plugins { get; private set; }
    }
}
