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

        // TODO: This should be refactored along the lines of NpgsqlCompositeMethodCallTranslator.
        /// <inheritdoc />
        [CanBeNull]
        protected override Expression VisitSubQuery(SubQueryExpression expression)
            => base.VisitSubQuery(expression) ??
               VisitConcatContainsCount(expression) ??
               VisitAnyAllLike(expression) ??
               VisitAnyAllContains(expression);

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
        /// Visits an array-based <see cref="SubQueryExpression"/> to translate a
        /// <see cref="ConcatResultOperator"/>,
        /// <see cref="ContainsResultOperator"/>, or
        /// <see cref="CountResultOperator"/>.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        protected virtual Expression VisitConcatContainsCount([NotNull] SubQueryExpression expression)
        {
            var queryModel = expression.QueryModel;
            var from = queryModel.MainFromClause.FromExpression;
            var results = queryModel.ResultOperators;

            if (!IsArrayOrList(from.Type) || results.Count != 1)
                return null;

            // BUG: This keeps a few unit tests from failing.
            // - SimpleQueryNpgsqlTest.Contains_with_local_anonymous_type_array_closure
            // - SimpleQueryNpgsqlTest.Contains_with_local_tuple_array_closure
            // - SimpleQueryNpgsqlTest.Where_navigation_contains
            if (from is ParameterExpression)
                return null;

            var array = Visit(from) ?? from;

            switch (results[0])
            {
            case ConcatResultOperator concat when IsArrayOrList(concat.Source2.Type):
                return new CustomBinaryExpression(array, Visit(concat.Source2) ?? concat.Source2, "||", array.Type);

            case ContainsResultOperator contains:
                return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "=", Visit(contains.Item) ?? contains.Item, array);

            case CountResultOperator _ when array.Type.IsArray:
                return Expression.ArrayLength(array);

            case CountResultOperator _:
                return new SqlFunctionExpression("array_length", typeof(int), new[] { array, Expression.Constant(1) });

            default:
                return null;
            }
        }

        /// <summary>
        /// Visits an array-based <see cref="SubQueryExpression"/> to translate a
        /// <see cref="AnyResultOperator"/> or <see cref="AllResultOperator"/>
        /// when the relevant predicate is a LIKE or ILIKE expression.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        protected virtual Expression VisitAnyAllLike([NotNull] SubQueryExpression expression)
        {
            var queryModel = expression.QueryModel;
            var body = queryModel.BodyClauses;
            var from = queryModel.MainFromClause.FromExpression;
            var results = queryModel.ResultOperators;

            if (!IsArrayOrList(from.Type) || results.Count != 1)
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

            var operand = Visit(call.Arguments[1]) ?? call.Arguments[1];
            var array = Visit(from) ?? from;

            // ReSharper disable AssignNullToNotNullAttribute
            switch (call.Method)
            {
            case MethodInfo m when m == Like2MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "LIKE", operand, array);

            case MethodInfo m when m == Like3MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "LIKE", operand, array);

            case MethodInfo m when m == ILike2MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "ILIKE", operand, array);

            case MethodInfo m when m == ILike3MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "ILIKE", operand, array);

            default:
                return null;
            }
        }

        /// <summary>
        /// Visits an array-based <see cref="SubQueryExpression"/> to translate a
        /// <see cref="AnyResultOperator"/> or <see cref="AllResultOperator"/>
        /// when the relevant predicate is a <see cref="SubQueryExpression"/>
        /// for which the relevant predicate is a <see cref="ContainsResultOperator"/>.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        protected virtual Expression VisitAnyAllContains([NotNull] SubQueryExpression expression)
        {
            var queryModel = expression.QueryModel;
            var body = queryModel.BodyClauses;
            var from = queryModel.MainFromClause.FromExpression;
            var results = queryModel.ResultOperators;

            if (!IsArrayOrList(from.Type) || results.Count != 1)
                return null;

            var array = Visit(from) ?? from;

            switch (results[0])
            {
            case AnyResultOperator _
                when body.Count == 1 &&
                     body[0] is WhereClause where &&
                     Visit(where.Predicate) is ArrayAnyAllExpression a &&
                     a.IsAnyEquals:
                return new CustomBinaryExpression(array, Visit(a.Array) ?? a.Array, "&&", typeof(bool));

            case AllResultOperator all
                when Visit(all.Predicate) is ArrayAnyAllExpression a &&
                     a.IsAnyEquals:
                return new CustomBinaryExpression(Visit(a.Array) ?? a.Array, array, "@>", typeof(bool));

            default:
                return null;
            }
        }

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
    }
}
