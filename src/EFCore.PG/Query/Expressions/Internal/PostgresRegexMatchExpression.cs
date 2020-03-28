using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    public class PostgresRegexMatchExpression : SqlExpression, IEquatable<PostgresRegexMatchExpression>
    {
        /// <inheritdoc />
        public override Type Type => typeof(bool);

        /// <summary>
        /// The match expression.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Match { get; }

        /// <summary>
        /// The pattern to match.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Pattern { get; }

        /// <summary>
        /// The options for regular expression evaluation.
        /// </summary>
        public virtual RegexOptions Options { get; }

        /// <summary>
        /// Constructs a <see cref="PostgresRegexMatchExpression"/>.
        /// </summary>
        /// <param name="match">The expression to match.</param>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="options">The options for regular expression evaluation.</param>
        /// <param name="typeMapping">The type mapping for the expression.</param>
        public PostgresRegexMatchExpression(
            [NotNull] SqlExpression match,
            [NotNull] SqlExpression pattern,
            RegexOptions options,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Match = match;
            Pattern = pattern;
            Options = options;
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Match), (SqlExpression)visitor.Visit(Pattern));

        public virtual PostgresRegexMatchExpression Update([NotNull] SqlExpression match, [NotNull] SqlExpression pattern)
            => match != Match || pattern != Pattern
                ? new PostgresRegexMatchExpression(match, pattern, Options, TypeMapping)
                : this;

        public virtual bool Equals(PostgresRegexMatchExpression other)
            => ReferenceEquals(this, other) ||
                   other is object &&
                   base.Equals(other) &&
                   Match.Equals(other.Match) &&
                   Pattern.Equals(other.Pattern) &&
                   Options.Equals(other.Options);

        public override bool Equals(object other)
            => other is PostgresRegexMatchExpression otherRegexMatch && Equals(otherRegexMatch);

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Match, Pattern, Options);

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Match);
            expressionPrinter.Append(" ~ ");
            expressionPrinter.Visit(Pattern);
        }

        /// <inheritdoc />
        public override string ToString() => $"{Match} ~ {Pattern}";
    }
}
