using System.Collections.Immutable;
using System.Text;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     The type mapping for the PostgreSQL hstore type. Supports both <see cref="Dictionary{TKey,TValue} " />
///     and <see cref="ImmutableDictionary{TKey,TValue}" /> where TKey and TValue are both strings.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/static/hstore.html
/// </remarks>
public class NpgsqlHstoreTypeMapping : NpgsqlTypeMapping
{
    private static readonly HstoreMutableComparer MutableComparerInstance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static NpgsqlHstoreTypeMapping Default { get; } = new(typeof(Dictionary<string, string>));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlHstoreTypeMapping(Type clrType)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(clrType, comparer: GetComparer(clrType)),
                "hstore"),
            NpgsqlDbType.Hstore)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlHstoreTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Hstore)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlHstoreTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var sb = new StringBuilder("HSTORE '");
        foreach (var kv in (IReadOnlyDictionary<string, string?>)value)
        {
            sb.Append('"');
            sb.Append(kv.Key); // TODO: Escape
            sb.Append("\"=>");
            if (kv.Value is null)
            {
                sb.Append("NULL");
            }
            else
            {
                sb.Append('"');
                sb.Append(kv.Value); // TODO: Escape
                sb.Append("\",");
            }
        }

        sb.Remove(sb.Length - 1, 1);

        sb.Append('\'');
        return sb.ToString();
    }

    private static ValueComparer? GetComparer(Type clrType)
    {
        if (clrType == typeof(Dictionary<string, string>))
        {
            return MutableComparerInstance;
        }

        if (clrType == typeof(ImmutableDictionary<string, string>))
        {
            // Because ImmutableDictionary is immutable, we can use the default value comparer, which doesn't
            // clone for snapshot and just does reference comparison.
            // We could compare contents here if the references are different, but that would penalize the 99% case
            // where a different reference means different contents, which would only save a very rare database update.
            return null;
        }

        throw new ArgumentException(
            $"CLR type must be {nameof(Dictionary<string, string>)} or {nameof(ImmutableDictionary<string, string>)}");
    }

    private sealed class HstoreMutableComparer() : ValueComparer<Dictionary<string, string>>(
        (a, b) => Compare(a, b),
        o => o.GetHashCode(),
        o => new Dictionary<string, string>(o))
    {
        private static bool Compare(Dictionary<string, string>? a, Dictionary<string, string>? b)
        {
            if (a is null)
            {
                return b is null;
            }

            if (b is null || a.Count != b.Count)
            {
                return false;
            }

            foreach (var kv in a)
            {
                if (!b.TryGetValue(kv.Key, out var bValue) || kv.Value != bValue)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
