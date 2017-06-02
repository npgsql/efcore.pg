using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using JetBrains.Annotations;
using Npgsql;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlStringTypeMapping : StringTypeMapping
    {
        readonly NpgsqlDbType _npgsqlDbType;

        public NpgsqlStringTypeMapping(string storeType, NpgsqlDbType npgsqlDbType)
            : base(storeType)
        {
            _npgsqlDbType = npgsqlDbType;
        }

        protected override void ConfigureParameter([NotNull] DbParameter parameter)
            => ((NpgsqlParameter)parameter).NpgsqlDbType = _npgsqlDbType;
    }
}
