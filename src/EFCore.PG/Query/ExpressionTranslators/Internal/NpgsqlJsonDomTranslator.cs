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
public class NpgsqlJsonDomTranslator : IMemberTranslator, IMethodCallTranslator
{
    private static readonly MemberInfo RootElement = typeof(JsonDocument).GetProperty(nameof(JsonDocument.RootElement))!;
    private static readonly MethodInfo GetProperty = typeof(JsonElement).GetRuntimeMethod(nameof(JsonElement.GetProperty), new[] { typeof(string) })!;
    private static readonly MethodInfo GetArrayLength = typeof(JsonElement).GetRuntimeMethod(nameof(JsonElement.GetArrayLength), Type.EmptyTypes)!;

    private static readonly MethodInfo ArrayIndexer = typeof(JsonElement).GetProperties()
        .Single(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(int))
        .GetMethod!;

    private static readonly string[] GetMethods =
    {
        nameof(JsonElement.GetBoolean),
        nameof(JsonElement.GetDateTime),
        nameof(JsonElement.GetDateTimeOffset),
        nameof(JsonElement.GetDecimal),
        nameof(JsonElement.GetDouble),
        nameof(JsonElement.GetGuid),
        nameof(JsonElement.GetInt16),
        nameof(JsonElement.GetInt32),
        nameof(JsonElement.GetInt64),
        nameof(JsonElement.GetSingle),
        nameof(JsonElement.GetString)
    };

    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _stringTypeMapping;
    private readonly IModel _model;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlJsonDomTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
        _model = model;
        _stringTypeMapping = typeMappingSource.FindMapping(typeof(string), model)!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (member.DeclaringType != typeof(JsonDocument))
        {
            return null;
        }

        if (member == RootElement &&
            instance is ColumnExpression column &&
            column.TypeMapping is NpgsqlJsonTypeMapping)
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
        if (method.DeclaringType != typeof(JsonElement) ||
            instance?.TypeMapping is not NpgsqlJsonTypeMapping mapping)
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

        if (method == GetProperty || method == ArrayIndexer)
        {
            return instance is PostgresJsonTraversalExpression prevPathTraversal
                ? prevPathTraversal.Append(_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]))
                : null;
        }

        if (GetMethods.Contains(method.Name) &&
            arguments.Count == 0 &&
            instance is PostgresJsonTraversalExpression traversal)
        {
            var traversalToText = new PostgresJsonTraversalExpression(
                traversal.Expression,
                traversal.Path,
                returnsText: true,
                typeof(string),
                _stringTypeMapping);

            return method.Name == nameof(JsonElement.GetString)
                ? traversalToText
                : ConvertFromText(traversalToText, method.ReturnType);
        }

        if (method == GetArrayLength)
        {
            return _sqlExpressionFactory.Function(
                mapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                new[] { instance },
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

    // The PostgreSQL traversal operator always returns text, so we need to convert to int, bool, etc.
    private SqlExpression ConvertFromText(SqlExpression expression, Type returnType)
    {
        switch (Type.GetTypeCode(returnType))
        {
            case TypeCode.Boolean:
            case TypeCode.Byte:
            case TypeCode.DateTime:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.SByte:
            case TypeCode.Single:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return _sqlExpressionFactory.Convert(expression, returnType, _typeMappingSource.FindMapping(returnType, _model));
            default:
                return returnType == typeof(Guid)
                    ? _sqlExpressionFactory.Convert(expression, returnType, _typeMappingSource.FindMapping(returnType, _model))
                    : expression;
        }
    }
}
