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
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Remotion.Linq.Clauses;
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
        /// The <see cref="MethodInfo"/> for <see cref="DbFunctionsExtensions.Like(DbFunctions,string,string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo Like2MethodInfo =
            typeof(DbFunctionsExtensions)
                .GetRuntimeMethod(nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        /// <summary>
        /// The <see cref="MethodInfo"/> for <see cref="DbFunctionsExtensions.Like(DbFunctions,string,string, string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo Like3MethodInfo =
            typeof(DbFunctionsExtensions)
                .GetRuntimeMethod(nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// The <see cref="MethodInfo"/> for <see cref="NpgsqlDbFunctionsExtensions.ILike(DbFunctions,string,string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo ILike2MethodInfo =
            typeof(NpgsqlDbFunctionsExtensions)
                .GetRuntimeMethod(nameof(NpgsqlDbFunctionsExtensions.ILike), new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// The <see cref="MethodInfo"/> for <see cref="NpgsqlDbFunctionsExtensions.ILike(DbFunctions,string,string,string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo ILike3MethodInfo =
            typeof(NpgsqlDbFunctionsExtensions)
                .GetRuntimeMethod(nameof(NpgsqlDbFunctionsExtensions.ILike), new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });

        /// <summary>
        /// The query model visitor.
        /// </summary>
        [NotNull] readonly RelationalQueryModelVisitor _queryModelVisitor;

        /// <inheritdoc />
        public NpgsqlSqlTranslatingExpressionVisitor(
            [NotNull] SqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] SelectExpression targetSelectExpression = null,
            [CanBeNull] Expression topLevelPredicate = null,
            bool inProjection = false)
            : base(dependencies, queryModelVisitor, targetSelectExpression, topLevelPredicate, inProjection)
            => _queryModelVisitor = queryModelVisitor;

        /// <inheritdoc />
        protected override Expression VisitSubQuery(SubQueryExpression expression)
            => base.VisitSubQuery(expression) ?? VisitLikeAnyAll(expression) ?? VisitEqualsAny(expression);

        /// <inheritdoc />
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

                    if (left == null || right == null)
                        return base.VisitBinary(expression);

                    return Expression.MakeBinary(ExpressionType.ArrayIndex, left, right);
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
                {
                    if (Visit(fromExpression) is Expression from)
                        return Expression.ArrayLength(from);
                }

                // Translate someArray.Contains(someValue)
                if (subQueryModel.ResultOperators.First() is ContainsResultOperator contains)
                {
                    if (Visit(contains.Item) is Expression containsItem)
                    {
                        if (Visit(fromExpression) is Expression from)
                            return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "=", containsItem, from);
                    }
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
            var body = queryModel.BodyClauses;

            if (results.Count != 1)
                return null;

            ArrayComparisonType comparisonType;
            MethodCallExpression call;
            switch (results[0])
            {
            case AnyResultOperator _:
                comparisonType = ArrayComparisonType.ANY;
                call =
                    body.Count == 1 &&
                    body[0] is WhereClause whereClause &&
                    whereClause.Predicate is MethodCallExpression methocCall
                        ? methocCall
                        : null;
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

            if (!(Visit(queryModel.MainFromClause.FromExpression) is Expression patterns))
                return null;

            if (!(Visit(call.Arguments[1]) is Expression match))
                return null;

            switch (call.Method)
            {
            case MethodInfo m when m == Like2MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "LIKE", match, patterns);

            case MethodInfo m when m == Like3MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "LIKE", match, patterns);

            case MethodInfo m when m == ILike2MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "ILIKE", match, patterns);

            case MethodInfo m when m == ILike3MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "ILIKE", match, patterns);

            default:
                return null;
            }
        }
    }
}
