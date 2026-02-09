using System.Text.Json;
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
public class NpgsqlJsonDomTranslator(
    IRelationalTypeMappingSource typeMappingSource,
    NpgsqlSqlExpressionFactory sqlExpressionFactory,
    IModel model)
    : IMemberTranslator, IMethodCallTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource = typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory = sqlExpressionFactory;
    private readonly RelationalTypeMapping _stringTypeMapping = typeMappingSource.FindMapping(typeof(string), model)!;
    private readonly IModel _model = model;

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
        if (member.DeclaringType != typeof(JsonDocument))
        {
            return null;
        }

        if (member.Name == nameof(JsonDocument.RootElement)
            && instance is ColumnExpression { TypeMapping: NpgsqlJsonTypeMapping } column)
        {
            // Simply get rid of the RootElement member access
            return column;
        }

        return null;
    }

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
        if (method.DeclaringType != typeof(JsonElement) || instance?.TypeMapping is not NpgsqlJsonTypeMapping mapping)
        {
            return null;
        }

        // The root of the JSON expression is a ColumnExpression. We wrap that with an empty traversal
        // expression (col #>> '{}'); subsequent traversals will gradually append the path into that.
        // Note that it's possible to call methods such as GetString() directly on the root, and the
        // empty traversal is necessary to properly convert it to a text.
        instance = instance is ColumnExpression columnExpression
            ? _sqlExpressionFactory.JsonTraversal(
                columnExpression, returnsText: false, typeof(string), mapping)
            : instance;

        if (method.Name is nameof(JsonElement.GetProperty) or "get_Item" && arguments is [_])
        {
            return instance is PgJsonTraversalExpression prevPathTraversal
                ? prevPathTraversal.Append(_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]))
                : null;
        }

        if (method.Name is nameof(JsonElement.GetBoolean)
                or nameof(JsonElement.GetDateTime)
                or nameof(JsonElement.GetDateTimeOffset)
                or nameof(JsonElement.GetDecimal)
                or nameof(JsonElement.GetDouble)
                or nameof(JsonElement.GetGuid)
                or nameof(JsonElement.GetInt16)
                or nameof(JsonElement.GetInt32)
                or nameof(JsonElement.GetInt64)
                or nameof(JsonElement.GetSingle)
                or nameof(JsonElement.GetString)
            && arguments.Count == 0 && instance is PgJsonTraversalExpression traversal)
        {
            var traversalToText = new PgJsonTraversalExpression(
                traversal.Expression,
                traversal.Path,
                returnsText: true,
                typeof(string),
                _stringTypeMapping);

            // The PostgreSQL traversal operator always returns text - for these scalar-returning methods, apply a conversion from string.
            return method.Name == nameof(JsonElement.GetString)
                ? traversalToText
                : _sqlExpressionFactory.Convert(
                    traversalToText, method.ReturnType, _typeMappingSource.FindMapping(method.ReturnType, _model));
        }

        if (method.Name == nameof(JsonElement.GetArrayLength) && arguments is [])
        {
            return _sqlExpressionFactory.Function(
                mapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                [instance],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(int));
        }

        if (method.Name.StartsWith("TryGet", StringComparison.Ordinal) && arguments.Count == 0)
        {
            throw new InvalidOperationException($"The TryGet* methods on {nameof(JsonElement)} aren't translated yet, use Get* instead.'");
        }

        return null;
    }
}
