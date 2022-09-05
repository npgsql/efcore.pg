using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlNodaTimeAggregateMethodCallTranslatorPlugin : IAggregateMethodCallTranslatorPlugin
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlNodaTimeAggregateMethodCallTranslatorPlugin(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        if (sqlExpressionFactory is not NpgsqlSqlExpressionFactory npgsqlSqlExpressionFactory)
        {
            throw new ArgumentException($"Must be an {nameof(NpgsqlSqlExpressionFactory)}", nameof(sqlExpressionFactory));
        }

        Translators = new IAggregateMethodCallTranslator[]
        {
            new NpgsqlNodaTimeAggregateMethodTranslator(npgsqlSqlExpressionFactory, typeMappingSource)
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IAggregateMethodCallTranslator> Translators { get; }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlNodaTimeAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private static readonly bool[][] FalseArrays = { Array.Empty<bool>(), new[] { false } };

    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlNodaTimeAggregateMethodTranslator(
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (source.Selector is not SqlExpression sqlExpression || method.DeclaringType != typeof(NpgsqlNodaTimeDbFunctionsExtensions))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(NpgsqlNodaTimeDbFunctionsExtensions.Sum) => _sqlExpressionFactory.AggregateFunction(
                "sum", new[] { sqlExpression }, source, nullable: true, argumentsPropagateNullability: FalseArrays[1],
                returnType: sqlExpression.Type, sqlExpression.TypeMapping),

            nameof(NpgsqlNodaTimeDbFunctionsExtensions.Average) => _sqlExpressionFactory.AggregateFunction(
                "avg", new[] { sqlExpression }, source, nullable: true, argumentsPropagateNullability: FalseArrays[1],
                returnType: sqlExpression.Type, sqlExpression.TypeMapping),

            nameof(NpgsqlNodaTimeDbFunctionsExtensions.RangeAgg) => _sqlExpressionFactory.AggregateFunction(
                "range_agg", new[] { sqlExpression }, source, nullable: true, argumentsPropagateNullability: FalseArrays[1],
                returnType: method.ReturnType, _typeMappingSource.FindMapping(method.ReturnType)),

            nameof(NpgsqlNodaTimeDbFunctionsExtensions.RangeIntersectAgg) => _sqlExpressionFactory.AggregateFunction(
                "range_intersect_agg", new[] { sqlExpression }, source, nullable: true, argumentsPropagateNullability: FalseArrays[1],
                returnType: sqlExpression.Type, sqlExpression.TypeMapping),

            _ => null
        };
    }
}
