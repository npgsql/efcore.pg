using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlBitTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlBitTypeMapping() : base("bit", typeof(BitArray), NpgsqlDbType.Bit) {}

        protected NpgsqlBitTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlBitTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlBitTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

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
