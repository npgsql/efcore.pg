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
public class NpgsqlJsonPocoTranslator : IMemberTranslator
{
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
    public NpgsqlJsonPocoTranslator(
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
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance?.TypeMapping is not NpgsqlJsonTypeMapping && instance is not PostgresJsonTraversalExpression)
        {
            return null;
        }

        if (member.Name == nameof(List<object>.Count)
            && member.DeclaringType?.IsGenericType == true
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
    public virtual SqlExpression? TranslateMemberAccess(
        SqlExpression instance, SqlExpression member, Type returnType)
    {
        // The first time we see a JSON traversal it's on a column - create a JsonTraversalExpression.
        // Traversals on top of that get appended into the same expression.

        if (instance is ColumnExpression { TypeMapping: NpgsqlJsonTypeMapping } columnExpression)
        {
            return ConvertFromText(
                _sqlExpressionFactory.JsonTraversal(
                    columnExpression,
                    new[] { member },
                    returnsText: true,
                    typeof(string),
                    _stringTypeMapping),
                returnType);
        }

        if (instance is PostgresJsonTraversalExpression prevPathTraversal)
        {
            return ConvertFromText(
                prevPathTraversal.Append(_sqlExpressionFactory.ApplyDefaultTypeMapping(member)),
                returnType);
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? TranslateArrayLength(SqlExpression expression)
    {
        if (expression is ColumnExpression { TypeMapping: NpgsqlJsonTypeMapping mapping })
        {
            return _sqlExpressionFactory.Function(
                mapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                new[] { expression },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[2],
                typeof(int));
        }

        if (expression is PostgresJsonTraversalExpression traversal)
        {
            // The traversal expression has ReturnsText=true (e.g. ->> not ->), so we recreate it to return
            // the JSON object instead.
            var lastPathComponent = traversal.Path.Last();
            var newTraversal = new PostgresJsonTraversalExpression(
                traversal.Expression, traversal.Path,
                returnsText: false,
                lastPathComponent.Type,
                _typeMappingSource.FindMapping(lastPathComponent.Type, _model));

            var jsonMapping = (NpgsqlJsonTypeMapping)traversal.Expression.TypeMapping!;
            return _sqlExpressionFactory.Function(
                jsonMapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                new[] { newTraversal },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[2],
                typeof(int));
        }

        return null;
    }

    // The PostgreSQL traversal operator always returns text, so we need to convert to int, bool, etc.
    private SqlExpression ConvertFromText(SqlExpression expression, Type returnType)
    {
        var unwrappedReturnType = returnType.UnwrapNullableType();

        switch (Type.GetTypeCode(unwrappedReturnType))
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
        }

        if (unwrappedReturnType == typeof(Guid)
            || unwrappedReturnType == typeof(DateTimeOffset)
            || unwrappedReturnType == typeof(DateOnly)
            || unwrappedReturnType == typeof(TimeOnly))
        {
            return _sqlExpressionFactory.Convert(expression, returnType, _typeMappingSource.FindMapping(returnType, _model));
        }

        return expression;
    }
}
