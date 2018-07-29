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
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionVisitors;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    public class RegexMatchExpression : Expression
    {
        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type => typeof(bool);

        /// <summary>
        /// The match expression.
        /// </summary>
        [NotNull]
        public virtual Expression Match { get; }

        /// <summary>
        /// The pattern to match.
        /// </summary>
        [NotNull]
        public virtual Expression Pattern { get; }

        /// <summary>
        /// The options for regular expression evaluation.
        /// </summary>
        public RegexOptions Options { get; }

        /// <summary>
        /// Constructs a <see cref="RegexMatchExpression"/>.
        /// </summary>
        /// <param name="match">The expression to match.</param>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="options">The options for regular expression evaluation.</param>
        /// <exception cref="ArgumentNullException" />
        public RegexMatchExpression([NotNull] Expression match, [NotNull] Expression pattern, RegexOptions options)
        {
            Match = Check.NotNull(match, nameof(match));
            Pattern = Check.NotNull(pattern, nameof(pattern));
            Options = options;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitRegexMatch(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var match = visitor.Visit(Match) ?? Match;
            var pattern = visitor.Visit(Pattern) ?? Pattern;

            return
                match != Match || pattern != Pattern
                    ? new RegexMatchExpression(match, pattern, Options)
                    : this;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Match} ~ {Pattern}";
    }
}
