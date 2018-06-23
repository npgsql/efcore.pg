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
        #region MethodInfo

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

        #endregion

        /// <summary>
        /// The version of PostgreSQL to target.
        /// </summary>
        [NotNull] readonly Version _compatibility;

        /// <summary>
        /// The query model visitor.
        /// </summary>
        [NotNull] readonly RelationalQueryModelVisitor _queryModelVisitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlSqlTranslatingExpressionVisitor"/> class.
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="queryModelVisitor"></param>
        /// <param name="compatibility"></param>
        /// <param name="targetSelectExpression"></param>
        /// <param name="topLevelPredicate"></param>
        /// <param name="inProjection"></param>
        public NpgsqlSqlTranslatingExpressionVisitor(
            [NotNull] SqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [NotNull] Version compatibility,
            [CanBeNull] SelectExpression targetSelectExpression = null,
            [CanBeNull] Expression topLevelPredicate = null,
            bool inProjection = false)
            : base(dependencies, queryModelVisitor, targetSelectExpression, topLevelPredicate, inProjection)
        {
            _queryModelVisitor = queryModelVisitor;
            _compatibility = compatibility;
        }

        #region Visits

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

            var source = queryModel.MainFromClause.FromExpression;

            // ReSharper disable AssignNullToNotNullAttribute
            switch (call.Method)
            {
            case MethodInfo m when m == Like2MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "LIKE", Visit(call.Arguments[1]), Visit(source));

            case MethodInfo m when m == Like3MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "LIKE", Visit(call.Arguments[1]), Visit(source));

            case MethodInfo m when m == ILike2MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "ILIKE", Visit(call.Arguments[1]), Visit(source));

            case MethodInfo m when m == ILike3MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "ILIKE", Visit(call.Arguments[1]), Visit(source));

            default:
                return null;
            }
            // ReSharper restore AssignNullToNotNullAttribute
        }

        #endregion
    }
}
