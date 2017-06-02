using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlTypeMapping : RelationalTypeMapping
    {
        internal NpgsqlTypeMapping([NotNull] string storeType, [NotNull] Type clrType, NpgsqlDbType npgsqlDbType)
            : base(storeType, clrType, unicode: false, size: null, dbType: null)
        {
            NpgsqlDbType = npgsqlDbType;
        }

        internal NpgsqlTypeMapping([NotNull] string storeType, [NotNull] Type clrType)
            : base(storeType, clrType, unicode: false, size: null, dbType: null)
        {}

        public NpgsqlDbType? NpgsqlDbType { get; protected set; }

        protected override void ConfigureParameter([NotNull] DbParameter parameter)
        {
            if (NpgsqlDbType.HasValue)
                ((NpgsqlParameter)parameter).NpgsqlDbType = NpgsqlDbType.Value;
        }
    }
}
