using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlStatisticsAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _doubleTypeMapping, _longTypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlStatisticsAggregateMethodTranslator(
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _doubleTypeMapping = typeMappingSource.FindMapping(typeof(double))!;
        _longTypeMapping = typeMappingSource.FindMapping(typeof(long))!;
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
        // Docs: https://www.postgresql.org/docs/current/functions-aggregate.html#FUNCTIONS-AGGREGATE-STATISTICS-TABLE

        if (method.DeclaringType != typeof(NpgsqlAggregateDbFunctionsExtensions)
            || source.Selector is not SqlExpression sqlExpression)
        {
            return null;
        }

        // These four functions are simple and take a single enumerable argument
        var functionName = method.Name switch
        {
            nameof(NpgsqlAggregateDbFunctionsExtensions.StandardDeviationSample) => "stddev_samp",
            nameof(NpgsqlAggregateDbFunctionsExtensions.StandardDeviationPopulation) => "stddev_pop",
            nameof(NpgsqlAggregateDbFunctionsExtensions.VarianceSample) => "var_samp",
            nameof(NpgsqlAggregateDbFunctionsExtensions.VariancePopulation) => "var_pop",
            _ => null
        };

        if (functionName is not null)
        {
            return _sqlExpressionFactory.AggregateFunction(
                functionName,
                new[] { sqlExpression },
                source,
                nullable: true,
                argumentsPropagateNullability: FalseArrays[1],
                typeof(double),
                _doubleTypeMapping);
        }

        functionName = method.Name switch
        {
            nameof(NpgsqlAggregateDbFunctionsExtensions.Correlation) => "corr",
            nameof(NpgsqlAggregateDbFunctionsExtensions.CovariancePopulation) => "covar_pop",
            nameof(NpgsqlAggregateDbFunctionsExtensions.CovarianceSample) => "covar_samp",
            nameof(NpgsqlAggregateDbFunctionsExtensions.RegrAverageX) => "regr_avgx",
            nameof(NpgsqlAggregateDbFunctionsExtensions.RegrAverageY) => "regr_avgy",
            nameof(NpgsqlAggregateDbFunctionsExtensions.RegrCount) => "regr_count",
            nameof(NpgsqlAggregateDbFunctionsExtensions.RegrIntercept) => "regr_intercept",
            nameof(NpgsqlAggregateDbFunctionsExtensions.RegrR2) => "regr_r2",
            nameof(NpgsqlAggregateDbFunctionsExtensions.RegrSlope) => "regr_slope",
            nameof(NpgsqlAggregateDbFunctionsExtensions.RegrSXX) => "regr_sxx",
            nameof(NpgsqlAggregateDbFunctionsExtensions.RegrSXY) => "regr_sxy",
            _ => null
        };

        if (functionName is not null)
        {
            // These methods accept two enumerable (column) arguments; this is represented in LINQ as a projection from the grouping
            // to a tuple of the two columns. Since we generally translate tuples to PostgresRowValueExpression, we take it apart here.
            if (source.Selector is not PostgresRowValueExpression rowValueExpression)
            {
                return null;
            }

            var (y, x) = (rowValueExpression.Values[0], rowValueExpression.Values[1]);

            return method.Name == nameof(NpgsqlAggregateDbFunctionsExtensions.RegrCount)
                ? _sqlExpressionFactory.AggregateFunction(
                    functionName,
                    new[] { y, x },
                    source,
                    nullable: true,
                    argumentsPropagateNullability: FalseArrays[2],
                    typeof(long),
                    _longTypeMapping)
                : _sqlExpressionFactory.AggregateFunction(
                    functionName,
                    new[] { y, x },
                    source,
                    nullable: true,
                    argumentsPropagateNullability: FalseArrays[2],
                    typeof(double),
                    _doubleTypeMapping);
        }

        return null;
    }
}
