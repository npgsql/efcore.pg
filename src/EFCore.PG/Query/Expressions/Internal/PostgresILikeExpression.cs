using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL ILIKE expression.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class PostgresILikeExpression : SqlExpression, IEquatable<PostgresILikeExpression>
    {
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
        /// The escape character to use in <see cref="Pattern"/>.
        /// </summary>
        [CanBeNull]
        public virtual SqlExpression EscapeChar { get; }

        /// <summary>
        /// Constructs a <see cref="PostgresILikeExpression"/>.
        /// </summary>
        /// <param name="match">The expression to match.</param>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="escapeChar">The escape character to use in <paramref name="pattern"/>.</param>
        /// <exception cref="ArgumentNullException" />
        public PostgresILikeExpression(
            [NotNull] SqlExpression match,
            [NotNull] SqlExpression pattern,
            [CanBeNull] SqlExpression escapeChar,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Match = match;
            Pattern = pattern;
            EscapeChar = escapeChar;
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update(
                (SqlExpression)visitor.Visit(Match),
                (SqlExpression)visitor.Visit(Pattern),
                (SqlExpression)visitor.Visit(EscapeChar));

        public virtual PostgresILikeExpression Update(
            [NotNull] SqlExpression match,
            [NotNull] SqlExpression pattern,
            [NotNull] SqlExpression escapeChar)
            => match == Match && pattern == Pattern && escapeChar == EscapeChar
                ? this
                : new PostgresILikeExpression(match, pattern, escapeChar, TypeMapping);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is PostgresILikeExpression other && Equals(other);

        /// <inheritdoc />
        public virtual bool Equals(PostgresILikeExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               base.Equals(other) &&
               Equals(Match, other.Match) &&
               Equals(Pattern, other.Pattern) &&
               Equals(EscapeChar, other.EscapeChar);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Match, Pattern, EscapeChar);

        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Match);
            expressionPrinter.Append(" ILIKE ");
            expressionPrinter.Visit(Pattern);

            if (EscapeChar != null)
            {
                expressionPrinter.Append(" ESCAPE ");
                expressionPrinter.Visit(EscapeChar);
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"{Match} ILIKE {Pattern}{(EscapeChar == null ? "" : $" ESCAPE {EscapeChar}")}";
    }
}
