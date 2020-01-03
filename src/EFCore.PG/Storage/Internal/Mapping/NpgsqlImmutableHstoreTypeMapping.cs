using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The type mapping for the PostgreSQL hstore type to immutable .NET Dictionaries.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/hstore.html
    /// </remarks>
    public class NpgsqlImmutableHstoreTypeMapping : NpgsqlHstoreTypeMapping
    {
        static readonly HstoreComparer ComparerInstance = new HstoreComparer();

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlImmutableHstoreTypeMapping"/> class.
        /// </summary>
        public NpgsqlImmutableHstoreTypeMapping()
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(IReadOnlyDictionary<string, string>), null, ComparerInstance),
                    "hstore"
                )) {}

        protected NpgsqlImmutableHstoreTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlImmutableHstoreTypeMapping(parameters);

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

        class HstoreComparer : ValueComparer<IReadOnlyDictionary<string, string>>
        {
            public HstoreComparer() : base(
                (a, b) => Compare(a,b),
                o => o.GetHashCode(),
                o => o)
            {}
        }
    }
}
