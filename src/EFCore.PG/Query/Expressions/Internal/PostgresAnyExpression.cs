namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
/// Represents a PostgreSQL array ANY expression.
/// </summary>
/// <example>
/// 1 = ANY ('{0,1,2}'), 'cat' LIKE ANY ('{a%,b%,c%}')
/// </example>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/functions-comparisons.html
/// </remarks>
public class PostgresAnyExpression : SqlExpression, IEquatable<PostgresAnyExpression>
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
    public virtual PostgresAnyOperatorType OperatorType { get; }

    /// <summary>
    /// Constructs a <see cref="PostgresAnyExpression"/>.
    /// </summary>
    /// <param name="operatorType">The operator symbol to the array expression.</param>
    /// <param name="item">The value to find.</param>
    /// <param name="array">The array to search.</param>
    /// <param name="typeMapping">The type mapping for the expression.</param>
    public PostgresAnyExpression(
        SqlExpression item,
        SqlExpression array,
        PostgresAnyOperatorType operatorType,
        RelationalTypeMapping? typeMapping)
        : base(typeof(bool), typeMapping)
    {
        if (!(array is SqlConstantExpression { Value: null }))
        {
            if (!array.Type.IsArrayOrGenericList())
            {
                throw new ArgumentException("Array expression must be of type array or List<>", nameof(array));
            }

            if (array is SqlConstantExpression && operatorType == PostgresAnyOperatorType.Equal)
            {
                throw new ArgumentException($"Use {nameof(InExpression)} for equality against constant arrays", nameof(array));
            }
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
    public virtual PostgresAnyExpression Update(SqlExpression item, SqlExpression array)
        => item != Item || array != Array
            ? new PostgresAnyExpression(item, array, OperatorType, TypeMapping)
            : this;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(Item), (SqlExpression)visitor.Visit(Array));

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PostgresAnyExpression e && Equals(e);

    /// <inheritdoc />
    public virtual bool Equals(PostgresAnyExpression? other)
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
                PostgresAnyOperatorType.Equal    => "=",
                PostgresAnyOperatorType.Like     => "LIKE",
                PostgresAnyOperatorType.ILike    => "ILIKE",

                _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {OperatorType}")
            })
            .Append(" ANY(");
        expressionPrinter.Visit(Array);
        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override string ToString() => $"{Item} {OperatorType} ANY({Array})";
}

/// <summary>
/// Determines the operator type for a <see cref="PostgresAnyExpression" />.
/// </summary>
public enum PostgresAnyOperatorType
{
    /// <summary>
    ///     Represents a PostgreSQL = ANY operator.
    /// </summary>
    Equal,
    
    /// <summary>
    ///     Represents a PostgreSQL LIKE ANY operator.
    /// </summary>
    Like,
    
    /// <summary>
    ///     Represents a PostgreSQL ILIKE ANY operator.
    /// </summary>
    ILike,
}
