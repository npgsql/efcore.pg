namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
/// Represents a PostgreSQL ILIKE expression.
/// </summary>
// ReSharper disable once InconsistentNaming
public class PostgresILikeExpression : SqlExpression, IEquatable<PostgresILikeExpression>
{
    /// <summary>
    /// The match expression.
    /// </summary>
    public virtual SqlExpression Match { get; }

    /// <summary>
    /// The pattern to match.
    /// </summary>
    public virtual SqlExpression Pattern { get; }

    /// <summary>
    /// The escape character to use in <see cref="Pattern"/>.
    /// </summary>
    public virtual SqlExpression? EscapeChar { get; }

    /// <summary>
    /// Constructs a <see cref="PostgresILikeExpression"/>.
    /// </summary>
    /// <param name="match">The expression to match.</param>
    /// <param name="pattern">The pattern to match.</param>
    /// <param name="escapeChar">The escape character to use in <paramref name="pattern"/>.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping"/> associated with the expression.</param>
    /// <exception cref="ArgumentNullException" />
    public PostgresILikeExpression(
        SqlExpression match,
        SqlExpression pattern,
        SqlExpression? escapeChar,
        RelationalTypeMapping? typeMapping)
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
            EscapeChar is null ? null : (SqlExpression)visitor.Visit(EscapeChar));

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual PostgresILikeExpression Update(
        SqlExpression match,
        SqlExpression pattern,
        SqlExpression? escapeChar)
        => match == Match && pattern == Pattern && escapeChar == EscapeChar
            ? this
            : new PostgresILikeExpression(match, pattern, escapeChar, TypeMapping);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PostgresILikeExpression other && Equals(other);

    /// <inheritdoc />
    public virtual bool Equals(PostgresILikeExpression? other)
        => ReferenceEquals(this, other) ||
            other is not null &&
            base.Equals(other) &&
            Equals(Match, other.Match) &&
            Equals(Pattern, other.Pattern) &&
            Equals(EscapeChar, other.EscapeChar);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Match, Pattern, EscapeChar);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Match);
        expressionPrinter.Append(" ILIKE ");
        expressionPrinter.Visit(Pattern);

        if (EscapeChar is not null)
        {
            expressionPrinter.Append(" ESCAPE ");
            expressionPrinter.Visit(EscapeChar);
        }
    }

    /// <inheritdoc />
    public override string ToString() => $"{Match} ILIKE {Pattern}{(EscapeChar is null ? "" : $" ESCAPE {EscapeChar}")}";
}
