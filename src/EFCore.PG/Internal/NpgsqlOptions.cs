using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class NpgsqlOptions : INpgsqlOptions
    {
        public void Initialize(IDbContextOptions options)
        {
            var npgsqlOptions = options.FindExtension<NpgsqlOptionsExtension>() ?? new NpgsqlOptionsExtension();

            NullFirstOrderingEnabled = npgsqlOptions.NullFirstOrdering ?? false;
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
    }
}
