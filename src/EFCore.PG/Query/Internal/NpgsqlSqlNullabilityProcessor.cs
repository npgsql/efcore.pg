using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <inheritdoc />
public class NpgsqlSqlNullabilityProcessor : SqlNullabilityProcessor
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    /// Creates a new instance of the <see cref="NpgsqlSqlNullabilityProcessor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="useRelationalNulls">A bool value indicating whether relational null semantics are in use.</param>
    public NpgsqlSqlNullabilityProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        bool useRelationalNulls)
        : base(dependencies, useRelationalNulls)
        => _sqlExpressionFactory = dependencies.SqlExpressionFactory;

    /// <inheritdoc />
    protected override SqlExpression VisitCustomSqlExpression(
        SqlExpression sqlExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
        => sqlExpression switch
        {
            PostgresAnyExpression postgresAnyExpression
                => VisitAny(postgresAnyExpression, allowOptimizedExpansion, out nullable),
            PostgresAllExpression postgresAllExpression
                => VisitAll(postgresAllExpression, allowOptimizedExpansion, out nullable),
            PostgresArrayIndexExpression arrayIndexExpression
                => VisitArrayIndex(arrayIndexExpression, allowOptimizedExpansion, out nullable),
            PostgresBinaryExpression binaryExpression
                => VisitBinary(binaryExpression, allowOptimizedExpansion, out nullable),
            PostgresILikeExpression ilikeExpression
                => VisitILike(ilikeExpression, allowOptimizedExpansion, out nullable),
            PostgresJsonTraversalExpression postgresJsonTraversalExpression
                => VisitJsonTraversal(postgresJsonTraversalExpression, allowOptimizedExpansion, out nullable),
            PostgresNewArrayExpression newArrayExpression
                => VisitNewArray(newArrayExpression, allowOptimizedExpansion, out nullable),
            PostgresRegexMatchExpression regexMatchExpression
                => VisitRegexMatch(regexMatchExpression, allowOptimizedExpansion, out nullable),
            PostgresRowValueExpression postgresRowValueExpression
                => VisitRowValueExpression(postgresRowValueExpression, allowOptimizedExpansion, out nullable),
            PostgresUnknownBinaryExpression postgresUnknownBinaryExpression
                => VisitUnknownBinary(postgresUnknownBinaryExpression, allowOptimizedExpansion, out nullable),

            // PostgresFunctionExpression is visited via the SqlFunctionExpression override below

            _ => base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable)
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitAny(PostgresAnyExpression anyExpression, bool allowOptimizedExpansion, out bool nullable)
    {
        Check.NotNull(anyExpression, nameof(anyExpression));

        var item = Visit(anyExpression.Item, out var itemNullable);
        var array = Visit(anyExpression.Array, out var entireArrayNullable);

        SqlExpression updated = anyExpression.Update(item, array);

        if (UseRelationalNulls)
        {
            nullable = false;
            return updated;
        }

        // We only do null compensation for item = ANY(array), not for any other operator.
        // Note that LIKE (without ANY) doesn't get null-compensated either, so we're consistent with that.
        if (anyExpression.OperatorType != PostgresAnyOperatorType.Equal)
        {
            // If the array is a constant and it contains a null, or if the array isn't a constant,
            // we assume the entire expression can be null.
            nullable = itemNullable || entireArrayNullable || MayContainNulls(anyExpression.Array);
            return updated;
        }

        // For PostgresAnyOperatorType.Equal, we perform null compensation.
        // When the array is a parameter, we don't look at the values (in contrast to relational's
        // VisitIn which expands parameter arrays into constants).

        // Note that ANY does not use GIN/GIST indexes on the array, but it does use indexes on the item.
        // All of the below expansions allow index use on the item.

        // non_nullable = ANY(array) -> non_nullable = ANY(array) (optimized)
        // non_nullable = ANY(array) -> non_nullable = ANY(array) AND (non_nullable = ANY(array) IS NOT NULL) (full)
        // nullable = ANY(array) -> nullable = ANY(array) OR (nullable IS NULL AND array_position(array, NULL) IS NOT NULL) (optimized)
        // nullable = ANY(array) -> (nullable = ANY(array) AND (nullable = ANY(array) IS NOT NULL)) OR (nullable IS NULL AND array_position(array, NULL) IS NOT NULL) (full)

        nullable = false;

        // ANY returns NULL if an element isn't found in the array but the array contains NULL, instead of false.
        // So for non-optimized, we compensate by adding a check that NULL isn't returned.
        if (!allowOptimizedExpansion)
        {
            updated = _sqlExpressionFactory.And(updated, _sqlExpressionFactory.IsNotNull(updated));
        }

        if (!itemNullable)
        {
            return updated;
        }

        // If the item is nullable, add an OR to check for the item being null and the array containing null.
        // The latter check is done with array_position, which returns null when a value was not found, and
        // a position if the item (including null!) was found (IS NOT DISTINCT FROM semantics)
        return _sqlExpressionFactory.OrElse(
            updated,
            _sqlExpressionFactory.AndAlso(
                _sqlExpressionFactory.IsNull(item),
                _sqlExpressionFactory.IsNotNull(
                    _sqlExpressionFactory.Function(
                        "array_position",
                        new[] { array, _sqlExpressionFactory.Constant(null, item.TypeMapping) },
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[2],
                        typeof(int)))));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitAll(PostgresAllExpression allExpression, bool allowOptimizedExpansion, out bool nullable)
    {
        Check.NotNull(allExpression, nameof(allExpression));

        var item = Visit(allExpression.Item, out var itemNullable);
        var array = Visit(allExpression.Array, out var entireArrayNullable);

        SqlExpression updated = allExpression.Update(item, array);

        if (UseRelationalNulls)
        {
            nullable = false;
            return updated;
        }

        // If the array is a constant and it contains a null, or if the array isn't a constant,
        // we assume the entire expression can be null.
        nullable = itemNullable || entireArrayNullable || MayContainNulls(allExpression.Array);
        return updated;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitArrayIndex(
        PostgresArrayIndexExpression arrayIndexExpression, bool allowOptimizedExpansion, out bool nullable)
    {
        Check.NotNull(arrayIndexExpression, nameof(arrayIndexExpression));

        var array = Visit(arrayIndexExpression.Array, allowOptimizedExpansion, out var arrayNullable);
        var index = Visit(arrayIndexExpression.Index, allowOptimizedExpansion, out var indexNullable);

        nullable = arrayNullable || indexNullable || ((NpgsqlArrayTypeMapping)arrayIndexExpression.Array.TypeMapping!).IsElementNullable;

        return arrayIndexExpression.Update(array, index);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitBinary(PostgresBinaryExpression binaryExpression, bool allowOptimizedExpansion, out bool nullable)
    {
        Check.NotNull(binaryExpression, nameof(binaryExpression));

        // For arrays Contains (arr1 @> arr2), we do not implement null semantics because doing so would prevent
        // index use. So '{1, 2, NULL}' @> '{NULL}' returns false. This is notably the translation for .NET
        // Contains over column arrays.

        var left = Visit(binaryExpression.Left, allowOptimizedExpansion, out var leftNullable);
        var right = Visit(binaryExpression.Right, allowOptimizedExpansion, out var rightNullable);

        nullable = binaryExpression.OperatorType switch
        {
            // The following LTree search methods return null for "not found"
            PostgresExpressionType.LTreeFirstAncestor   => true,
            PostgresExpressionType.LTreeFirstDescendent => true,
            PostgresExpressionType.LTreeFirstMatches    => true,

            _                                           => leftNullable || rightNullable
        };

        return binaryExpression.Update(left, right);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override SqlExpression VisitSqlFunction(
        SqlFunctionExpression sqlFunctionExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        // PostgresFunctionExpression extends SqlFunctionExpression, and adds aggregate predicate and ordering expressions to that.
        // First call the base VisitSqlFunction to visit the arguments
        var visitedBase = base.VisitSqlFunction(sqlFunctionExpression, allowOptimizedExpansion, out nullable);

        // base.VisitSqlFunction has some special logic for SUM which wraps it in a COALESCE
        // (see https://github.com/dotnet/efcore/issues/28158), so we need some special handling to properly visit the
        // PostgresFunctionExpression it wraps.
        if (sqlFunctionExpression.IsBuiltIn
            && string.Equals(sqlFunctionExpression.Name, "SUM", StringComparison.OrdinalIgnoreCase)
            && visitedBase is SqlFunctionExpression { Name: "COALESCE", Arguments: { } } coalesceExpression
            && coalesceExpression.Arguments[0] is PostgresFunctionExpression wrappedFunctionExpression)
        {
            // The base logic assumes sum is operating over numbers, which breaks sum over PG interval.
            // Detect that case and remove the coalesce entirely (note that we don't need coalescing since sum function is in
            // EF.Functions.Sum, and returns nullable. This is a temporary hack until #38158 is fixed.
            if (sqlFunctionExpression.Type == typeof(TimeSpan)
                || sqlFunctionExpression.Type.FullName is "NodaTime.Period" or "NodaTime.Duration")
            {
                return coalesceExpression.Arguments[0];
            }

            var visitedArguments = coalesceExpression.Arguments!.ToArray();
            visitedArguments[0] = VisitPostgresFunctionComponents(wrappedFunctionExpression);

            return coalesceExpression.Update(coalesceExpression.Instance, visitedArguments);
        }

        return visitedBase is PostgresFunctionExpression pgFunctionExpression
            ? VisitPostgresFunctionComponents(pgFunctionExpression)
            : visitedBase;

        PostgresFunctionExpression VisitPostgresFunctionComponents(PostgresFunctionExpression pgFunctionExpression)
        {
            var aggregateChanged = false;

            var visitedAggregatePredicate = Visit(pgFunctionExpression.AggregatePredicate, allowOptimizedExpansion: true, out _);
            aggregateChanged |= visitedAggregatePredicate != pgFunctionExpression.AggregatePredicate;

            OrderingExpression[]? visitedOrderings = null;
            for (var i = 0; i < pgFunctionExpression.AggregateOrderings.Count; i++)
            {
                var ordering = pgFunctionExpression.AggregateOrderings[i];
                var visitedOrdering = ordering.Update(Visit(ordering.Expression, out _));
                if (visitedOrdering != ordering && visitedOrderings is null)
                {
                    visitedOrderings = new OrderingExpression[pgFunctionExpression.AggregateOrderings.Count];
                    for (var j = 0; j < i; j++)
                    {
                        visitedOrderings[j] = pgFunctionExpression.AggregateOrderings[j];
                    }

                    aggregateChanged = true;
                }

                if (visitedOrderings is not null)
                {
                    visitedOrderings[i] = visitedOrdering;
                }
            }

            return aggregateChanged
                ? pgFunctionExpression.UpdateAggregateComponents(
                    visitedAggregatePredicate,
                    visitedOrderings ?? pgFunctionExpression.AggregateOrderings)
                : pgFunctionExpression;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitILike(PostgresILikeExpression iLikeExpression, bool allowOptimizedExpansion, out bool nullable)
    {
        Check.NotNull(iLikeExpression, nameof(iLikeExpression));

        var like = new LikeExpression(
            iLikeExpression.Match,
            iLikeExpression.Pattern,
            iLikeExpression.EscapeChar,
            iLikeExpression.TypeMapping);

        var visited = base.VisitLike(like, allowOptimizedExpansion, out nullable);

        return visited == like
            ? iLikeExpression
            : visited is LikeExpression visitedLike
                ? iLikeExpression.Update(visitedLike.Match, visitedLike.Pattern, visitedLike.EscapeChar)
                : visited;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitNewArray(
        PostgresNewArrayExpression newArrayExpression, bool allowOptimizedExpansion, out bool nullable)
    {
        Check.NotNull(newArrayExpression, nameof(newArrayExpression));

        SqlExpression[]? newInitializers = null;
        for (var i = 0; i < newArrayExpression.Expressions.Count; i++)
        {
            var initializer = newArrayExpression.Expressions[i];
            var newInitializer = Visit(initializer, allowOptimizedExpansion, out _);
            if (newInitializer != initializer && newInitializers is null)
            {
                newInitializers = new SqlExpression[newArrayExpression.Expressions.Count];
                for (var j = 0; j < i; j++)
                {
                    newInitializers[j] = newArrayExpression.Expressions[j];
                }
            }

            if (newInitializers is not null)
            {
                newInitializers[i] = newInitializer;
            }
        }

        nullable = false;
        return newInitializers is null
            ? newArrayExpression
            : new PostgresNewArrayExpression(newInitializers, newArrayExpression.Type, newArrayExpression.TypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitRegexMatch(
        PostgresRegexMatchExpression regexMatchExpression, bool allowOptimizedExpansion, out bool nullable)
    {
        Check.NotNull(regexMatchExpression, nameof(regexMatchExpression));

        var match = Visit(regexMatchExpression.Match, out var matchNullable);
        var pattern = Visit(regexMatchExpression.Pattern, out var patternNullable);

        nullable = matchNullable || patternNullable;

        return regexMatchExpression.Update(match, pattern);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitJsonTraversal(
        PostgresJsonTraversalExpression jsonTraversalExpression, bool allowOptimizedExpansion, out bool nullable)
    {
        Check.NotNull(jsonTraversalExpression, nameof(jsonTraversalExpression));

        var expression = Visit(jsonTraversalExpression.Expression, out _);

        SqlExpression[]? newPath = null;
        for (var i = 0; i < jsonTraversalExpression.Path.Count; i++)
        {
            var pathComponent = jsonTraversalExpression.Path[i];
            var newPathComponent = Visit(pathComponent, allowOptimizedExpansion, out _);
            if (newPathComponent != pathComponent && newPath is null)
            {
                newPath = new SqlExpression[jsonTraversalExpression.Path.Count];
                for (var j = 0; j < i; j++)
                {
                    newPath[j] = jsonTraversalExpression.Path[j];
                }
            }

            if (newPath is not null)
            {
                newPath[i] = newPathComponent;
            }
        }

        // For now, anything inside a JSON document is considered nullable.
        // See #1851 for optimizing this for JSON POCO mapping.
        nullable = true;

        return jsonTraversalExpression.Update(expression, newPath?.ToArray() ?? jsonTraversalExpression.Path);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitRowValueExpression(
        PostgresRowValueExpression rowValueExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        SqlExpression[]? newValues = null;

        for (var i = 0; i < rowValueExpression.Values.Count; i++)
        {
            var value = rowValueExpression.Values[i];

            // Note that we disallow optimized expansion, since the null vs. false distinction does matter inside the row's values
            var newValue = Visit(value, allowOptimizedExpansion: false, out _);
            if (newValue != value && newValues is null)
            {
                newValues = new SqlExpression[rowValueExpression.Values.Count];
                for (var j = 0; j < i; j++)
                {
                    newValues[j] = rowValueExpression.Values[j];
                }
            }

            if (newValues is not null)
            {
                newValues[i] = newValue;
            }
        }

        // The row value expression itself can never be null
        nullable = false;

        return rowValueExpression.Update(newValues ?? rowValueExpression.Values);
    }

    /// <summary>
    /// Visits a <see cref="PostgresUnknownBinaryExpression" /> and computes its nullability.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="unknownBinaryExpression">A <see cref="PostgresUnknownBinaryExpression" /> expression to visit.</param>
    /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
    /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
    /// <returns>An optimized sql expression.</returns>
    protected virtual SqlExpression VisitUnknownBinary(
        PostgresUnknownBinaryExpression unknownBinaryExpression, bool allowOptimizedExpansion, out bool nullable)
    {
        Check.NotNull(unknownBinaryExpression, nameof(unknownBinaryExpression));

        var left = Visit(unknownBinaryExpression.Left, allowOptimizedExpansion, out var leftNullable);
        var right = Visit(unknownBinaryExpression.Right, allowOptimizedExpansion, out var rightNullable);

        nullable = leftNullable || rightNullable;

        return unknownBinaryExpression.Update(left, right);
    }

    private static bool MayContainNulls(SqlExpression arrayExpression)
    {
        if (arrayExpression is SqlConstantExpression constantArrayExpression &&
            constantArrayExpression.Value is Array constantArray)
        {
            for (var i = 0; i < constantArray.Length; i++)
            {
                if (constantArray.GetValue(i) is null)
                {
                    return true;
                }
            }

            return false;
        }

        return true;
    }
}
