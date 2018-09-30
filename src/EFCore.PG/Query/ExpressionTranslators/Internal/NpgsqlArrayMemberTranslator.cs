using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL array operators mapped to generic array members.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-array.html
    /// </remarks>
    public class NpgsqlArrayMemberTranslator : IMemberTranslator
    {
        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull] readonly Version _postgresVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlArrayMemberTranslator"/> class.
        /// </summary>
        /// <param name="postgresVersion">The backend version to target.</param>
        public NpgsqlArrayMemberTranslator([CanBeNull] Version postgresVersion) => _postgresVersion = postgresVersion;

        /// <inheritdoc />
        public Expression Translate(MemberExpression expression)
            => ArrayInstanceHandler(expression) ?? ListInstanceHandler(expression);

        #region Handlers

        /// <summary>
        /// Translates instance members defined on <see cref="Array"/>.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <returns>
        /// A translated expression or null if no translation is supported.
        /// </returns>
        [CanBeNull]
        Expression ArrayInstanceHandler([NotNull] MemberExpression expression)
        {
            var instance = expression.Expression;

            if (instance is null || !instance.Type.IsArray)
                return null;

            switch (expression.Member.Name)
            {
            case nameof(Array.Length) when VersionAtLeast(9, 4):
                return new SqlFunctionExpression("cardinality", typeof(int), new[] { instance });

            case nameof(Array.Length) when VersionAtLeast(8, 4) && instance.Type.GetArrayRank() == 1:
                return
                    Expression.Coalesce(
                        new SqlFunctionExpression(
                            "array_length",
                            typeof(int?),
                            new[] { instance, Expression.Constant(1) }),
                        Expression.Constant(0));

            case nameof(Array.Length) when VersionAtLeast(8, 4):
                return
                    Enumerable.Range(1, instance.Type.GetArrayRank())
                              .Select(x =>
                                  Expression.Coalesce(
                                      new SqlFunctionExpression(
                                          "array_length",
                                          typeof(int?),
                                          new[] { instance, Expression.Constant(x) }),
                                      Expression.Constant(0)))
                              .Cast<Expression>()
                              .Aggregate(Expression.Multiply);

            case nameof(Array.Rank) when VersionAtLeast(8, 4):
                return
                    Expression.Coalesce(
                        new SqlFunctionExpression(
                            "array_ndims",
                            typeof(int?),
                            new[] { instance }),
                        Expression.Constant(1));

            default:
                return null;
            }
        }

        /// <summary>
        /// Translates instance members defined on <see cref="List{T}"/>.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <returns>
        /// A translated expression or null if no translation is supported.
        /// </returns>
        [CanBeNull]
        Expression ListInstanceHandler([NotNull] MemberExpression expression)
        {
            var instance = expression.Expression;

            if (instance is null || !instance.Type.IsGenericType || instance.Type.GetGenericTypeDefinition() != typeof(List<>))
                return null;

            switch (expression.Member.Name)
            {
            case nameof(IList.Count) when VersionAtLeast(8, 4):
                return
                    Expression.Coalesce(
                        new SqlFunctionExpression(
                            "array_length",
                            typeof(int?),
                            new Expression[] { instance, Expression.Constant(1) }),
                        Expression.Constant(0));

            default:
                return null;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// True if <see cref="_postgresVersion"/> is null, greater than, or equal to the specified version.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <returns>
        /// True if <see cref="_postgresVersion"/> is null, greater than, or equal to the specified version.
        /// </returns>
        bool VersionAtLeast(int major, int minor) => _postgresVersion is null || new Version(major, minor) <= _postgresVersion;

        #endregion
    }
}
