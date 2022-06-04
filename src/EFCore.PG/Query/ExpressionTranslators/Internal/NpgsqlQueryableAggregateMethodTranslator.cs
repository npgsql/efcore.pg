using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

public class NpgsqlQueryableAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    public NpgsqlQueryableAggregateMethodTranslator(
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

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
                                "AVG",
                                new[] { averageSqlExpression },
                                nullable: true,
                                argumentsPropagateNullability: FalseArrays[1],
                                source,
                                typeof(double)),
                            averageSqlExpression.Type,
                            averageSqlExpression.TypeMapping)
                        : _sqlExpressionFactory.AggregateFunction(
                            "AVG",
                            new[] { averageSqlExpression },
                            nullable: true,
                            argumentsPropagateNullability: FalseArrays[1],
                            source,
                            averageSqlExpression.Type,
                            averageSqlExpression.TypeMapping);

                // PostgreSQL COUNT() always returns bigint, so we need to downcast to int
                case nameof(Queryable.Count)
                when methodInfo == QueryableMethods.CountWithoutPredicate
                    || methodInfo == QueryableMethods.CountWithPredicate:
                    var countSqlExpression = (source.Selector as SqlExpression) ?? _sqlExpressionFactory.Fragment("*");
                    return _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.ApplyDefaultTypeMapping(
                            _sqlExpressionFactory.AggregateFunction(
                                "COUNT",
                                new[] { countSqlExpression },
                                nullable: false,
                                argumentsPropagateNullability: FalseArrays[1],
                                source,
                                typeof(long))),
                        typeof(int), _typeMappingSource.FindMapping(typeof(int)));

                case nameof(Queryable.LongCount)
                when methodInfo == QueryableMethods.LongCountWithoutPredicate
                    || methodInfo == QueryableMethods.LongCountWithPredicate:
                    var longCountSqlExpression = (source.Selector as SqlExpression) ?? _sqlExpressionFactory.Fragment("*");
                    return _sqlExpressionFactory.ApplyDefaultTypeMapping(
                        _sqlExpressionFactory.AggregateFunction(
                            "COUNT",
                            new[] { longCountSqlExpression },
                            nullable: false,
                            argumentsPropagateNullability: FalseArrays[1],
                            source,
                            typeof(long)));

                case nameof(Queryable.Max)
                when (methodInfo == QueryableMethods.MaxWithoutSelector
                    || methodInfo == QueryableMethods.MaxWithSelector)
                    && source.Selector is SqlExpression maxSqlExpression:
                    return _sqlExpressionFactory.AggregateFunction(
                            "MAX",
                            new[] { maxSqlExpression },
                            nullable: true,
                            argumentsPropagateNullability: FalseArrays[1],
                            source,
                            maxSqlExpression.Type,
                            maxSqlExpression.TypeMapping);

                case nameof(Queryable.Min)
                when (methodInfo == QueryableMethods.MinWithoutSelector
                    || methodInfo == QueryableMethods.MinWithSelector)
                    && source.Selector is SqlExpression minSqlExpression:
                    return _sqlExpressionFactory.AggregateFunction(
                            "MIN",
                            new[] { minSqlExpression },
                            nullable: true,
                            argumentsPropagateNullability: FalseArrays[1],
                            source,
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
                                "SUM",
                                new[] { sumSqlExpression },
                                nullable: true,
                                argumentsPropagateNullability: FalseArrays[1],
                                source,
                                typeof(long)),
                            sumInputType,
                            sumSqlExpression.TypeMapping);
                    }

                    if (sumInputType == typeof(long))
                    {
                        return _sqlExpressionFactory.Convert(
                            _sqlExpressionFactory.AggregateFunction(
                                "SUM",
                                new[] { sumSqlExpression },
                                nullable: true,
                                argumentsPropagateNullability: FalseArrays[1],
                                source,
                                typeof(decimal)),
                            sumInputType,
                            sumSqlExpression.TypeMapping);
                    }

                    return _sqlExpressionFactory.AggregateFunction(
                        "SUM",
                        new[] { sumSqlExpression },
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[1],
                        source,
                        sumInputType,
                        sumSqlExpression.TypeMapping);
            }
        }

        return null;
    }
}
