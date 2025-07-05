using System.Runtime.CompilerServices;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <inheritdoc />
public class NpgsqlSqlNullabilityProcessor : SqlNullabilityProcessor
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlSqlNullabilityProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        RelationalParameterBasedSqlProcessorParameters parameters)
        : base(dependencies, parameters)
    {
        _sqlExpressionFactory = dependencies.SqlExpressionFactory;
    }

    /// <inheritdoc />
    protected override SqlExpression VisitSqlBinary(
        SqlBinaryExpression sqlBinaryExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        return sqlBinaryExpression switch
        {
            {
                OperatorType: ExpressionType.Equal or ExpressionType.NotEqual,
                Left: PgRowValueExpression leftRowValue,
                Right: PgRowValueExpression rightRowValue
            }
            => VisitRowValueComparison(sqlBinaryExpression.OperatorType, leftRowValue, rightRowValue, out nullable),

            _ => base.VisitSqlBinary(sqlBinaryExpression, allowOptimizedExpansion, out nullable)
        };

        SqlExpression VisitRowValueComparison(
            ExpressionType operatorType,
            PgRowValueExpression leftRowValue,
            PgRowValueExpression rightRowValue,
            out bool nullable)
        {
            // Equality checks between row values require some special null semantics compensation.
            // Row value equality/inequality works the same as regular equals/non-equals; this means that it's fine as long as we're
            // comparing non-nullable values (no need to compensate), but for nullable values, we need to compensate. We go over the value
            // pairs, and extract out pairs that require compensation to an expanded, non-value-tuple expression (regular equality null
            // semantics). Note that PostgreSQL does have DISTINCT FROM/NOT DISTINCT FROM which would have been perfect here, but those
            // still don't use indexes.
            // Note that we don't do compensation for comparisons (e.g. greater than) since these are expressed via EF.Functions, which
            // correspond directly to SQL constructs.
            // The PG docs around this are in https://www.postgresql.org/docs/current/functions-comparisons.html#ROW-WISE-COMPARISON

            Check.DebugAssert(leftRowValue.Values.Count == rightRowValue.Values.Count, "left.Values.Count == right.Values.Count");
            var count = leftRowValue.Values.Count;

            SqlExpression? expandedExpression = null;
            List<SqlExpression>? visitedLeftValues = null;
            List<SqlExpression>? visitedRightValues = null;

            for (var i = 0; i < count; i++)
            {
                // Visit the left value, populating visitedLeftValues only if we haven't yet switched to an expanded expression, and only if
                // the visitation actually changed something
                var leftValue = leftRowValue.Values[i];
                var rightValue = rightRowValue.Values[i];
                var visitedLeftValue = Visit(leftRowValue.Values[i], out var leftNullable);
                var visitedRightValue = Visit(rightRowValue.Values[i], out var rightNullable);

                // If both sides are non-nullable, no null expansion is required; the same is true if we're doing equality in optimized
                // mode, and only one side is nullable (WHERE (NonNullable1, NonNullable2) = (NonNullable3, Nullable4)).
                if (!leftNullable && !rightNullable
                    || allowOptimizedExpansion && operatorType is ExpressionType.Equal && (!leftNullable || !rightNullable))
                {
                    // The comparison for this value pair doesn't require expansion and can remain in the row value (so continue below).
                    // But if the visitation above changed a value, construct a list to hold the visited values.
                    if (visitedLeftValue != leftValue && visitedLeftValues is null)
                    {
                        visitedLeftValues = SliceToList(leftRowValue.Values, count, i);
                    }

                    visitedLeftValues?.Add(visitedLeftValue);

                    if (visitedRightValue != rightValue && visitedRightValues is null)
                    {
                        visitedRightValues = SliceToList(rightRowValue.Values, count, i);
                    }

                    visitedRightValues?.Add(visitedRightValue);

                    continue;
                }

                // If we're here, the value pair requires null semantics compensation. We build a binary expression around the pair and
                // visit that (that adds the compensation). We then chain all such expressions together with AND.
                var valueBinaryExpression = Visit(
                    _sqlExpressionFactory.MakeBinary(
                        operatorType, visitedLeftValue, visitedRightValue, typeMapping: null, existingExpression: sqlBinaryExpression)!,
                    allowOptimizedExpansion,
                    out _);

                if (expandedExpression is null)
                {
                    // visitedLeft/RightValues will contain all pairs that can remain in the row value (since they don't require
                    // compensation)
                    visitedLeftValues = SliceToList(leftRowValue.Values, count, i);
                    visitedRightValues = SliceToList(rightRowValue.Values, count, i);

                    expandedExpression = valueBinaryExpression;
                }
                else
                {
                    expandedExpression = operatorType switch
                    {
                        ExpressionType.Equal => _sqlExpressionFactory.AndAlso(expandedExpression, valueBinaryExpression),
                        ExpressionType.NotEqual => _sqlExpressionFactory.OrElse(expandedExpression, valueBinaryExpression),
                        _ => throw new UnreachableException()
                    };
                }
            }

            // TODO: This is wrong, need to properly calculate this: #3250
            nullable = false;

            if (expandedExpression is null)
            {
                // No pairs required compensation, so they all stay in the row value.
                // Either return the original binary expression (if no value visitation changed anything), or construct a new one over the
                // visited values.
                return visitedLeftValues is null && visitedRightValues is null
                    ? sqlBinaryExpression
                    : _sqlExpressionFactory.MakeBinary(
                        operatorType,
                        visitedLeftValues is null
                            ? leftRowValue
                            : new PgRowValueExpression(visitedLeftValues, leftRowValue.Type, leftRowValue.TypeMapping),
                        visitedRightValues is null
                            ? rightRowValue
                            : new PgRowValueExpression(visitedRightValues, leftRowValue.Type, leftRowValue.TypeMapping),
                        typeMapping: null,
                        existingExpression: sqlBinaryExpression)!;
            }

            Check.DebugAssert(visitedLeftValues is not null, "visitedLeftValues is not null");
            Check.DebugAssert(visitedRightValues is not null, "visitedRightValues is not null");

            // All pairs required compensation - none are left in the row value comparison. Just return the expanded expression.
            if (visitedLeftValues.Count is 0)
            {
                return expandedExpression;
            }

            // Some pairs required compensation; we're going to return a combined expression of the unexpanded pairs (which didn't require
            // compensation) and of the expanded expression.
            // If there's only one unexpanded pair left, that's a special case that doesn't require row values (just a regular pair
            // comparison). Otherwise create the new row comparison expression for the unexpanded pairs.
            var unexpandedExpression = visitedLeftValues.Count is 1
                ? _sqlExpressionFactory.MakeBinary(operatorType, visitedLeftValues[0], visitedRightValues[0], typeMapping: null)!
                : _sqlExpressionFactory.MakeBinary(
                    operatorType,
                    new PgRowValueExpression(visitedLeftValues, leftRowValue.Type, leftRowValue.TypeMapping),
                    new PgRowValueExpression(visitedRightValues, rightRowValue.Type, rightRowValue.TypeMapping),
                    typeMapping: null)!;

            // Technically the CLR type and type mappings are incorrect, as we're truncating the row values.
            // But that shouldn't matter.
            return _sqlExpressionFactory.MakeBinary(
                operatorType: operatorType switch
                {
                    ExpressionType.Equal => ExpressionType.AndAlso,
                    ExpressionType.NotEqual => ExpressionType.OrElse,
                    _ => throw new UnreachableException()
                },
                unexpandedExpression,
                expandedExpression,
                typeMapping: null)!;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static List<SqlExpression> SliceToList(IReadOnlyList<SqlExpression> source, int capacity, int count)
            {
                var list = new List<SqlExpression>(capacity);

                for (var i = 0; i < count; i++)
                {
                    list.Add(source[i]);
                }

                return list;
            }
        }
    }

    /// <inheritdoc />
    protected override SqlExpression VisitCustomSqlExpression(
        SqlExpression sqlExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
        => sqlExpression switch
        {
            PgAnyExpression e => VisitAny(e, allowOptimizedExpansion, out nullable),
            PgAllExpression e => VisitAll(e, allowOptimizedExpansion, out nullable),
            PgArrayIndexExpression e => VisitArrayIndex(e, allowOptimizedExpansion, out nullable),
            PgArraySliceExpression e => VisitArraySlice(e, allowOptimizedExpansion, out nullable),
            PgBinaryExpression e => VisitPostgresBinary(e, allowOptimizedExpansion, out nullable),
            PgILikeExpression e => VisitILike(e, allowOptimizedExpansion, out nullable),
            PgJsonTraversalExpression e => VisitJsonTraversal(e, allowOptimizedExpansion, out nullable),
            PgNewArrayExpression e => VisitNewArray(e, allowOptimizedExpansion, out nullable),
            PgRegexMatchExpression e => VisitRegexMatch(e, allowOptimizedExpansion, out nullable),
            PgRowValueExpression e => VisitRowValueExpression(e, allowOptimizedExpansion, out nullable),
            PgUnknownBinaryExpression e => VisitUnknownBinary(e, allowOptimizedExpansion, out nullable),

            // PostgresFunctionExpression is visited via the SqlFunctionExpression override below

            _ => base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable)
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitAny(PgAnyExpression anyExpression, bool allowOptimizedExpansion, out bool nullable)
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
        if (anyExpression.OperatorType != PgAnyOperatorType.Equal)
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
                        [array, _sqlExpressionFactory.Constant(null, item.Type, item.TypeMapping)],
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
    protected virtual SqlExpression VisitAll(PgAllExpression allExpression, bool allowOptimizedExpansion, out bool nullable)
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
        PgArrayIndexExpression arrayIndexExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        Check.NotNull(arrayIndexExpression, nameof(arrayIndexExpression));

        var array = Visit(arrayIndexExpression.Array, allowOptimizedExpansion, out var arrayNullable);
        var index = Visit(arrayIndexExpression.Index, allowOptimizedExpansion, out var indexNullable);

        nullable = arrayNullable || indexNullable || arrayIndexExpression.IsNullable;

        return arrayIndexExpression.Update(array, index);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitArraySlice(
        PgArraySliceExpression arraySliceExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        Check.NotNull(arraySliceExpression, nameof(arraySliceExpression));

        var array = Visit(arraySliceExpression.Array, allowOptimizedExpansion, out var arrayNullable);
        var lowerBound = Visit(arraySliceExpression.LowerBound, allowOptimizedExpansion, out var lowerBoundNullable);
        var upperBound = Visit(arraySliceExpression.UpperBound, allowOptimizedExpansion, out var upperBoundNullable);

        nullable = arrayNullable || lowerBoundNullable || upperBoundNullable || arraySliceExpression.IsNullable;

        return arraySliceExpression.Update(array, lowerBound, upperBound);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitPostgresBinary(
        PgBinaryExpression binaryExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
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
            PgExpressionType.LTreeFirstAncestor => true,
            PgExpressionType.LTreeFirstDescendent => true,
            PgExpressionType.LTreeFirstMatches => true,

            _ => leftNullable || rightNullable
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
            && coalesceExpression.Arguments[0] is PgFunctionExpression wrappedFunctionExpression)
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

        return visitedBase is PgFunctionExpression pgFunctionExpression
            ? VisitPostgresFunctionComponents(pgFunctionExpression)
            : visitedBase;

        PgFunctionExpression VisitPostgresFunctionComponents(PgFunctionExpression pgFunctionExpression)
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
    protected virtual SqlExpression VisitILike(PgILikeExpression iLikeExpression, bool allowOptimizedExpansion, out bool nullable)
    {
        // Note: this is largely duplicated from relational SqlNullabilityProcessor.VisitLike.
        // We unfortunately can't reuse that since it may return arbitrary expression tree structures with LikeExpression embedded, but
        // we need ILikeExpression (see #3034).
        var match = Visit(iLikeExpression.Match, out var matchNullable);
        var pattern = Visit(iLikeExpression.Pattern, out var patternNullable);
        var escapeChar = Visit(iLikeExpression.EscapeChar, out var escapeCharNullable);

        SqlExpression result = iLikeExpression.Update(match, pattern, escapeChar);

        if (UseRelationalNulls)
        {
            nullable = matchNullable || patternNullable || escapeCharNullable;

            return result;
        }

        nullable = false;

        // The null semantics behavior we implement for LIKE is that it only returns true when both sides are non-null and match; any other
        // input returns false:
        // foo LIKE f% -> true
        // foo LIKE null -> false
        // null LIKE f% -> false
        // null LIKE null -> false

        if (IsNull(match) || IsNull(pattern) || IsNull(escapeChar))
        {
            return _sqlExpressionFactory.Constant(false, iLikeExpression.TypeMapping);
        }

        // A constant match-all pattern (%) returns true for all cases, except where the match string is null:
        // nullable_foo LIKE % -> foo IS NOT NULL
        // non_nullable_foo LIKE % -> true
        if (pattern is SqlConstantExpression { Value: "%" })
        {
            return matchNullable
                ? _sqlExpressionFactory.IsNotNull(match)
                : _sqlExpressionFactory.Constant(true, iLikeExpression.TypeMapping);
        }

        if (!allowOptimizedExpansion)
        {
            if (matchNullable)
            {
                result = _sqlExpressionFactory.AndAlso(result, GenerateNotNullCheck(match));
            }

            if (patternNullable)
            {
                result = _sqlExpressionFactory.AndAlso(result, GenerateNotNullCheck(pattern));
            }

            if (escapeChar is not null && escapeCharNullable)
            {
                result = _sqlExpressionFactory.AndAlso(result, GenerateNotNullCheck(escapeChar));
            }

            // TODO: This revisits the operand; ideally we'd call ProcessNullNotNull directly but that's private
            SqlExpression GenerateNotNullCheck(SqlExpression operand)
                => _sqlExpressionFactory.Not(
                    Visit(_sqlExpressionFactory.IsNull(operand), allowOptimizedExpansion, out _));
        }

        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitNewArray(
        PgNewArrayExpression newArrayExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
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
            : new PgNewArrayExpression(newInitializers, newArrayExpression.Type, newArrayExpression.TypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitRegexMatch(
        PgRegexMatchExpression regexMatchExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
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
        PgJsonTraversalExpression jsonTraversalExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
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
        PgRowValueExpression rowValueExpression,
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
    ///     Visits a <see cref="PgUnknownBinaryExpression" /> and computes its nullability.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="unknownBinaryExpression">A <see cref="PgUnknownBinaryExpression" /> expression to visit.</param>
    /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
    /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
    /// <returns>An optimized sql expression.</returns>
    protected virtual SqlExpression VisitUnknownBinary(
        PgUnknownBinaryExpression unknownBinaryExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        Check.NotNull(unknownBinaryExpression, nameof(unknownBinaryExpression));

        var left = Visit(unknownBinaryExpression.Left, allowOptimizedExpansion, out var leftNullable);
        var right = Visit(unknownBinaryExpression.Right, allowOptimizedExpansion, out var rightNullable);

        nullable = leftNullable || rightNullable;

        return unknownBinaryExpression.Update(left, right);
    }

    private static bool MayContainNulls(SqlExpression arrayExpression)
    {
        if (arrayExpression is SqlConstantExpression { Value: Array constantArray })
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

    // Note that we can check parameter values for null since we cache by the parameter nullability; but we cannot do the same for bool.
    private bool IsNull(SqlExpression? expression)
        => expression is SqlConstantExpression { Value: null }
            || expression is SqlParameterExpression { Name: string parameterName } && ParametersFacade.IsParameterNull(parameterName);
}
