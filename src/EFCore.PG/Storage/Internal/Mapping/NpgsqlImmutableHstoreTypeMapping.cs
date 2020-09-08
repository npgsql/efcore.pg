using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The type mapping for the PostgreSQL hstore type to <see cref="ImmutableDictionary"/>.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/hstore.html
    /// </remarks>
    public class NpgsqlImmutableHstoreTypeMapping : NpgsqlHstoreTypeMapping
    {
        static readonly HstoreComparer ComparerInstance = new HstoreComparer();

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlReadOnlyHstoreTypeMapping"/> class.
        /// </summary>
        public NpgsqlImmutableHstoreTypeMapping()
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(ImmutableDictionary<string, string>), null, ComparerInstance),
                    "hstore"
                )) {}

        protected NpgsqlImmutableHstoreTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlImmutableHstoreTypeMapping(parameters);

        class HstoreComparer : ValueComparer<ImmutableDictionary<string, string>>
        {
            public HstoreComparer() : base(
                // We could compare contents here if the references are different, but that would penalize the 99% case
                // where a different reference means different contents, which would only save a very rare database update.
                (a, b) => ReferenceEquals(a, b),
                o => o.GetHashCode(),
                o => o)
            {}
        }
    }
}
