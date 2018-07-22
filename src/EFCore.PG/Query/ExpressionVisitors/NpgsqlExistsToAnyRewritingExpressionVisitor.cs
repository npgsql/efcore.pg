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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionVisitors
{
    /// <summary>
    /// An expression rewriter for <see cref="Array.Exists{T}(T[],Predicate{T})"/>.
    /// </summary>
    public class NpgsqlExistsToAnyRewritingExpressionVisitor : ExpressionVisitorBase
    {
        /// <summary>
        /// The generic <see cref="MethodInfo"/> for <see cref="Array.Exists{T}(T[],Predicate{T})"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo Exists =
            typeof(Array).GetRuntimeMethods().Single(x => x.Name == nameof(Array.Exists));

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (!methodCallExpression.Method.MethodIsClosedFormOf(Exists))
                return methodCallExpression;

            if (!(methodCallExpression.Arguments[0] is Expression array))
                return methodCallExpression;

            if (!(methodCallExpression.Arguments[1] is LambdaExpression predicate))
                return methodCallExpression;

            var mainFromClause =
                new MainFromClause(
                    "<array_item>",
                    array.Type.GetElementType(),
                    array);

            var qsre = new QuerySourceReferenceExpression(mainFromClause);
            var queryModel = new QueryModel(mainFromClause, new SelectClause(qsre));

            var where =
                new WhereClause(
                    ReplacingExpressionVisitor.Replace(
                        predicate.Parameters[0],
                        qsre,
                        predicate.Body));

            queryModel.BodyClauses.Add(where);
            queryModel.ResultOperators.Add(new AnyResultOperator());
            queryModel.ResultTypeOverride = typeof(bool);

            return new SubQueryExpression(queryModel);
        }
    }
}
