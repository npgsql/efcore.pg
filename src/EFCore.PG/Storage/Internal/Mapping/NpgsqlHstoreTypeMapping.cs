using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The type mapping for the PostgreSQL hstore type. Supports both <see cref="Dictionary{TKey,TValue} "/>
    /// and <see cref="ImmutableDictionary{TKey,TValue}" /> over strings.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/hstore.html
    /// </remarks>
    public class NpgsqlHstoreTypeMapping : NpgsqlTypeMapping
    {
        static readonly HstoreMutableComparer MutableComparerInstance = new HstoreMutableComparer();

        public NpgsqlHstoreTypeMapping([NotNull] Type clrType)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(clrType, comparer: GetComparer(clrType)),
                    "hstore"),
                NpgsqlDbType.Hstore)
        {
        }

        protected NpgsqlHstoreTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Hstore) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlHstoreTypeMapping(parameters);

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

        static ValueComparer GetComparer(Type clrType)
        {
            if (clrType == typeof(Dictionary<string, string>))
                return MutableComparerInstance;

            if (clrType == typeof(ImmutableDictionary<string, string>))
            {
                // Because ImmutableDictionary is immutable, we can use the default value comparer, which doesn't
                // clone for snapshot and just does reference comparison.
                // We could compare contents here if the references are different, but that would penalize the 99% case
                // where a different reference means different contents, which would only save a very rare database update.
                return null;
            }

            throw new ArgumentException($"CLR type must be {nameof(Dictionary<string,string>)} or {nameof(ImmutableDictionary<string,string>)}");
        }

        sealed class HstoreMutableComparer : ValueComparer<Dictionary<string, string>>
        {
            public HstoreMutableComparer() : base(
                (a, b) => Compare(a,b),
                o => o.GetHashCode(),
                o => o == null ? null : new Dictionary<string, string>(o))
            {}

            static bool Compare(Dictionary<string, string> a, Dictionary<string, string> b)
            {
                if (a is null)
                    return b is null;
                if (b is null)
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
