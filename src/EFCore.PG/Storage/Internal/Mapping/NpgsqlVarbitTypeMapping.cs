using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlVarbitTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlVarbitTypeMapping() : base("bit varying", typeof(BitArray), NpgsqlDbType.Varbit) {}

        protected override string GenerateNonNullSqlLiteral(object value)
            => ((BitArray)value).ToString();
    }
}
