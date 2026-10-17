using System.Text.Json;
using System.Text.Json.Serialization;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlJsonPocoTranslator(
    IRelationalTypeMappingSource typeMappingSource,
    NpgsqlSqlExpressionFactory sqlExpressionFactory,
    IModel model)
    : IMemberTranslator, IMethodCallTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource = typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory = sqlExpressionFactory;
    private readonly RelationalTypeMapping _stringTypeMapping = typeMappingSource.FindMapping(typeof(string), model)!;
    private readonly IModel _model = model;

    private static readonly ValueConverter<object, JsonElement> ObjectJsonConverter = new(
        value => JsonSerializer.SerializeToElement(value, (JsonSerializerOptions?)null),
        value => value.ValueKind == JsonValueKind.Null ? null! : (object)value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is not null
            && method is { Name: "get_Item", DeclaringType.IsGenericType: true }
            && arguments is [var key]
            && key.Type == typeof(string)
            && method.DeclaringType.GetGenericTypeDefinition() is var declaringType
            && (declaringType == typeof(Dictionary<,>)
                || declaringType == typeof(IDictionary<,>)
                || declaringType == typeof(IReadOnlyDictionary<,>)))
        {
            return TranslateMemberAccess(instance, key, method.ReturnType);
        }

        // Predicate-less Any - translate to a simple length check.
        return method is { IsGenericMethod: true, Name: nameof(Enumerable.Any) }
            && method.DeclaringType == typeof(Enumerable)
            && arguments is [var source]
            && TranslateArrayLength(source) is SqlExpression arrayLengthTranslation
            ? _sqlExpressionFactory.GreaterThan(arrayLengthTranslation, _sqlExpressionFactory.Constant(0))
            : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance?.TypeMapping is not NpgsqlJsonTypeMapping && instance is not PgJsonTraversalExpression)
        {
            return null;
        }

        if (member is { Name: "Count", DeclaringType.IsGenericType: true }
            && member.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return TranslateArrayLength(instance);
        }

        return TranslateMemberAccess(
            instance,
            _sqlExpressionFactory.Constant(member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? member.Name),
            returnType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? TranslateMemberAccess(SqlExpression instance, SqlExpression member, Type returnType)
    {
        (SqlExpression? root, IEnumerable<SqlExpression> path) = instance switch
        {
            ColumnExpression { TypeMapping: NpgsqlJsonTypeMapping } columnExpression
                => (columnExpression, [member]),
            PgJsonTraversalExpression traversal
                => (traversal.Expression, traversal.Path.Append(member)),
            _ => (null, [])
        };

        if (root is null)
        {
            return null;
        }

        var mapping = _typeMappingSource.FindMapping(returnType.UnwrapNullableType(), _model);
        if (mapping is not (null or NpgsqlArrayTypeMapping or NpgsqlHstoreTypeMapping or NpgsqlJsonTypeMapping))
        {
            var text = _sqlExpressionFactory.JsonTraversal(root, path, returnsText: true, typeof(string), _stringTypeMapping);

            return returnType == typeof(string)
                ? text
                : _sqlExpressionFactory.Convert(text, returnType, mapping);
        }

        var storeType = root.TypeMapping!.StoreType;
        mapping = returnType == typeof(object)
            // Reading JSON as object defaults to string in Npgsql; dictionary values need deserialized JSON instead.
            ? (RelationalTypeMapping)_typeMappingSource.FindMapping(typeof(JsonElement), storeType)!
                .WithComposedConverter(ObjectJsonConverter)
            : _typeMappingSource.FindMapping(returnType, storeType)!;

        return _sqlExpressionFactory.JsonTraversal(root, path, returnsText: false, returnType, mapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? TranslateArrayLength(SqlExpression expression)
    {
        switch (expression)
        {
            case ColumnExpression { TypeMapping: NpgsqlJsonTypeMapping mapping }:
                return _sqlExpressionFactory.Function(
                    mapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                    [expression],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(int));

            case PgJsonTraversalExpression traversal:
                var jsonMapping = (NpgsqlJsonTypeMapping)traversal.Expression.TypeMapping!;
                return _sqlExpressionFactory.Function(
                    jsonMapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                    [traversal],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(int));

            default:
                return null;
        }
    }
}
