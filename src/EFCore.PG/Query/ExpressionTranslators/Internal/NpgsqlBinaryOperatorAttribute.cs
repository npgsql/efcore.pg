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
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Represents information about a PostgreSQL operator.
    /// </summary>
    /// <remarks>
    /// This attribute stores metadata describing the representation of a binary operator
    /// in PostgreSQL on enum members representing the available binary operators for a PostgreSQL type.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    internal class NpgsqlBinaryOperatorAttribute : Attribute
    {
        /// <summary>
        /// The operator symbol.
        /// </summary>
        [NotNull]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// The operator represented by the method.
        /// </summary>
        [NotNull]
        public Type ReturnType { get; set; } = typeof(void);

        /// <summary>
        /// Creates a <see cref="CustomBinaryExpression"/> representing the operator.
        /// </summary>
        /// <param name="left">
        /// The left-hand expression.
        /// </param>
        /// <param name="right">
        /// The right-hand expression.
        /// </param>
        /// <returns>
        /// A <see cref="CustomBinaryExpression"/> representing the operator.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        [NotNull]
        public Expression Create([NotNull] Expression left, [NotNull] Expression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            // TODO: will the left type arguments always be valid?
            Type type = ReturnType.IsGenericType ? ReturnType.MakeGenericType(left.Type.GetGenericArguments()) : ReturnType;

            return new CustomBinaryExpression(left, right, Symbol, type);
        }
    }
}
