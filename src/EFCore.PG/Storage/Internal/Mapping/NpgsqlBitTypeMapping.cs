using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlBitTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlBitTypeMapping() : base("bit", typeof(BitArray), NpgsqlDbType.Bit) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var bits = (BitArray)value;
            var sb = new StringBuilder();
            sb.Append("BIT B'");
            for (var i = 0; i < bits.Count; i++)
                sb.Append(bits[i] ? '1' : '0');
            sb.Append('\'');
            return sb.ToString();

        }    }
}
