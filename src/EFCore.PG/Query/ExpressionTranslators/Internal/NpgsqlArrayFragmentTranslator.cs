using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for
    /// <see cref="Array.Exists{T}(T[],Predicate{T})"/> and <see cref="List{T}.Exists(Predicate{T})"/>
    /// as PostgreSQL array operations.
    /// </summary>
    public class NpgsqlArrayFragmentTranslator : IExpressionFragmentTranslator
    {
        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(Expression expression)
        {
            if (!(expression is SubQueryExpression subQuery))
                return null;

            if (subQuery.QueryModel.BodyClauses.Count != 1)
                return null;

            if (!(subQuery.QueryModel.BodyClauses[0] is WhereClause where))
                return null;

            if (!(where.Predicate is BinaryExpression b))
                return null;

            var qsre = b.Left as QuerySourceReferenceExpression ?? b.Right as QuerySourceReferenceExpression;
            if (qsre is null)
                return null;

            var operand = b.Left is QuerySourceReferenceExpression ? b.Right : b.Left;

            if (qsre.ReferencedQuerySource is MainFromClause mfc &&
                mfc.FromExpression is Expression from &&
                IsArrayOrList(from.Type))
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (b.NodeType)
                {
                case ExpressionType.Equal:
                    return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "=", operand, from);

                case ExpressionType.NotEqual:
                    return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "<>", operand, from);

                // TODO: the direction of the lt/lte/gt/gte operators depends on where the array is (left/right).

                case ExpressionType.LessThan:
                    return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "<", operand, from);

                case ExpressionType.LessThanOrEqual:
                    return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "<=", operand, from);

                case ExpressionType.GreaterThan:
                    return new ArrayAnyAllExpression(ArrayComparisonType.ANY, ">", operand, from);

                case ExpressionType.GreaterThanOrEqual:
                    return new ArrayAnyAllExpression(ArrayComparisonType.ANY, ">=", operand, from);

                default:
                    return null;
                }
            }

            return null;
        }

        #region Helpers

        /// <summary>
        /// Tests if the type is an array or a <see cref="List{T}"/>.
        /// </summary>
        /// <param name="type">
        /// The type to test.
        /// </param>
        /// <returns>
        /// True if <paramref name="type"/> is an array or a <see cref="List{T}"/>; otherwise, false.
        /// </returns>
        static bool IsArrayOrList([NotNull] Type type) => type.IsArray || type.IsGenericType && typeof(List<>) == type.GetGenericTypeDefinition();

        #endregion
    }
}
