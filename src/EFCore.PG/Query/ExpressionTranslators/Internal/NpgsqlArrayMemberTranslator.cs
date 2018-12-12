using System;
using System.Collections;
using System.Collections.Generic;
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
        public Expression Translate(MemberExpression e)
            => ArrayInstanceHandler(e) ??
               ListInstanceHandler(e);

        #region Handlers

        [CanBeNull]
        Expression ArrayInstanceHandler([NotNull] MemberExpression e)
        {
            var instance = e.Expression;

            if (instance == null || !instance.Type.IsArray || instance.Type.GetArrayRank() != 1)
                return null;

            switch (e.Member.Name)
            {
            case nameof(Array.Length) when VersionAtLeast(8, 4):
                return Expression.Coalesce(
                    new SqlFunctionExpression(
                        "array_length",
                        typeof(int?),
                        new[] { instance, Expression.Constant(1) }),
                    Expression.Constant(0));

            case nameof(Array.Rank) when VersionAtLeast(8, 4):
                return Expression.Coalesce(
                    new SqlFunctionExpression(
                        "array_ndims",
                        typeof(int?),
                        new[] { instance }),
                    Expression.Constant(1));

            default:
                return null;
            }
        }

        [CanBeNull]
        Expression ListInstanceHandler([NotNull] MemberExpression e)
        {
            var instance = e.Expression;

            if (instance is null || !instance.Type.IsGenericType || instance.Type.GetGenericTypeDefinition() != typeof(List<>))
                return null;

            switch (e.Member.Name)
            {
            case nameof(IList.Count) when VersionAtLeast(8, 4):
                return Expression.Coalesce(
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
