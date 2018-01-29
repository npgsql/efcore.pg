using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlLineTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlLineTypeMapping() : base("line", typeof(NpgsqlLine), NpgsqlDbType.Line) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var line = (NpgsqlLine)value;
            return $"({line.A.ToString("G17", CultureInfo.InvariantCulture)},{line.B.ToString("G17", CultureInfo.InvariantCulture)},{line.C.ToString("G17", CultureInfo.InvariantCulture)})";
        }
    }
}
