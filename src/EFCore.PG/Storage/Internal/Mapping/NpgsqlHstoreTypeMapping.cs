using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlHstoreTypeMapping : NpgsqlTypeMapping
    {
        static readonly HstoreComparer ComparerInstance = new HstoreComparer();

        public NpgsqlHstoreTypeMapping()
            : base("hstore", typeof(Dictionary<string, string>), null, ComparerInstance, NpgsqlDbType.Hstore) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var sb = new StringBuilder("HSTORE '");
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

            sb.Append('\'');
            return sb.ToString();
        }

        class HstoreComparer : ValueComparer<Dictionary<string, string>>
        {
            public HstoreComparer() : base(
                (a, b) => Compare(a,b),
                source => Snapshot(source))
            {}

            static bool Compare(Dictionary<string, string> a, Dictionary<string, string> b)
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

            static Dictionary<string, string> Snapshot(Dictionary<string, string> source)
                => source == null ? null : new Dictionary<string, string>(source);
        }
    }
}
