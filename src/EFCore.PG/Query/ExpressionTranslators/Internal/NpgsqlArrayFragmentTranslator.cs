using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for array fragments.
    /// </summary>
    public class NpgsqlArrayFragmentTranslator : IExpressionFragmentTranslator
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
        [CanBeNull]
        public Expression Translate(Expression expression)
        {
            if (!(expression is SubQueryExpression subQuery))
                return null;

            var model = subQuery.QueryModel;

            if (!IsArrayOrList(model.MainFromClause.FromExpression.Type))
                return null;

            return
                AllResult(model) ??
                AnyResult(model) ??
                ConcatResult(model) ??
                CountResult(model);
        }

        #region SubQueries

        /// <summary>
        /// Visits an array-based ALL expression.
        /// </summary>
        /// <param name="model">The query model to visit.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        static Expression AllResult([NotNull] QueryModel model)
        {
            Expression array = model.MainFromClause.FromExpression;

            // TODO: when is there more than one result operator?
            // Only handle singular result operators.
            if (model.ResultOperators.Count == 1 && model.ResultOperators[0] is AllResultOperator all)
                return ConstructArrayLike(array, all.Predicate, ArrayComparisonType.ALL);

            return null;
        }

        /// <summary>
        /// Visits an array-based ANY expression.
        /// </summary>
        /// <param name="model">The query model to visit.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        static Expression AnyResult([NotNull] QueryModel model)
        {
            Expression array = model.MainFromClause.FromExpression;

            // TODO: when is there more than one result operator?
            // Only handle singular result operators.
            if (model.ResultOperators.Count != 1 || !(model.ResultOperators[0] is AnyResultOperator _))
                return null;

            if (model.BodyClauses.Count == 1 && model.BodyClauses[0] is WhereClause where)
                return ConstructArrayLike(array, where.Predicate, ArrayComparisonType.ANY);

            return null;
        }

        /// <summary>
        /// Visits an array-based concatenation expression: {array|value} || {array|value}.
        /// </summary>
        /// <param name="model">The query model to visit.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        static Expression ConcatResult([NotNull] QueryModel model)
        {
            if (model.BodyClauses.Count != 0)
                return null;

            if (model.ResultOperators.Count != 1)
                return null;

            if (!(model.ResultOperators[0] is ConcatResultOperator concat))
                return null;

            Expression from = model.MainFromClause.FromExpression;

            Expression other = concat.Source2;

            if (!IsArrayOrList(other.Type))
                return null;

            return new CustomBinaryExpression(from, other, "||", from.Type);
        }

        /// <summary>
        /// Visits an array-based count expression: {array}.Length, {list}.Count, {array|list}.Count(), {array|list}.Count({predicate}).
        /// </summary>
        /// <param name="model">The query model to visit.</param>
        /// <returns>
        /// An expression or null.
        /// </returns>
        [CanBeNull]
        static Expression CountResult([NotNull] QueryModel model)
        {
            // TODO: handle count operation with predicate.
            if (model.BodyClauses.Count != 0)
                return null;

            if (model.ResultOperators.Count != 1)
                return null;

            if (!(model.ResultOperators[0] is CountResultOperator _))
                return null;

            Expression from = model.MainFromClause.FromExpression;

            return
                from.Type.IsArray
                    ? Expression.MakeMemberAccess(from, from.Type.GetRuntimeProperty(nameof(Array.Length)))
                    : Expression.MakeMemberAccess(from, from.Type.GetRuntimeProperty(nameof(IList.Count)));
        }

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
        static Expression ConstructArrayLike([NotNull] Expression array, [CanBeNull] Expression predicate, ArrayComparisonType comparisonType)
        {
            if (!(predicate is MethodCallExpression call))
                return null;

            if (call.Arguments.Count < 2)
                return null;

            Expression operand = call.Arguments[1];
            Expression collection = array;

            switch (call.Method)
            {
            case MethodInfo m when m == Like2MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "LIKE", operand, collection);

            case MethodInfo m when m == Like3MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "LIKE", operand, collection);

            case MethodInfo m when m == ILike2MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "ILIKE", operand, collection);

            case MethodInfo m when m == ILike3MethodInfo:
                return new ArrayAnyAllExpression(comparisonType, "ILIKE", operand, collection);

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
