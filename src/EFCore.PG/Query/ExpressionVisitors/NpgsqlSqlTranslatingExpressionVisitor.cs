using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    public class NpgsqlSqlTranslatingExpressionVisitor : SqlTranslatingExpressionVisitor
    {
        private readonly RelationalQueryModelVisitor _queryModelVisitor;

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
        {
            // Prefer the default EF Core translation if one exists
            var result = base.VisitSubQuery(expression);
            if (result != null)
                return result;

            var subQueryModel = expression.QueryModel;
            var fromExpression = subQueryModel.MainFromClause.FromExpression;

            var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                fromExpression, _queryModelVisitor.QueryCompilationContext, out var qsre);

            if (properties.Count == 0)
                return null;
            var lastPropertyType = properties[properties.Count - 1].ClrType;
            if (lastPropertyType.IsArray && lastPropertyType.GetArrayRank() == 1)
            {
                // Translate someArray.Length
                if (subQueryModel.ResultOperators.First() is CountResultOperator)
                    return Expression.ArrayLength(Visit(fromExpression));

                // Translate someArray.Contains(someValue)
                if (subQueryModel.ResultOperators.First() is ContainsResultOperator contains)
                {
                    var containsItem = Visit(contains.Item);
                    if (containsItem != null)
                        return new ArrayAnyExpression(containsItem, Visit(fromExpression));
                }
            }

            return null;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                    expression.Left, _queryModelVisitor.QueryCompilationContext, out var qsre);
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
    }
}
