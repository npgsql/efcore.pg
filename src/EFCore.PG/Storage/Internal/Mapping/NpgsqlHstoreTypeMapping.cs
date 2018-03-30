using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlHstoreTypeMapping : NpgsqlTypeMapping
    {
        static readonly HstoreComparer ComparerInstance = new HstoreComparer();

        public NpgsqlHstoreTypeMapping()
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(Dictionary<string, string>), null, ComparerInstance),
                    "hstore"
                ), NpgsqlDbType.Hstore) {}

        protected NpgsqlHstoreTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlHstoreTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlHstoreTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

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
                o => o.GetHashCode(),
                o => o == null ? null : new Dictionary<string, string>(o))
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
        }
    }
}
