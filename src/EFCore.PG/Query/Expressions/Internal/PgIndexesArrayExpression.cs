namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
///     Represents a PostgreSQL expression that increments all elements of an array by 1.
///     Used for converting zero-based indexes to one-based indexes.
///     Generates SQL: (SELECT array_agg(x + 1) FROM unnest(arrayExpression) AS x)
/// </summary>
public class PgIndexesArrayExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="PgIndexesArrayExpression" /> class.
    /// </summary>
    /// <param name="arrayExpression">The array expression whose elements should be incremented.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public PgIndexesArrayExpression(
        SqlExpression arrayExpression,
        RelationalTypeMapping? typeMapping)
        : base(typeof(int[]), typeMapping)
    {
        Check.NotNull(arrayExpression, nameof(arrayExpression));

        ArrayExpression = arrayExpression;
    }

    /// <summary>
    ///     The array expression to increment.
    /// </summary>
    public virtual SqlExpression ArrayExpression { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Check.NotNull(visitor, nameof(visitor));

        var visitedArray = (SqlExpression)visitor.Visit(ArrayExpression);

        return visitedArray != ArrayExpression
            ? new PgIndexesArrayExpression(visitedArray, TypeMapping)
            : this;
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="arrayExpression">The array expression.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual PgIndexesArrayExpression Update(SqlExpression arrayExpression)
    {
        Check.NotNull(arrayExpression, nameof(arrayExpression));

        return arrayExpression == ArrayExpression
            ? this
            : new PgIndexesArrayExpression(arrayExpression, TypeMapping);
    }

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(PgIndexesArrayExpression).GetConstructor(
                [typeof(SqlExpression), typeof(RelationalTypeMapping)])!,
            ArrayExpression.Quote(),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        Check.NotNull(expressionPrinter, nameof(expressionPrinter));

        // Generate: (SELECT array_agg(x + 1) FROM unnest(arrayExpression) AS x)
        expressionPrinter.Append("(SELECT array_agg(x + 1) FROM unnest(");
        expressionPrinter.Visit(ArrayExpression);
        expressionPrinter.Append(") AS x)");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is not null
            && (ReferenceEquals(this, obj)
                || obj is PgIndexesArrayExpression other
                && Equals(other));

    private bool Equals(PgIndexesArrayExpression other)
        => base.Equals(other)
            && ArrayExpression.Equals(other.ArrayExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), ArrayExpression);
}
