using System;
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

            ReverseNullOrderingEnabled = npgsqlOptions.ReverseNullOrdering ?? false;
        }

        public void Validate(IDbContextOptions options)
        {
            var npgsqlOptions = options.FindExtension<NpgsqlOptionsExtension>() ?? new NpgsqlOptionsExtension();

            if (ReverseNullOrderingEnabled != (npgsqlOptions.ReverseNullOrdering ?? false))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(NpgsqlDbContextOptionsBuilder.ReverseNullOrdering),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }

        public virtual bool ReverseNullOrderingEnabled { get; private set; }
    }
}
