namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
///     A SQL expression that represents a slicing into a PostgreSQL array (e.g. array[2:3]).
/// </summary>
/// <remarks>
///     <see href="https://www.postgresql.org/docs/current/arrays.html#ARRAYS-ACCESSING" />.
/// </remarks>
public class PgArraySliceExpression : SqlExpression, IEquatable<PgArraySliceExpression>
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     The array being sliced.
    /// </summary>
    public virtual SqlExpression Array { get; }

    /// <summary>
    ///     The lower bound of the slice.
    /// </summary>
    public virtual SqlExpression? LowerBound { get; }

    /// <summary>
    ///     The upper bound of the slice.
    /// </summary>
    public virtual SqlExpression? UpperBound { get; }

    /// <summary>
    ///     Whether the expression is nullable.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="PgArraySliceExpression" /> class.
    /// </summary>
    /// <param name="array">The array tp slice into.</param>
    /// <param name="lowerBound">The lower bound of the slice.</param>
    /// <param name="upperBound">The upper bound of the slice.</param>
    /// <param name="nullable">Whether the expression is nullable.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public PgArraySliceExpression(
        SqlExpression array,
        SqlExpression? lowerBound,
        SqlExpression? upperBound,
        bool nullable,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type.UnwrapNullableType(), typeMapping)
    {
        Check.NotNull(array, nameof(array));

        if (lowerBound is null && upperBound is null)
        {
            throw new ArgumentException("At least one of lowerBound or upperBound must be provided");
        }

        Array = array;
        LowerBound = lowerBound;
        UpperBound = upperBound;
        IsNullable = nullable;
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="array">The <see cref="Array" /> property of the result.</param>
    /// <param name="lowerBound">The lower bound of the slice.</param>
    /// <param name="upperBound">The upper bound of the slice.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual PgArraySliceExpression Update(SqlExpression array, SqlExpression? lowerBound, SqlExpression? upperBound)
        => array == Array && lowerBound == LowerBound && upperBound == UpperBound
            ? this
            : new PgArraySliceExpression(array, lowerBound, upperBound, IsNullable, Type, TypeMapping);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(PgArraySliceExpression).GetConstructor(
                [typeof(SqlExpression), typeof(SqlExpression), typeof(SqlExpression), typeof(bool), typeof(Type), typeof(RelationalTypeMapping)])!,
            Array.Quote(),
            RelationalExpressionQuotingUtilities.QuoteOrNull(LowerBound),
            RelationalExpressionQuotingUtilities.QuoteOrNull(UpperBound),
            Constant(IsNullable),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update(
            (SqlExpression)visitor.Visit(Array),
            (SqlExpression?)visitor.Visit(LowerBound),
            (SqlExpression?)visitor.Visit(UpperBound));

    /// <inheritdoc />
    public virtual bool Equals(PgArraySliceExpression? other)
        => ReferenceEquals(this, other)
            || other is not null
            && base.Equals(other)
            && Array.Equals(other.Array)
            && (LowerBound is null ? other.LowerBound is null : LowerBound.Equals(other.LowerBound))
            && (UpperBound is null ? other.UpperBound is null : UpperBound.Equals(other.UpperBound))
            && IsNullable == other.IsNullable;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is PgArraySliceExpression e && Equals(e);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Array, LowerBound, UpperBound);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Array);
        expressionPrinter.Append("[");
        expressionPrinter.Visit(LowerBound);
        expressionPrinter.Append(":");
        expressionPrinter.Visit(UpperBound);
        expressionPrinter.Append("]");
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{Array}[{LowerBound}:{UpperBound}]";
}
