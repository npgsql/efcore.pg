using System.Collections.Generic;
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
    public class NpgsqlMutableHstoreTypeMapping : NpgsqlHstoreTypeMapping
    {
        static readonly HstoreComparer ComparerInstance = new HstoreComparer();

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlMutableHstoreTypeMapping"/> class.
        /// </summary>
        public NpgsqlMutableHstoreTypeMapping()
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(Dictionary<string, string>), null, ComparerInstance),
                    "hstore"
                )) {}

        protected NpgsqlMutableHstoreTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlMutableHstoreTypeMapping(parameters);

        private static bool Compare(IReadOnlyDictionary<string, string> a, IReadOnlyDictionary<string, string> b)
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

        class HstoreComparer : ValueComparer<Dictionary<string, string>>
        {
            public HstoreComparer() : base(
                (a, b) => Compare(a,b),
                o => o.GetHashCode(),
                o => o == null ? null : new Dictionary<string, string>(o))
            {}
        }
    }
}
