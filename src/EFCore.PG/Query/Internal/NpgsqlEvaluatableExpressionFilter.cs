using System.Runtime.CompilerServices;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

public class NpgsqlEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
{
    private static readonly MethodInfo TsQueryParse =
        typeof(NpgsqlTsQuery).GetRuntimeMethod(nameof(NpgsqlTsQuery.Parse), new[] { typeof(string) })!;

    private static readonly MethodInfo TsVectorParse =
        typeof(NpgsqlTsVector).GetRuntimeMethod(nameof(NpgsqlTsVector.Parse), new[] { typeof(string) })!;

    public NpgsqlEvaluatableExpressionFilter(
        EvaluatableExpressionFilterDependencies dependencies,
        RelationalEvaluatableExpressionFilterDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    public override bool IsEvaluatableExpression(Expression expression, IModel model)
    {
        switch (expression)
        {
            case MethodCallExpression methodCallExpression:
                var declaringType = methodCallExpression.Method.DeclaringType;

                if (methodCallExpression.Method == TsQueryParse
                    || methodCallExpression.Method == TsVectorParse
                    || declaringType == typeof(NpgsqlDbFunctionsExtensions)
                    || declaringType == typeof(NpgsqlFullTextSearchDbFunctionsExtensions)
                    || declaringType == typeof(NpgsqlFullTextSearchLinqExtensions)
                    || declaringType == typeof(NpgsqlNetworkDbFunctionsExtensions)
                    || declaringType == typeof(NpgsqlJsonDbFunctionsExtensions)
                    || declaringType == typeof(NpgsqlRangeDbFunctionsExtensions))
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