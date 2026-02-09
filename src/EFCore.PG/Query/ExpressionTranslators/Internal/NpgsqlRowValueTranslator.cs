using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlRowValueTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    /// <inheritdoc />
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(ValueType))] // For ValueTuple.Create
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Translate ValueTuple.Create
        if (method.DeclaringType == typeof(ValueTuple) && method is { IsStatic: true, Name: nameof(ValueTuple.Create) })
        {
            return new PgRowValueExpression(arguments, method.ReturnType);
        }

        // Translate EF.Functions.GreaterThan and other comparisons
        if (method.DeclaringType != typeof(NpgsqlDbFunctionsExtensions))
        {
            return null;
        }

        var expressionType = method.Name switch
        {
            nameof(NpgsqlDbFunctionsExtensions.GreaterThan) => ExpressionType.GreaterThan,
            nameof(NpgsqlDbFunctionsExtensions.LessThan) => ExpressionType.LessThan,
            nameof(NpgsqlDbFunctionsExtensions.GreaterThanOrEqual) => ExpressionType.GreaterThanOrEqual,
            nameof(NpgsqlDbFunctionsExtensions.LessThanOrEqual) => ExpressionType.LessThanOrEqual,
            _ => (ExpressionType?)null
        };

        if (expressionType is null)
        {
            return null;
        }

        var leftCount = arguments[1] is PgRowValueExpression leftRowValue
            ? leftRowValue.Values.Count
            : arguments[1] is SqlConstantExpression { Value : ITuple leftTuple }
                ? (int?)leftTuple.Length
                : null;

        var rightCount = arguments[2] is PgRowValueExpression rightRowValue
            ? rightRowValue.Values.Count
            : arguments[2] is SqlConstantExpression { Value : ITuple rightTuple }
                ? (int?)rightTuple.Length
                : null;

        if (leftCount is null || rightCount is null)
        {
            return null;
        }

        if (leftCount != rightCount)
        {
            throw new ArgumentException(NpgsqlStrings.RowValueComparisonRequiresTuplesOfSameLength);
        }

        return sqlExpressionFactory.MakeBinary(expressionType.Value, arguments[1], arguments[2], typeMapping: null);
    }
}
