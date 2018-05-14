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

using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators
{
    /// <summary>
    /// Describes the operator type of a range expression.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-range.html
    /// </remarks>
    [PublicAPI]
    public enum NpgsqlRangeOperatorType
    {
        /// <summary>
        /// No operator specified.
        /// </summary>
        [NpgsqlBinaryOperator] None,

        /// <summary>
        /// The = operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "=", ReturnType = typeof(bool))] Equal,

        /// <summary>
        /// The &lt;> operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "<>", ReturnType = typeof(bool))] NotEqual,

        /// <summary>
        /// The &lt; operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "<", ReturnType = typeof(bool))] LessThan,

        /// <summary>
        /// The > operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = ">", ReturnType = typeof(bool))] GreaterThan,

        /// <summary>
        /// The &lt;= operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "<=", ReturnType = typeof(bool))] LessThanOrEqual,

        /// <summary>
        /// The >= operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = ">=", ReturnType = typeof(bool))] GreaterThanOrEqual,

        /// <summary>
        /// The @> operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "@>", ReturnType = typeof(bool))] Contains,

        /// <summary>
        /// The &lt;@ operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "<@", ReturnType = typeof(bool))] ContainedBy,

        /// <summary>
        /// The && operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "&&", ReturnType = typeof(bool))] Overlaps,

        /// <summary>
        /// The &lt;&lt; operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "<<", ReturnType = typeof(bool))] IsStrictlyLeftOf,

        /// <summary>
        /// The >> operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = ">>", ReturnType = typeof(bool))] IsStrictlyRightOf,

        /// <summary>
        /// The &amp;&lt; operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "&<", ReturnType = typeof(bool))] DoesNotExtendRightOf,

        /// <summary>
        /// The &amp;&gt; operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "&>", ReturnType = typeof(bool))] DoesNotExtendLeftOf,

        /// <summary>
        /// The -|- operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "-|-", ReturnType = typeof(bool))] IsAdjacentTo,

        /// <summary>
        /// The + operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "+", ReturnType = typeof(NpgsqlRange<>))] Union,

        /// <summary>
        /// The * operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "*", ReturnType = typeof(NpgsqlRange<>))] Intersection,

        /// <summary>
        /// The - operator.
        /// </summary>
        [NpgsqlBinaryOperator(Symbol = "-", ReturnType = typeof(NpgsqlRange<>))] Difference
    }
}
