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

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionVisitors
{
    public class NpgsqlSqlTranslatingExpressionVisitor : SqlTranslatingExpressionVisitor
    {
        readonly RelationalQueryModelVisitor _queryModelVisitor;

        public NpgsqlSqlTranslatingExpressionVisitor(
            [NotNull] SqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] SelectExpression targetSelectExpression = null,
            [CanBeNull] Expression topLevelPredicate = null,
            bool inProjection = false)
            : base(dependencies, queryModelVisitor, targetSelectExpression, topLevelPredicate, inProjection)
        {
            _queryModelVisitor = queryModelVisitor;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
            => base.VisitSubQuery(expression) ?? VisitLikeAnyAll(expression) ?? VisitEqualsAny(expression);

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                    expression.Left, _queryModelVisitor.QueryCompilationContext, out _);
                if (properties.Count == 0)
                    return base.VisitBinary(expression);
                var lastPropertyType = properties[properties.Count - 1].ClrType;
                if (lastPropertyType.IsArray && lastPropertyType.GetArrayRank() == 1)
                {
                    var left = Visit(expression.Left);
                    var right = Visit(expression.Right);

                    return left != null && right != null
                        ? Expression.MakeBinary(ExpressionType.ArrayIndex, left, right)
                        : null;
                }
            }

            return base.VisitBinary(expression);
        }

        /// <summary>
        /// Visits a <see cref="SubQueryExpression"/> and attempts to translate a '= ANY' expression.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>
        /// An '= ANY' expression or null.
        /// </returns>
        [CanBeNull]
        protected virtual Expression VisitEqualsAny([NotNull] SubQueryExpression expression)
        {
            var subQueryModel = expression.QueryModel;
            var fromExpression = subQueryModel.MainFromClause.FromExpression;

            var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                fromExpression, _queryModelVisitor.QueryCompilationContext, out _);

            if (properties.Count == 0)
                return null;
            var lastPropertyType = properties[properties.Count - 1].ClrType;
            if (lastPropertyType.IsArray && lastPropertyType.GetArrayRank() == 1 && subQueryModel.ResultOperators.Count > 0)
            {
                // Translate someArray.Length
                if (subQueryModel.ResultOperators.First() is CountResultOperator)
                    return Expression.ArrayLength(Visit(fromExpression));

                // Translate someArray.Contains(someValue)
                if (subQueryModel.ResultOperators.First() is ContainsResultOperator contains)
                {
                    var containsItem = Visit(contains.Item);
                    if (containsItem != null)
                        return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "=", containsItem, Visit(fromExpression));
                }
            }

            return null;
        }

        /// <summary>
        /// Visits a <see cref="SubQueryExpression"/> and attempts to translate a LIKE/ILIKE ANY/ALL expression.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>
        /// A 'LIKE ANY', 'LIKE ALL', 'ILIKE ANY', or 'ILIKE ALL' expression or null.
        /// </returns>
        [CanBeNull]
        protected virtual Expression VisitLikeAnyAll([NotNull] SubQueryExpression expression)
        {
            var queryModel = expression.QueryModel;
            var results = queryModel.ResultOperators;

            if (results.Count != 1)
                return null;

            ArrayComparisonType comparisonType;
            MethodCallExpression call;
            switch (results[0])
            {
            case AnyResultOperator _:
                comparisonType = ArrayComparisonType.ANY;
                call = ComputeAny(queryModel);
                break;

            case AllResultOperator allResult:
                comparisonType = ArrayComparisonType.ALL;
                call = allResult.Predicate as MethodCallExpression;
                break;

            default:
                return null;
            }

            if (call is null)
                return null;

            var source = queryModel.MainFromClause.FromExpression;

            // ReSharper disable AssignNullToNotNullAttribute
            switch (call.Method.Name)
            {
// TODO: Can StartsWith and EndsWith be implemented correctly?
//            case "StartsWith":
//                return new ArrayAnyAllExpression(comparisonType, "LIKE", Visit(call.Object), Visit(source));
//
//            case "EndsWith":
//                return new ArrayAnyAllExpression(comparisonType, "LIKE", Visit(call.Object), Visit(source));

            case "Like":
                return new ArrayAnyAllExpression(comparisonType, "LIKE", Visit(call.Arguments[1]), Visit(source));

            case "ILike":
                return new ArrayAnyAllExpression(comparisonType, "ILIKE", Visit(call.Arguments[1]), Visit(source));

            default:
                return null;
            }
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [CanBeNull]
        static MethodCallExpression ComputeAny([NotNull] QueryModel queryModel)
        {
            var bodyClauses = queryModel.BodyClauses;

            if (bodyClauses.Count == 1 &&
                bodyClauses[0] is WhereClause whereClause &&
                whereClause.Predicate is MethodCallExpression call)
                return call;

            return null;
        }
    }
}
