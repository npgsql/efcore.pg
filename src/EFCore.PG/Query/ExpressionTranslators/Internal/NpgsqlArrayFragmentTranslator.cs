#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

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

//            if (ContainsResult(subQuery) is Expression contains)
//                return contains;

            if (subQuery.QueryModel.BodyClauses.Count != 1)
                return null;

            if (!(subQuery.QueryModel.BodyClauses[0] is WhereClause where))
                return null;

            if (!(where.Predicate is BinaryExpression b))
                return null;

            if (!TryFindArray(b, out Expression from, out ArrayPosition position) || from is null)
                return null;

            var operand = position is ArrayPosition.Left ? b.Right : b.Left;

            // In PostgreSQL, the array is on the right. Flip the sign if needed.
            bool flip = position is ArrayPosition.Left;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (b.NodeType)
            {
            case ExpressionType.Equal:
                return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "=", operand, from);

            case ExpressionType.NotEqual:
                return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "<>", operand, from);

            case ExpressionType.LessThan:
                return new ArrayAnyAllExpression(ArrayComparisonType.ANY, flip ? ">" : "<", operand, from);

            case ExpressionType.LessThanOrEqual:
                return new ArrayAnyAllExpression(ArrayComparisonType.ANY, flip ? ">=" : "<=", operand, from);

            case ExpressionType.GreaterThan:
                return new ArrayAnyAllExpression(ArrayComparisonType.ANY, flip ? "<" : ">", operand, from);

            case ExpressionType.GreaterThanOrEqual:
                return new ArrayAnyAllExpression(ArrayComparisonType.ANY, flip ? "<=" : ">=", operand, from);

            default:
                return null;
            }
        }

        #region SubQueries

        [CanBeNull]
        static Expression ContainsResult(SubQueryExpression expression)
        {
            var model = expression.QueryModel;

            if (!(model.MainFromClause.FromExpression is Expression from))
                return null;

            if (!IsArrayOrList(from.Type))
                return null;

            if (model.BodyClauses.Count != 0)
                return null;

            if (model.ResultOperators.Count != 1)
                return null;

            if (!(model.ResultOperators[0] is ContainsResultOperator contains))
                return null;

            return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "=", contains.Item, from);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Try to return the array expression and its position in the <see cref="BinaryExpression"/>.
        /// </summary>
        /// <param name="binaryExpression">The expression to visit.</param>
        /// <param name="array">The array expression, if found.</param>
        /// <param name="position">The postion of the array.</param>
        /// <returns>
        /// True if the array was found; otherwise, false.
        /// </returns>
        static bool TryFindArray([NotNull] BinaryExpression binaryExpression, [CanBeNull] out Expression array, out ArrayPosition position)
        {
            if (TryFindArray(binaryExpression.Left, out array))
            {
                position = ArrayPosition.Left;
                return true;
            }

            if (TryFindArray(binaryExpression.Right, out array))
            {
                position = ArrayPosition.Right;
                return true;
            }

            position = ArrayPosition.None;
            return false;
        }

        /// <summary>
        /// Try to return the array expression.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <param name="array">The array expression, if found.</param>
        /// <returns>
        /// True if the array was found; otherwise, false.
        /// </returns>
        static bool TryFindArray([NotNull] Expression expression, [CanBeNull] out Expression array)
        {
            switch (expression)
            {
            // Is one side a qsre pointing to an array?
            case QuerySourceReferenceExpression qsre
                when qsre.ReferencedQuerySource is MainFromClause mfc &&
                     mfc.FromExpression is Expression from &&
                     IsArrayOrList(from.Type):
                array = from;
                return true;

            // Is the expression a parameter array?
            case ParameterExpression param
                when IsArrayOrList(param.Type):
                array = param;
                return true;

            default:
                array = null;
                return false;
            }
        }

        /// <summary>
        /// Describes the position of an array in a <see cref="BinaryExpression"/>.
        /// </summary>
        private enum ArrayPosition
        {
            None,
            Left,
            Right
        }

        /// <summary>
        /// Tests if the type is an array or a <see cref="List{T}"/>.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <returns>
        /// True if <paramref name="type"/> is an array or a <see cref="List{T}"/>; otherwise, false.
        /// </returns>
        static bool IsArrayOrList([NotNull] Type type) => type.IsArray || type.IsGenericType && typeof(List<>) == type.GetGenericTypeDefinition();

        #endregion
    }
}
