using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    /// <inheritdoc />
    public class NpgsqlSqlNullabilityProcessor : SqlNullabilityProcessor
    {
        readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        /// Creates a new instance of the <see cref="NpgsqlSqlNullabilityProcessor" /> class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
        /// <param name="useRelationalNulls">A bool value indicating whether relational null semantics are in use.</param>
        public NpgsqlSqlNullabilityProcessor(
            [NotNull] RelationalParameterBasedSqlProcessorDependencies dependencies,
            bool useRelationalNulls)
            : base(dependencies, useRelationalNulls)
            => _sqlExpressionFactory = dependencies.SqlExpressionFactory;

        /// <inheritdoc />
        protected override SqlExpression VisitCustomSqlExpression(
            SqlExpression sqlExpression, bool allowOptimizedExpansion, out bool nullable)
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
                PostgresNewArrayExpression newArrayExpression
                => VisitNewArray(newArrayExpression, allowOptimizedExpansion, out nullable),
                PostgresRegexMatchExpression regexMatchExpression
                => VisitRegexMatch(regexMatchExpression, allowOptimizedExpansion, out nullable),
                PostgresJsonTraversalExpression postgresJsonTraversalExpression
                => VisitJsonTraversal(postgresJsonTraversalExpression, allowOptimizedExpansion, out nullable),
                PostgresUnknownBinaryExpression postgresUnknownBinaryExpression
                => VisitUnknownBinary(postgresUnknownBinaryExpression, allowOptimizedExpansion, out nullable),

                _ => base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable)
            };

        /// <summary>
        /// Visits a <see cref="PostgresAnyExpression" /> and computes its nullability.
        /// </summary>
        /// <param name="anyExpression">A <see cref="PostgresAnyExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitAny(
            [NotNull] PostgresAnyExpression anyExpression, bool allowOptimizedExpansion, out bool nullable)
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

            if (array is SqlConstantExpression)
            {
                throw new ArgumentException(
                    $"{nameof(PostgresAnyExpression)} is not supported with operator {anyExpression.OperatorType}, " +
                    $"use {nameof(InExpression)} instead.");
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
                updated = _sqlExpressionFactory.And(updated, _sqlExpressionFactory.IsNotNull(updated));

            if (!itemNullable)
                return updated;

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
        /// Visits a <see cref="PostgresAnyExpression" /> and computes its nullability.
        /// </summary>
        /// <param name="allExpression">A <see cref="PostgresAnyExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitAll(
            [NotNull] PostgresAllExpression allExpression, bool allowOptimizedExpansion, out bool nullable)
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
        /// Visits an <see cref="PostgresArrayIndexExpression" /> and computes its nullability.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="arrayIndexExpression">A <see cref="PostgresArrayIndexExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitArrayIndex(
            [NotNull] PostgresArrayIndexExpression arrayIndexExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(arrayIndexExpression, nameof(arrayIndexExpression));

            var array = Visit(arrayIndexExpression.Array, allowOptimizedExpansion, out var arrayNullable);
            var index = Visit(arrayIndexExpression.Index, allowOptimizedExpansion, out var indexNullable);

            nullable = arrayNullable || indexNullable;

            return arrayIndexExpression.Update(array, index);
        }

        /// <summary>
        /// Visits a <see cref="PostgresBinaryExpression" /> and computes its nullability.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="binaryExpression">A <see cref="PostgresBinaryExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitBinary(
            [NotNull] PostgresBinaryExpression binaryExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            // For arrays Contains (arr1 @> arr2), we do not implement null semantics because doing so would prevent
            // index use. So '{1, 2, NULL}' @> '{NULL}' returns false. This is notably the translation for .NET
            // Contains over column arrays.

            var left = Visit(binaryExpression.Left, allowOptimizedExpansion, out var leftNullable);
            var right = Visit(binaryExpression.Right, allowOptimizedExpansion, out var rightNullable);

            nullable = leftNullable || rightNullable;

            return binaryExpression.Update(left, right);
        }

        /// <summary>
        /// Visits a <see cref="PostgresILikeExpression" /> and computes its nullability.
        /// </summary>
        /// <param name="iLikeExpression">A <see cref="PostgresILikeExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitILike(
            [NotNull] PostgresILikeExpression iLikeExpression, bool allowOptimizedExpansion, out bool nullable)
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
        /// Visits a <see cref="PostgresNewArrayExpression" /> and computes its nullability.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="newArrayExpression">A <see cref="PostgresNewArrayExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitNewArray(
            [NotNull] PostgresNewArrayExpression newArrayExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(newArrayExpression, nameof(newArrayExpression));

            List<SqlExpression> newInitializers = null;
            for (var i = 0; i < newArrayExpression.Initializers.Count; i++)
            {
                var initializer = newArrayExpression.Initializers[i];
                var newInitializer = Visit(initializer, allowOptimizedExpansion, out _);
                if (newInitializer != initializer && newInitializers is null)
                {
                    newInitializers = new List<SqlExpression>();
                    for (var j = 0; j < i; j++)
                        newInitializers.Add(newInitializer);
                }

                if (newInitializers != null)
                    newInitializers.Add(newInitializer);
            }

            nullable = false;
            return newInitializers is null
                ? newArrayExpression
                : new PostgresNewArrayExpression(newInitializers, newArrayExpression.Type, newArrayExpression.TypeMapping);
        }

        /// <summary>
        /// Visits a <see cref="PostgresRegexMatchExpression" /> and computes its nullability.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="regexMatchExpression">A <see cref="PostgresRegexMatchExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitRegexMatch(
            [NotNull] PostgresRegexMatchExpression regexMatchExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(regexMatchExpression, nameof(regexMatchExpression));

            var match = Visit(regexMatchExpression.Match, out var matchNullable);
            var pattern = Visit(regexMatchExpression.Pattern, out var patternNullable);

            nullable = matchNullable || patternNullable;

            return regexMatchExpression.Update(match, pattern);
        }

        /// <summary>
        /// Visits a <see cref="PostgresJsonTraversalExpression" /> and computes its nullability.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="jsonTraversalExpression">A <see cref="PostgresJsonTraversalExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitJsonTraversal(
            [NotNull] PostgresJsonTraversalExpression jsonTraversalExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(jsonTraversalExpression, nameof(jsonTraversalExpression));

            var expression = Visit(jsonTraversalExpression.Expression, out nullable);

            List<SqlExpression> newPath = null;
            for (var i = 0; i < jsonTraversalExpression.Path.Count; i++)
            {
                var pathComponent = jsonTraversalExpression.Path[i];
                var newPathComponent = Visit(pathComponent, allowOptimizedExpansion, out var nullablePathComponent);
                nullable |= nullablePathComponent;
                if (newPathComponent != pathComponent && newPath is null)
                {
                    newPath = new List<SqlExpression>();
                    for (var j = 0; j < i; j++)
                        newPath.Add(newPathComponent);
                }

                if (newPath != null)
                    newPath.Add(newPathComponent);
            }

            nullable = false;

            return jsonTraversalExpression.Update(
                expression,
                newPath is null
                    ? jsonTraversalExpression.Path
                    : newPath.ToArray());
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
            [NotNull] PostgresUnknownBinaryExpression unknownBinaryExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(unknownBinaryExpression, nameof(unknownBinaryExpression));

            var left = Visit(unknownBinaryExpression.Left, allowOptimizedExpansion, out var leftNullable);
            var right = Visit(unknownBinaryExpression.Right, allowOptimizedExpansion, out var rightNullable);

            nullable = leftNullable || rightNullable;

            return unknownBinaryExpression.Update(left, right);
        }

        static bool MayContainNulls(SqlExpression arrayExpression)
        {
            if (arrayExpression is SqlConstantExpression constantArrayExpression &&
                constantArrayExpression.Value is Array constantArray)
            {
                for (var i = 0; i < constantArray.Length; i++)
                    if (constantArray.GetValue(i) is null)
                        return true;
                return false;
            }

            return true;
        }
    }
}
