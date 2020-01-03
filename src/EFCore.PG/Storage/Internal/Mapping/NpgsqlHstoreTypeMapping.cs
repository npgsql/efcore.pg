using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The type mapping for the PostgreSQL hstore type.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/hstore.html
    /// </remarks>
    public abstract class NpgsqlHstoreTypeMapping : NpgsqlTypeMapping
    {
        protected NpgsqlHstoreTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Hstore) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var sb = new StringBuilder("HSTORE '");
            foreach (var kv in (IReadOnlyDictionary<string, string>)value)
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

            sb.Append('\'');
            return sb.ToString();
        }
        protected static bool Compare(IReadOnlyDictionary<string, string> a, IReadOnlyDictionary<string, string> b)
        {
            if (a == null)
                return b == null;
            if (b == null)
                return false;
            if (a.Count != b.Count)
                return false;
            foreach (var kv in a)
                if (!b.TryGetValue(kv.Key, out var bValue) || kv.Value != bValue)
                    return false;
            return true;
        }
    }
}
