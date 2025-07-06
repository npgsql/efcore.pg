using System.Runtime.CompilerServices;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
{
    private readonly Version _postgresVersion;

    private static readonly MethodInfo TsQueryParse =
        typeof(NpgsqlTsQuery).GetRuntimeMethod(nameof(NpgsqlTsQuery.Parse), [typeof(string)])!;

    private static readonly MethodInfo TsVectorParse =
        typeof(NpgsqlTsVector).GetRuntimeMethod(nameof(NpgsqlTsVector.Parse), [typeof(string)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlEvaluatableExpressionFilter(
        EvaluatableExpressionFilterDependencies dependencies,
        RelationalEvaluatableExpressionFilterDependencies relationalDependencies,
        INpgsqlSingletonOptions npgsqlSingletonOptions)
        : base(dependencies, relationalDependencies)
    {
        _postgresVersion = npgsqlSingletonOptions.PostgresVersion;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsEvaluatableExpression(Expression expression, IModel model)
    {
        switch (expression)
        {
            case MethodCallExpression methodCallExpression:
                var declaringType = methodCallExpression.Method.DeclaringType;
                var method = methodCallExpression.Method;

                if (method == TsQueryParse
                    || method == TsVectorParse
                    || declaringType == typeof(NpgsqlDbFunctionsExtensions)
                    || declaringType == typeof(NpgsqlFullTextSearchDbFunctionsExtensions)
                    || declaringType == typeof(NpgsqlFullTextSearchLinqExtensions)
                    || declaringType == typeof(NpgsqlNetworkDbFunctionsExtensions)
                    || declaringType == typeof(NpgsqlJsonDbFunctionsExtensions)
                    || declaringType == typeof(NpgsqlRangeDbFunctionsExtensions)
                    // PG18 introduced uuidv7(), so we prevent local evaluation when targeting PG18 or later.
                    || declaringType == typeof(Guid) && method.Name == nameof(Guid.CreateVersion7) && _postgresVersion.AtLeast(18)
                    // Prevent evaluation of ValueTuple.Create, see NewExpression of ITuple below
                    || declaringType == typeof(ValueTuple) && method.Name == nameof(ValueTuple.Create))
                {
                    return false;
                }

                break;

            case NewExpression newExpression when newExpression.Type.IsAssignableTo(typeof(ITuple)):
                // We translate new ValueTuple<T1, T2...>(x, y...) to a SQL row value expression: (x, y)
                // (see NpgsqlSqlTranslatingExpressionVisitor.VisitNew).
                // We must prevent evaluation when the tuple contains only constants/parameters, since SQL row values cannot be
                // parameterized; we need to render them as "literals" instead:
                // WHERE (x, y) > (3, $1)
                return false;
        }

        return base.IsEvaluatableExpression(expression, model);
    }
}
