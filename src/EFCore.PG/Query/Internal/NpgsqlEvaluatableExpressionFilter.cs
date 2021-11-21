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

            case MemberExpression memberExpression:
                // We support translating certain NodaTime patterns which accept a time zone as a parameter,
                // e.g. Instant.InZone(timezone), as long as the timezone is expressed as an access on DateTimeZoneProviders.Tzdb.
                // Prevent this from being evaluated locally and so parameterized, so we can access the access tree on
                // DateTimeZoneProviders and extract the constant (see NpgsqlNodaTimeMethodCallTranslatorPlugin)
                if (memberExpression.Member.DeclaringType?.FullName == "NodaTime.DateTimeZoneProviders")
                {
                    return false;
                }

                break;
        }

        return base.IsEvaluatableExpression(expression, model);
    }
}