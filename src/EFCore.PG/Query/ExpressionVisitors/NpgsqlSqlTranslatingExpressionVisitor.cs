using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionVisitors
{
    /// <summary>
    /// The default relational LINQ translating expression visitor for Npgsql.
    /// </summary>
    public class NpgsqlSqlTranslatingExpressionVisitor : SqlTranslatingExpressionVisitor
    {
        /// <summary>
        /// The current query model visitor.
        /// </summary>
        [NotNull] readonly RelationalQueryModelVisitor _queryModelVisitor;

        /// <summary>
        /// The current query compilation context.
        /// </summary>
        [NotNull]
        RelationalQueryCompilationContext Context => _queryModelVisitor.QueryCompilationContext;

        /// <inheritdoc />
        public NpgsqlSqlTranslatingExpressionVisitor(
            [NotNull] SqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] SelectExpression targetSelectExpression = null,
            [CanBeNull] Expression topLevelPredicate = null,
            bool inProjection = false)
            : base(dependencies, queryModelVisitor, targetSelectExpression, topLevelPredicate, inProjection)
            => _queryModelVisitor = queryModelVisitor;

        #region Overrides

        /// <inheritdoc />
        protected override Expression VisitBinary(BinaryExpression expression)
        {
            var left = Visit(expression.Left);
            var right = Visit(expression.Right);

            if (left == null || right == null)
                return base.VisitBinary(expression);

            if (MemberAccessBindingExpressionVisitor.GetPropertyPath(expression.Left, Context, out _).Count != 0)
            {
                if (expression.NodeType == ExpressionType.ArrayIndex)
                    return Expression.ArrayIndex(left, right);

                if (expression.NodeType == ExpressionType.Index)
                    return Expression.ArrayAccess(left, right);
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
        protected override Expression VisitSubQuery(SubQueryExpression expression)
            => base.VisitSubQuery(expression) ?? VisitArraySubQuery(expression);

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression expression)
        {
            // Character literals are represented as integer literals by the expression tree.
            // So `myString[0] == 'T'` looks like `((int)myString[0]) == 84`.
            // Since `substr` returns `text`, not `char` or `int`, wrap it in `ascii`.
            if (expression.NodeType == ExpressionType.Convert &&
                Visit(expression.Operand) is IndexExpression index &&
                index.Object.Type == typeof(string))
                return new SqlFunctionExpression("ascii", typeof(int), new[] { index });

            return base.VisitUnary(expression);
        }

        /// <inheritdoc />
        /// <remarks>
        /// https://github.com/aspnet/EntityFrameworkCore/blob/release/2.2/src/EFCore.Relational/Query/ExpressionVisitors/SqlTranslatingExpressionVisitor.cs#L1077-L1144
        /// </remarks>
        protected override Expression VisitExtension(Expression expression)
        {
            switch (expression)
            {
            case ArrayAnyAllExpression e:
            {
                var operand = Visit(e.Operand);
                var array = Visit(e.Array);

                if (operand == null || array == null)
                    return null;

                return
                    operand != e.Operand || array != e.Array
                        ? new ArrayAnyAllExpression(e.ArrayComparisonType, e.Operator, operand, array)
                        : e;
            }

            case CustomBinaryExpression e:
            {
                var left = Visit(e.Left);
                var right = Visit(e.Right);

                if (left == null || right == null)
                    return null;

                return
                    left != e.Left || right != e.Right
                        ? new CustomBinaryExpression(left, right, e.Operator, e.Type)
                        : e;
            }

            default:
                return base.VisitExtension(expression);
            }
        }

        #endregion

        #region ArraySubQueries

        /// <summary>
        /// Visits an array-based subquery.
        /// </summary>
        /// <param name="expression">The subquery expression.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        protected virtual Expression VisitArraySubQuery([NotNull] SubQueryExpression expression)
        {
            var model = expression.QueryModel;
            var from = model.MainFromClause.FromExpression;
            var results = model.ResultOperators;

            // Only handle types mapped to PostgreSQL arrays.
            if (!IsArrayOrList(from.Type))
                return null;

            // Only handle subqueries when the from expression is visitable.
            if (!(Visit(from) is Expression array))
                return null;

            // Only handle singular result operators.
            if (results.Count != 1)
                return null;

            switch (results[0])
            {
            case ContainsResultOperator contains:
                return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "=", Visit(contains.Item) ?? contains.Item, array);

            default:
                return null;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Tests if the type is an array or a <see cref="List{T}"/>.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <returns>
        /// True if <paramref name="type"/> is an array or a <see cref="List{T}"/>; otherwise, false.
        /// </returns>
        static bool IsArrayOrList([NotNull] Type type) => type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

        #endregion
    }
}
