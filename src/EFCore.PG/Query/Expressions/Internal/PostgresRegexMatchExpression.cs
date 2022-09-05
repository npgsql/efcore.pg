using System.Text.RegularExpressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
///     Represents a PostgreSQL regular expression match expression.
/// </summary>
public class PostgresRegexMatchExpression : SqlExpression, IEquatable<PostgresRegexMatchExpression>
{
    /// <inheritdoc />
    public override Type Type => typeof(bool);

    /// <summary>
    /// The match expression.
    /// </summary>
    public virtual SqlExpression Match { get; }

    /// <summary>
    /// The pattern to match.
    /// </summary>
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
        SqlExpression match,
        SqlExpression pattern,
        RegexOptions options,
        RelationalTypeMapping? typeMapping)
        : base(typeof(bool), typeMapping)
    {
        Match = match;
        Pattern = pattern;
        Options = options;
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(Match), (SqlExpression)visitor.Visit(Pattern));

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual PostgresRegexMatchExpression Update(SqlExpression match, SqlExpression pattern)
        => match != Match || pattern != Pattern
            ? new PostgresRegexMatchExpression(match, pattern, Options, TypeMapping)
            : this;

    /// <inheritdoc />
    public virtual bool Equals(PostgresRegexMatchExpression? other)
        => ReferenceEquals(this, other) ||
            other is not null &&
            base.Equals(other) &&
            Match.Equals(other.Match) &&
            Pattern.Equals(other.Pattern) &&
            Options.Equals(other.Options);

    /// <inheritdoc />
    public override bool Equals(object? other)
        => other is PostgresRegexMatchExpression otherRegexMatch && Equals(otherRegexMatch);

    /// <inheritdoc />
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
