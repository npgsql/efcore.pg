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
        public NpgsqlDbType? NpgsqlDbType { get; protected set; }

        readonly Type _type;

        internal NpgsqlTypeMapping([NotNull] string storeType, [NotNull] Type clrType, NpgsqlDbType? npgsqlDbType = null)
            : base(storeType, clrType, unicode: false, size: null, dbType: null)
        {
            _type = clrType;
            NpgsqlDbType = npgsqlDbType;
        }

        protected override void ConfigureParameter([NotNull] DbParameter parameter)
        {
            if (NpgsqlDbType.HasValue)
                ((NpgsqlParameter)parameter).NpgsqlDbType = NpgsqlDbType.Value;
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlTypeMapping(storeType, _type, NpgsqlDbType);
    }
}
