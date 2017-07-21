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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query
{
    class NpgsqlQueryModelVisitor : RelationalQueryModelVisitor
    {
        public NpgsqlQueryModelVisitor(
            [NotNull] EntityQueryModelVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitorDependencies relationalDependencies,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [CanBeNull] RelationalQueryModelVisitor parentQueryModelVisitor)
            : base(dependencies, relationalDependencies, queryCompilationContext, parentQueryModelVisitor)
        {
        }

        protected override void OptimizeQueryModel(
            QueryModel queryModel,
            bool asyncQuery)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var arrayTranslatingVisitor = new ArrayTranslatingVisitor(this);

            queryModel.TransformExpressions(arrayTranslatingVisitor.Visit);

            base.OptimizeQueryModel(queryModel, asyncQuery);
        }

        class ArrayTranslatingVisitor : ExpressionVisitorBase
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;

            internal ArrayTranslatingVisitor(RelationalQueryModelVisitor queryModelVisitor)
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
                var lastProperty = properties[properties.Count - 1];
                if (lastProperty.ClrType.IsArray)
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
                    var lastProperty = properties[properties.Count - 1];
                    if (lastProperty.ClrType.IsArray)
                    {
                        return expression;
                        /*
                        var left = Visit(expression.Left);
                        var right = Visit(expression.Right);

                        return left != null && right != null
                            ? Expression.MakeBinary(ExpressionType.ArrayIndex, left, right)
                            : null;
                            */
                    }
                }
                return base.VisitBinary(expression);
            }
        }
    }
}
