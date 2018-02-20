using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlPathTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPathTypeMapping() : base("path", typeof(NpgsqlPath), NpgsqlDbType.Path) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var path = (NpgsqlPath)value;
            var sb = new StringBuilder();
            sb.Append(path.Open ? '[' : '(');
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
            sb.Append(path.Open ? ']' : ')');
            return sb.ToString();
        }
    }
}
