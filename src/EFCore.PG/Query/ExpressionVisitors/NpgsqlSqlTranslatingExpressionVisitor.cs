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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
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
        #region MethodInfoFields

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

        /// <inheritdoc />
        public NpgsqlSqlTranslatingExpressionVisitor(
            [NotNull] SqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] SelectExpression targetSelectExpression = null,
            [CanBeNull] Expression topLevelPredicate = null,
            bool inProjection = false)
            : base(dependencies, queryModelVisitor, targetSelectExpression, topLevelPredicate, inProjection) {}

        #region Overrides

        /// <inheritdoc />
        protected override Expression VisitBinary(BinaryExpression expression)
            => expression.NodeType is ExpressionType.ArrayIndex
                ? Expression.MakeIndex(
                    Visit(expression.Left) ?? expression.Left,
                    indexer: null,
                    new[] { Visit(expression.Right) ?? expression.Right, })
                : base.VisitBinary(expression);

        /// <inheritdoc />
        [CanBeNull]
        protected override Expression VisitExtension(Expression expression)
        {
            switch (expression)
            {
            case SqlFunctionExpression e:
                return
                    new SqlFunctionExpression(
                        e.FunctionName,
                        e.Type,
                        e.Schema,
                        e.Arguments.Select(x => Visit(x) ?? x));

            case PgFunctionExpression e:
                return
                    new PgFunctionExpression(
                        e.Instance,
                        e.FunctionName,
                        e.Schema,
                        e.Type,
                        e.PositionalArguments.Select(x => Visit(x) ?? x),
                        e.NamedArguments.ToDictionary(x => x.Key, x => Visit(x.Value) ?? x.Value));

            case CustomBinaryExpression e:
                return
                    new CustomBinaryExpression(
                        Visit(e.Left) ?? e.Left,
                        Visit(e.Right) ?? e.Right,
                        e.Operator,
                        e.Type);

            case CustomUnaryExpression e:
                return
                    new CustomUnaryExpression(
                        Visit(e.Operand) ?? e.Operand,
                        e.Operator,
                        e.Type,
                        e.Postfix);

            default:
                return base.VisitExtension(expression);
            }
        }

        /// <inheritdoc />
        [CanBeNull]
        protected override Expression VisitSubQuery(SubQueryExpression expression)
            => base.VisitSubQuery(expression) ?? VisitArraySubQuery(expression);

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression expression)
            => Visit(expression.Operand) is Expression operand
                ? Expression.MakeUnary(expression.NodeType, operand, expression.Type)
                : base.VisitUnary(expression);

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
            var queryModel = expression.QueryModel;
            var from = queryModel.MainFromClause.FromExpression;
            var body = queryModel.BodyClauses;
            var results = queryModel.ResultOperators;

            // TODO: what causes the from expression to not be visitable?
            // Only handle subqueries when the from expression is visitable.
            if (!(Visit(from) is Expression array))
                return null;

            // Only handle types mapped to PostgreSQL arrays.
            if (!IsArrayOrList(array.Type))
                return null;

            // TODO: when is there more than one result operator?
            // Only handle singular result operators.
            if (results.Count != 1)
                return null;

            switch (results[0])
            {
            case AnyResultOperator _:
                return VisitArrayAny(array, body);

            case AllResultOperator allResultOperator:
                return VisitArrayAll(array, allResultOperator);

            case ContainsResultOperator contains:
                return new ArrayAnyAllExpression(ArrayComparisonType.ANY, "=", Visit(contains.Item) ?? contains.Item, array);

            default:
                return null;
            }
        }

        /// <summary>
        /// Visits an array-based ANY comparison: {operand} {operator} ANY ({array}).
        /// </summary>
        /// <param name="array">The array expression.</param>
        /// <param name="body">The body clauses.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        protected virtual Expression VisitArrayAny(Expression array, [NotNull] ObservableCollection<IBodyClause> body)
        {
            var predicate =
                body.Count == 1 &&
                body[0] is WhereClause whereClause
                    ? whereClause.Predicate
                    : null;

            if (predicate is null)
                return null;

            return
                VisitArrayLike(array, predicate, ArrayComparisonType.ANY) ??
                VisitArrayContains(array, predicate, ArrayComparisonType.ANY);
        }

        /// <summary>
        /// Visits an array-based ALL comparison: {operand} {operator} ALL ({array}).
        /// </summary>
        /// <param name="array">The array expression.</param>
        /// <param name="allResultOperator">The result operator.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        protected virtual Expression VisitArrayAll([NotNull] Expression array, [NotNull] AllResultOperator allResultOperator)
            => VisitArrayLike(array, allResultOperator.Predicate, ArrayComparisonType.ALL) ??
               VisitArrayContains(array, allResultOperator.Predicate, ArrayComparisonType.ALL);

        /// <summary>
        /// Visits an array-based comparison for an LIKE or ILIKE expression: {operand} {LIKE|ILIKE} {ANY|ALL} ({array}).
        /// </summary>
        /// <param name="array">The array expression.</param>
        /// <param name="predicate">The method call expression.</param>
        /// <param name="comparisonType">The array comparison type.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        protected virtual Expression VisitArrayLike([NotNull] Expression array, [NotNull] Expression predicate, ArrayComparisonType comparisonType)
        {
            if (!(predicate is MethodCallExpression call))
                return null;

            var operand = Visit(call.Arguments[1]) ?? call.Arguments[1];

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
        /// Visits an array-based comparison for a containment expression: {operand} = {ANY|ALL} ({array}).
        /// </summary>
        /// <param name="array">The array expression.</param>
        /// <param name="predicate">The method call expression.</param>
        /// <param name="comparisonType">The array comparison type.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        protected virtual Expression VisitArrayContains([NotNull] Expression array, [NotNull] Expression predicate, ArrayComparisonType comparisonType)
        {
            if (!(Visit(predicate) is ArrayAnyAllExpression expression) || !expression.IsContainsExpression)
                return null;

            var inner = Visit(expression.Array) ?? expression.Array;

            return new CustomBinaryExpression(array, inner, comparisonType == ArrayComparisonType.ALL ? "<@" : "&&", typeof(bool));
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
        static bool IsArrayOrList([NotNull] Type type) => type.IsArray || type.IsGenericType && typeof(List<>) == type.GetGenericTypeDefinition();

        #endregion
    }
}
