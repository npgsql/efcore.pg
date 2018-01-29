using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlHstoreTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlHstoreTypeMapping() : base("hstore", typeof(Dictionary<string, string>), NpgsqlDbType.Hstore) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var sb = new StringBuilder("'");
            foreach (var kv in (Dictionary<string, string>)value)
            {
                sb.Append('"');
                sb.Append(kv.Key);   // TODO: Escape
                sb.Append("\"=>");
                if (kv.Value == null)
                    sb.Append("NULL");
                else
                {
                    sb.Append('"');
                    sb.Append(kv.Value);   // TODO: Escape
                    sb.Append("\",");
                }
            }

            sb.Remove(sb.Length - 1, 1);

            sb.Append("'::hstore");
            return sb.ToString();
        }
    }
}
