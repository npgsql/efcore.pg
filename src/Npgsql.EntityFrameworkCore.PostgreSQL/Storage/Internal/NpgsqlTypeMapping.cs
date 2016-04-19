using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using Npgsql;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlTypeMapping : RelationalTypeMapping
    {
        public new NpgsqlDbType? StoreType { get; }

        internal NpgsqlTypeMapping([NotNull] string defaultTypeName, [NotNull] Type clrType, NpgsqlDbType storeType)
            : base(defaultTypeName, clrType)
        {
            StoreType = storeType;
        }

        internal NpgsqlTypeMapping([NotNull] string defaultTypeName, [NotNull] Type clrType)
            : base(defaultTypeName, clrType)
        { }

        protected override void ConfigureParameter([NotNull] DbParameter parameter)
        {
            if (StoreType.HasValue)
            {
                ((NpgsqlParameter) parameter).NpgsqlDbType = StoreType.Value;
            }
        }
    }
}
