using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    public class NpgsqlSqlTranslatingExpressionVisitor : SqlTranslatingExpressionVisitor
    {
        public NpgsqlSqlTranslatingExpressionVisitor(
            [NotNull] SqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] SelectExpression targetSelectExpression = null,
            [CanBeNull] Expression topLevelPredicate = null,
            bool inProjection = false)
            : base(dependencies, queryModelVisitor, targetSelectExpression, topLevelPredicate, inProjection)
        {
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            // Prefer the default EF Core translation if one exists
            var result = base.VisitSubQuery(expression);
            if (result != null)
                return result;

            // The following is to allow us to translate CLR array's .Length
            var subQueryModel = expression.QueryModel;

            // We want to only translate array length when it occurs on a property
            // of an EF entity
            if (subQueryModel.IsIdentityQuery()
                && subQueryModel.ResultOperators.Count == 1
                && subQueryModel.ResultOperators.First() is CountResultOperator
                && subQueryModel.MainFromClause.FromExpression is MemberExpression memberExpression
                && memberExpression.Type.IsArray
                && memberExpression.Expression is QuerySourceReferenceExpression
                )
            {
                return Expression.ArrayLength(Visit(memberExpression));
            }

            return null;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                // We want to only translate array indexing when it occurs on a property
                // of an EF entity
                if (!(expression.Left is MemberExpression memberExpression) ||
                    !(memberExpression.Expression is QuerySourceReferenceExpression))
                {
                    return base.VisitBinary(expression);
                }

                var left = Visit(expression.Left);
                var right = Visit(expression.Right);

                // ReSharper disable once AssignNullToNotNullAttribute
                return left != null && right != null
                    ? Expression.MakeBinary(ExpressionType.ArrayIndex, left, right)
                    : null;
            }
            return base.VisitBinary(expression);
        }
    }
}
