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
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL range operators.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-range.html
    /// </remarks>
    public class NpgsqlRangeTranslator : IMethodCallTranslator
    {
        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression methodCallExpression) =>
            TryTranslateOperator(methodCallExpression);

        /// <summary>
        /// Attempts to translate the <see cref="MethodCallExpression"/> as a PostgreSQL range operator.
        /// </summary>
        /// <param name="expression">The <see cref="MethodCallExpression"/> to be translated.</param>
        /// <returns>
        /// The expression if successful; otherwise, null.
        /// </returns>
        static Expression TryTranslateOperator([NotNull] MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
            case nameof(NpgsqlRangeExtensions.Contains):
                return MakeBinaryExpression(expression, "@>", typeof(bool));

            case nameof(NpgsqlRangeExtensions.ContainedBy):
                return MakeBinaryExpression(expression, "<@", typeof(bool));

            case nameof(NpgsqlRangeExtensions.Overlaps):
                return MakeBinaryExpression(expression, "&&", typeof(bool));

            case nameof(NpgsqlRangeExtensions.IsStrictlyLeftOf):
                return MakeBinaryExpression(expression, "<<", typeof(bool));

            case nameof(NpgsqlRangeExtensions.IsStrictlyRightOf):
                return MakeBinaryExpression(expression, ">>", typeof(bool));

            case nameof(NpgsqlRangeExtensions.DoesNotExtendRightOf):
                return MakeBinaryExpression(expression, "&<", typeof(bool));

            case nameof(NpgsqlRangeExtensions.DoesNotExtendLeftOf):
                return MakeBinaryExpression(expression, "&>", typeof(bool));

            case nameof(NpgsqlRangeExtensions.IsAdjacentTo):
                return MakeBinaryExpression(expression, "-|-", typeof(bool));

            case nameof(NpgsqlRangeExtensions.Union):
                return MakeBinaryExpression(expression, "+", expression.Arguments[0].Type);

            case nameof(NpgsqlRangeExtensions.Intersect):
                return MakeBinaryExpression(expression, "*", expression.Arguments[0].Type);

            case nameof(NpgsqlRangeExtensions.Except):
                return MakeBinaryExpression(expression, "-", expression.Arguments[0].Type);

            default:
                return null;
            }
        }

        /// <summary>
        /// Constructs a <see cref="CustomBinaryExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="MethodCallExpression"/> containing two parameters.</param>
        /// <param name="symbol">The symbolic operator for PostgreSQL.</param>
        /// <param name="returnType">The return type of the operator.</param>
        /// <returns>
        /// A <see cref="CustomBinaryExpression"/>.
        /// </returns>
        static Expression MakeBinaryExpression([NotNull] MethodCallExpression expression, [NotNull] string symbol, [NotNull] Type returnType) =>
            new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], symbol, returnType);
    }
}
