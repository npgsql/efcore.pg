namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
/// Represents a PostgreSQL array ALL expression.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/functions-comparisons.html
/// </remarks>
public class PostgresAllExpression : SqlExpression, IEquatable<PostgresAllExpression>
{
    /// <inheritdoc />
    public override Type Type => typeof(bool);

    /// <summary>
    /// The value to test against the <see cref="Array"/>.
    /// </summary>
    public virtual SqlExpression Item { get; }

    /// <summary>
    /// The array of values or patterns to test for the <see cref="Item"/>.
    /// </summary>
    public virtual SqlExpression Array { get; }

    /// <summary>
    /// The operator.
    /// </summary>
    public virtual PostgresAllOperatorType OperatorType { get; }

    /// <summary>
    /// Constructs a <see cref="PostgresAllExpression"/>.
    /// </summary>
    /// <param name="operatorType">The operator symbol to the array expression.</param>
    /// <param name="item">The value to find.</param>
    /// <param name="array">The array to search.</param>
    /// <param name="typeMapping">The type mapping for the expression.</param>
    public PostgresAllExpression(
        SqlExpression item,
        SqlExpression array,
        PostgresAllOperatorType operatorType,
        RelationalTypeMapping? typeMapping)
        : base(typeof(bool), typeMapping)
    {
        if (!(array.Type.IsArrayOrGenericList() || array is SqlConstantExpression { Value: null }))
        {
            throw new ArgumentException("Array expression must be of type array or List<>", nameof(array));
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
    public virtual PostgresAllExpression Update(SqlExpression item, SqlExpression array)
        => item != Item || array != Array
            ? new PostgresAllExpression(item, array, OperatorType, TypeMapping)
            : this;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(Item), (SqlExpression)visitor.Visit(Array));

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PostgresAllExpression e && Equals(e);

    /// <inheritdoc />
    public virtual bool Equals(PostgresAllExpression? other)
        => ReferenceEquals(this, other) ||
            other is not null &&
            base.Equals(other) &&
            Item.Equals(other.Item) &&
            Array.Equals(other.Array) &&
            OperatorType.Equals(other.OperatorType);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Item, Array, OperatorType);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Item);
        expressionPrinter
            .Append(" ")
            .Append(OperatorType switch
            {
                PostgresAllOperatorType.Like     => "LIKE",
                PostgresAllOperatorType.ILike    => "ILIKE",

                _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {OperatorType}")
            })
            .Append(" ALL(");
        expressionPrinter.Visit(Array);
        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override string ToString() => $"{Item} {OperatorType} ALL({Array})";
}

/// <summary>
/// Determines the operator type for a <see cref="PostgresAllExpression" />.
/// </summary>
public enum PostgresAllOperatorType
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
