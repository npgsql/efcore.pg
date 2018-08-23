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
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL range operators.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-range.html
    /// </remarks>
    public class NpgsqlRangeTranslator : IMethodCallTranslator, IMemberTranslator
    {
        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull] readonly Version _postgresVersion;

        /// <summary>
        /// Constructs a new instance of the <see cref="NpgsqlRangeTranslator"/> class.
        /// </summary>
        /// <param name="postgresVersion">The backend version to target.</param>
        public NpgsqlRangeTranslator([CanBeNull] Version postgresVersion) => _postgresVersion = postgresVersion;

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression e)
        {
            if (e.Method.DeclaringType != typeof(NpgsqlRangeExtensions))
                return null;

            switch (e.Method.Name)
            {
            case nameof(NpgsqlRangeExtensions.Contains) when VersionAtLeast(9, 2):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "@>", typeof(bool));

            case nameof(NpgsqlRangeExtensions.ContainedBy) when VersionAtLeast(9, 2):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "<@", typeof(bool));

            case nameof(NpgsqlRangeExtensions.Overlaps) when VersionAtLeast(9, 2):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&&", typeof(bool));

            case nameof(NpgsqlRangeExtensions.IsStrictlyLeftOf) when VersionAtLeast(9, 2):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "<<", typeof(bool));

            case nameof(NpgsqlRangeExtensions.IsStrictlyRightOf) when VersionAtLeast(9, 2):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], ">>", typeof(bool));

            case nameof(NpgsqlRangeExtensions.DoesNotExtendRightOf) when VersionAtLeast(9, 2):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&<", typeof(bool));

            case nameof(NpgsqlRangeExtensions.DoesNotExtendLeftOf) when VersionAtLeast(9, 2):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&>", typeof(bool));

            case nameof(NpgsqlRangeExtensions.IsAdjacentTo) when VersionAtLeast(9, 2):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "-|-", typeof(bool));

            case nameof(NpgsqlRangeExtensions.Union) when VersionAtLeast(9, 2):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "+", e.Arguments[0].Type);

            case nameof(NpgsqlRangeExtensions.Intersect) when VersionAtLeast(9, 2):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "*", e.Arguments[0].Type);

            case nameof(NpgsqlRangeExtensions.Except):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "-", e.Arguments[0].Type);

            case nameof(NpgsqlRangeExtensions.Merge) when VersionAtLeast(9, 5):
                return new SqlFunctionExpression("range_merge", e.Type, new[] { e.Arguments[0], e.Arguments[1] });

            default:
                return null;
            }
        }

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MemberExpression e)
        {
            var type = e.Member.DeclaringType;
            if (type == null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(NpgsqlRange<>))
                return null;

            switch (e.Member.Name)
            {
            case nameof(NpgsqlRange<int>.LowerBound) when VersionAtLeast(9, 2) && !e.Type.IsNullableType():
                return
                    new SqlFunctionExpression(
                        "COALESCE",
                        e.Type,
                        new Expression[]
                        {
                            new SqlFunctionExpression("lower", e.Type, new[] { e.Expression }),
                            Expression.Default(e.Type)
                        });

            case nameof(NpgsqlRange<int>.LowerBound) when VersionAtLeast(9, 2):
                return new SqlFunctionExpression("lower", e.Type, new[] { e.Expression });

            case nameof(NpgsqlRange<int>.UpperBound) when VersionAtLeast(9, 2) && !e.Type.IsNullableType():
                return
                    new SqlFunctionExpression(
                        "COALESCE",
                        e.Type,
                        new Expression[]
                        {
                            new SqlFunctionExpression("upper", e.Type, new[] { e.Expression }),
                            Expression.Default(e.Type)
                        });

            case nameof(NpgsqlRange<int>.LowerBound) when VersionAtLeast(9, 2):
                return new SqlFunctionExpression("upper", e.Type, new[] { e.Expression });

            case nameof(NpgsqlRange<int>.IsEmpty) when VersionAtLeast(9, 2):
                return new SqlFunctionExpression("isempty", e.Type, new[] { e.Expression });

            case nameof(NpgsqlRange<int>.LowerBoundIsInclusive) when VersionAtLeast(9, 2):
                return new SqlFunctionExpression("lower_inc", e.Type, new[] { e.Expression });

            case nameof(NpgsqlRange<int>.UpperBoundIsInclusive) when VersionAtLeast(9, 2):
                return new SqlFunctionExpression("upper_inc", e.Type, new[] { e.Expression });

            case nameof(NpgsqlRange<int>.LowerBoundInfinite) when VersionAtLeast(9, 2):
                return new SqlFunctionExpression("lower_inf", e.Type, new[] { e.Expression });

            case nameof(NpgsqlRange<int>.UpperBoundInfinite) when VersionAtLeast(9, 2):
                return new SqlFunctionExpression("upper_inf", e.Type, new[] { e.Expression });

            default:
                return null;
            }
        }

        #region Helpers

        /// <summary>
        /// True if <see cref="_postgresVersion"/> is null, greater than, or equal to the specified version.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <returns>
        /// True if <see cref="_postgresVersion"/> is null, greater than, or equal to the specified version.
        /// </returns>
        bool VersionAtLeast(int major, int minor)
            => _postgresVersion is null || new Version(major, minor) <= _postgresVersion;

        #endregion
    }
}
