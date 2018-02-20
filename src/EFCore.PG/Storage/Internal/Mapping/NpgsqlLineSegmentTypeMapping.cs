using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlLineSegmentTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlLineSegmentTypeMapping() : base("lseg", typeof(NpgsqlLSeg), NpgsqlDbType.LSeg) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var lseg = (NpgsqlLSeg)value;
            var x1 = lseg.Start.X.ToString("G17", CultureInfo.InvariantCulture);
            var y1 = lseg.Start.Y.ToString("G17", CultureInfo.InvariantCulture);
            var x2 = lseg.End.X.ToString("G17", CultureInfo.InvariantCulture);
            var y2 = lseg.End.Y.ToString("G17", CultureInfo.InvariantCulture);
            return $"(({x1}, {y2}), ({x2}, {y2}))";
        }
    }
}
