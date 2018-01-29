using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlBitTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlBitTypeMapping() : base("bit", typeof(BitArray), NpgsqlDbType.Bit) {}

        protected override string GenerateNonNullSqlLiteral(object value)
            => ((BitArray)value).ToString();
    }
}
