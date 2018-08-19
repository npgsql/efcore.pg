using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlVarbitTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlVarbitTypeMapping() : base("bit varying", typeof(BitArray), NpgsqlDbType.Varbit) {}

        protected NpgsqlVarbitTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Varbit) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlVarbitTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var bits = (BitArray)value;
            var sb = new StringBuilder();
            sb.Append("VARBIT B'");
            for (var i = 0; i < bits.Count; i++)
                sb.Append(bits[i] ? '1' : '0');
            sb.Append('\'');
            return sb.ToString();
        }
    }
}
