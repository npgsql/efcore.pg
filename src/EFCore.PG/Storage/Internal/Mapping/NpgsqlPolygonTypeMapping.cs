using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlPolygonTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPolygonTypeMapping() : base("polygon", typeof(NpgsqlPolygon), NpgsqlDbType.Polygon) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var path = (NpgsqlPath)value;
            var sb = new StringBuilder();
            sb.Append('(');
            for (var i = 0; i < path.Count; i++)
            {
                sb.Append('(');
                sb.Append(path[i].X.ToString("G17", CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(path[i].Y.ToString("G17", CultureInfo.InvariantCulture));
                sb.Append(')');
                if (i < path.Count - 1)
                    sb.Append(',');
            }
            sb.Append(')');
            return sb.ToString();
        }
    }
}
