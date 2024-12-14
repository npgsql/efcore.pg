namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
///     Represents a PostgreSQL array ALL expression.
/// </summary>
/// <remarks>
///     See https://www.postgresql.org/docs/current/static/functions-comparisons.html
/// </remarks>
public class PgAllExpression : SqlExpression, IEquatable<PgAllExpression>
{
    private static ConstructorInfo? _quotingConstructor;

    /// <inheritdoc />
    public override Type Type
        => typeof(bool);

    /// <summary>
    ///     The value to test against the <see cref="Array" />.
    /// </summary>
    public virtual SqlExpression Item { get; }

    /// <summary>
    ///     The array of values or patterns to test for the <see cref="Item" />.
    /// </summary>
    public virtual SqlExpression Array { get; }

    /// <summary>
    ///     The operator.
    /// </summary>
    public virtual PgAllOperatorType OperatorType { get; }

    /// <summary>
    ///     Constructs a <see cref="PgAllExpression" />.
    /// </summary>
    /// <param name="operatorType">The operator symbol to the array expression.</param>
    /// <param name="item">The value to find.</param>
    /// <param name="array">The array to search.</param>
    /// <param name="typeMapping">The type mapping for the expression.</param>
    public PgAllExpression(
        SqlExpression item,
        SqlExpression array,
        PgAllOperatorType operatorType,
        RelationalTypeMapping? typeMapping)
        : base(typeof(bool), typeMapping)
    {
        if (array.Type.TryGetElementType(typeof(IEnumerable<>)) is null)
        {
            throw new ArgumentException("Array expression must be an IEnumerable", nameof(array));
        }

        Item = item;
        Array = array;
        OperatorType = operatorType;
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="item">The <see cref="Item" /> property of the result.</param>
    /// <param name="array">The <see cref="Array" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual PgAllExpression Update(SqlExpression item, SqlExpression array)
        => item != Item || array != Array
            ? new PgAllExpression(item, array, OperatorType, TypeMapping)
            : this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(PgAllExpression).GetConstructor(
                [typeof(SqlExpression), typeof(SqlExpression), typeof(PgAllOperatorType), typeof(RelationalTypeMapping)])!,
            Item.Quote(),
            Array.Quote(),
            Constant(OperatorType),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(Item), (SqlExpression)visitor.Visit(Array));

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is PgAllExpression e && Equals(e);

    /// <inheritdoc />
    public virtual bool Equals(PgAllExpression? other)
        => ReferenceEquals(this, other)
            || other is not null
            && base.Equals(other)
            && Item.Equals(other.Item)
            && Array.Equals(other.Array)
            && OperatorType.Equals(other.OperatorType);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Item, Array, OperatorType);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Item);
        expressionPrinter
            .Append(" ")
            .Append(
                OperatorType switch
                {
                    PgAllOperatorType.Like => "LIKE",
                    PgAllOperatorType.ILike => "ILIKE",

                    _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {OperatorType}")
                })
            .Append(" ALL(");
        expressionPrinter.Visit(Array);
        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{Item} {OperatorType} ALL({Array})";
}

/// <summary>
///     Determines the operator type for a <see cref="PgAllExpression" />.
/// </summary>
public enum PgAllOperatorType
{
    /// <summary>
    ///     Represents a PostgreSQL LIKE ALL operator.
    /// </summary>
    Like,

    /// <summary>
    ///     Represents a PostgreSQL ILIKE ALL operator.
    /// </summary>
    ILike,
}
