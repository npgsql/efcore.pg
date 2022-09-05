using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlQueryableAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlQueryableAggregateMethodTranslator(
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
        if (method.DeclaringType == typeof(Queryable))
        {
            var methodInfo = method.IsGenericMethod
                ? method.GetGenericMethodDefinition()
                : method;
            switch (methodInfo.Name)
            {
                case nameof(Queryable.Average)
                when (QueryableMethods.IsAverageWithoutSelector(methodInfo)
                    || QueryableMethods.IsAverageWithSelector(methodInfo))
                    && source.Selector is SqlExpression averageSqlExpression:
                    var averageInputType = averageSqlExpression.Type;
                    if (averageInputType == typeof(int)
                        || averageInputType == typeof(long))
                    {
                        averageSqlExpression = _sqlExpressionFactory.ApplyDefaultTypeMapping(
                            _sqlExpressionFactory.Convert(averageSqlExpression, typeof(double)));
                    }

                    return averageInputType == typeof(float)
                        ? _sqlExpressionFactory.Convert(
                            _sqlExpressionFactory.AggregateFunction(
                                "avg",
                                new[] { averageSqlExpression },
                                source,
                                nullable: true,
                                 argumentsPropagateNullability: FalseArrays[1],
                                returnType: typeof(double)),
                            averageSqlExpression.Type,
                            averageSqlExpression.TypeMapping)
                        : _sqlExpressionFactory.AggregateFunction(
                            "avg",
                            new[] { averageSqlExpression },
                            source,
                            nullable: true,
                            argumentsPropagateNullability: FalseArrays[1],
                            averageSqlExpression.Type,
                            averageSqlExpression.TypeMapping);

                // PostgreSQL COUNT() always returns bigint, so we need to downcast to int
                case nameof(Queryable.Count)
                when methodInfo == QueryableMethods.CountWithoutPredicate
                    || methodInfo == QueryableMethods.CountWithPredicate:
                    var countSqlExpression = (source.Selector as SqlExpression) ?? _sqlExpressionFactory.Fragment("*");
                    return _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.AggregateFunction(
                            "count",
                            new[] { countSqlExpression },
                            source,
                            nullable: false,
                            argumentsPropagateNullability: FalseArrays[1],
                            typeof(long)),
                        typeof(int),
                        _typeMappingSource.FindMapping(typeof(int)));

                case nameof(Queryable.LongCount)
                when methodInfo == QueryableMethods.LongCountWithoutPredicate
                    || methodInfo == QueryableMethods.LongCountWithPredicate:
                    var longCountSqlExpression = (source.Selector as SqlExpression) ?? _sqlExpressionFactory.Fragment("*");
                    return _sqlExpressionFactory.AggregateFunction(
                            "count",
                            new[] { longCountSqlExpression },
                            source,
                            nullable: false,
                            argumentsPropagateNullability: FalseArrays[1],
                            typeof(long));

                case nameof(Queryable.Max)
                when (methodInfo == QueryableMethods.MaxWithoutSelector
                    || methodInfo == QueryableMethods.MaxWithSelector)
                    && source.Selector is SqlExpression maxSqlExpression:
                    return _sqlExpressionFactory.AggregateFunction(
                            "max",
                            new[] { maxSqlExpression },
                            source,
                            nullable: true,
                            argumentsPropagateNullability: FalseArrays[1],
                            maxSqlExpression.Type,
                            maxSqlExpression.TypeMapping);

                case nameof(Queryable.Min)
                when (methodInfo == QueryableMethods.MinWithoutSelector
                    || methodInfo == QueryableMethods.MinWithSelector)
                    && source.Selector is SqlExpression minSqlExpression:
                    return _sqlExpressionFactory.AggregateFunction(
                            "min",
                            new[] { minSqlExpression },
                            source,
                            nullable: true,
                            argumentsPropagateNullability: FalseArrays[1],
                            minSqlExpression.Type,
                            minSqlExpression.TypeMapping);

                // In PostgreSQL SUM() doesn't return the same type as its argument for smallint, int and bigint.
                // Cast to get the same type.
                // http://www.postgresql.org/docs/current/static/functions-aggregate.html
                case nameof(Queryable.Sum)
                when (QueryableMethods.IsSumWithoutSelector(methodInfo)
                    || QueryableMethods.IsSumWithSelector(methodInfo))
                    && source.Selector is SqlExpression sumSqlExpression:
                    var sumInputType = sumSqlExpression.Type;

                    // Note that there is no Sum over short in LINQ
                    if (sumInputType == typeof(int))
                    {
                        return _sqlExpressionFactory.Convert(
                            _sqlExpressionFactory.AggregateFunction(
                                "sum",
                                new[] { sumSqlExpression },
                                source,
                                nullable: true,
                                argumentsPropagateNullability: FalseArrays[1],
                                typeof(long)),
                            sumInputType,
                            sumSqlExpression.TypeMapping);
                    }

                    if (sumInputType == typeof(long))
                    {
                        return _sqlExpressionFactory.Convert(
                            _sqlExpressionFactory.AggregateFunction(
                                "sum",
                                new[] { sumSqlExpression },
                                source,
                                nullable: true,
                                argumentsPropagateNullability: FalseArrays[1],
                                typeof(decimal)),
                            sumInputType,
                            sumSqlExpression.TypeMapping);
                    }

                    return _sqlExpressionFactory.AggregateFunction(
                        "sum",
                        new[] { sumSqlExpression },
                        source,
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[1],
                        sumInputType,
                        sumSqlExpression.TypeMapping);
            }
        }

        return null;
    }
}
