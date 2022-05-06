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
        }

        return base.IsEvaluatableExpression(expression, model);
    }
}