using System.Collections;
using System.Text;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for PostgreSQL multirange types.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/rangetypes.html
/// </remarks>
public class NpgsqlMultirangeTypeMapping : NpgsqlTypeMapping
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    /// <summary>
    /// The relational type mapping of the ranges contained in this multirange.
    /// </summary>
    public virtual NpgsqlRangeTypeMapping RangeMapping { get; }

    /// <summary>
    /// The relational type mapping of the values contained in this multirange.
    /// </summary>
    public virtual RelationalTypeMapping SubtypeMapping { get; }

    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlRangeTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="rangeMapping">The type mapping of the ranges contained in this multirange.</param>
    /// <param name="sqlGenerationHelper">The SQL generation helper to delimit the store name.</param>
    public NpgsqlMultirangeTypeMapping(
        string storeType,
        Type clrType,
        NpgsqlRangeTypeMapping rangeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : this(storeType, storeTypeSchema: null, clrType, rangeMapping, sqlGenerationHelper) {}

    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlRangeTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map</param>
    /// <param name="storeTypeSchema">The schema of the type.</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="rangeMapping">The type mapping of the ranges contained in this multirange.</param>
    /// <param name="sqlGenerationHelper">The SQL generation helper to delimit the store name.</param>
    public NpgsqlMultirangeTypeMapping(
        string storeType,
        string? storeTypeSchema,
        Type clrType,
        NpgsqlRangeTypeMapping rangeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(
            sqlGenerationHelper.DelimitIdentifier(storeType, storeTypeSchema), clrType,
            GenerateNpgsqlDbType(rangeMapping.SubtypeMapping))
    {
        RangeMapping = rangeMapping;
        SubtypeMapping = rangeMapping.SubtypeMapping;
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlMultirangeTypeMapping(
        RelationalTypeMappingParameters parameters,
        NpgsqlDbType npgsqlDbType,
        NpgsqlRangeTypeMapping rangeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(parameters, npgsqlDbType)
    {
        RangeMapping = rangeMapping;
        SubtypeMapping = rangeMapping.SubtypeMapping;
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlMultirangeTypeMapping(parameters, NpgsqlDbType, RangeMapping, _sqlGenerationHelper);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => GenerateNonNullSqlLiteral(value, RangeMapping, StoreType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string GenerateNonNullSqlLiteral(object value, RelationalTypeMapping rangeMapping, string multirangeStoreType)
    {
        var multirange = (IList)value;

        var sb = new StringBuilder();
        sb.Append("'{");

        for (var i = 0; i < multirange.Count; i++)
        {
            sb.Append(rangeMapping.GenerateEmbeddedSqlLiteral(multirange[i]));
            if (i < multirange.Count - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append("}'::");
        sb.Append(multirangeStoreType);
        return sb.ToString();
    }

    private static NpgsqlDbType GenerateNpgsqlDbType(RelationalTypeMapping subtypeMapping)
    {
        NpgsqlDbType subtypeNpgsqlDbType;
        if (subtypeMapping is INpgsqlTypeMapping npgsqlTypeMapping)
        {
            subtypeNpgsqlDbType = npgsqlTypeMapping.NpgsqlDbType;
        }
        else
        {
            // We're using a built-in, non-Npgsql mapping such as IntTypeMapping.
            // Infer the NpgsqlDbType from the DbType (somewhat hacky but why not).
            Debug.Assert(subtypeMapping.DbType.HasValue);
            var p = new NpgsqlParameter { DbType = subtypeMapping.DbType.Value };
            subtypeNpgsqlDbType = p.NpgsqlDbType;
        }

        return NpgsqlDbType.Multirange | subtypeNpgsqlDbType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        // Note that arrays are handled in EF Core's CSharpHelper, so this method doesn't get called for them.

        // Unfortunately, List<NpgsqlRange<T>> requires MemberInit, which CSharpHelper doesn't support
        var type = value.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            throw new NotSupportedException("Cannot generate code literals for List<T>, consider using arrays instead");
        }

        throw new InvalidCastException();
    }
}
