namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
/// An SQL expression that represents an indexing into a PostgreSQL array.
/// </summary>
/// <remarks>
/// <see cref="SqlBinaryExpression"/> specifically disallows having an <see cref="SqlBinaryExpression.OperatorType"/>
/// of value <see cref="ExpressionType.ArrayIndex"/> as arrays are a PostgreSQL-only feature.
/// </remarks>
public class PostgresArrayIndexExpression : SqlExpression, IEquatable<PostgresArrayIndexExpression>
{
    /// <summary>
    /// The array being indexed.
    /// </summary>
    public virtual SqlExpression Array { get; }

    /// <summary>
    /// The index in the array.
    /// </summary>
    public virtual SqlExpression Index { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="PostgresArrayIndexExpression" /> class.
    /// </summary>
    /// <param name="array">The array tp index into.</param>
    /// <param name="index">An position in the array to index into.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public PostgresArrayIndexExpression(
        SqlExpression array,
        SqlExpression index,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type.UnwrapNullableType(), typeMapping)
    {
        Check.NotNull(array, nameof(array));
        Check.NotNull(index, nameof(index));

        if (!array.Type.TryGetElementType(out var elementType))
        {
            throw new ArgumentException("Array expression must of an array type", nameof(array));
        }

        if (type.UnwrapNullableType() != elementType.UnwrapNullableType())
        {
            throw new ArgumentException($"Mismatch between array type ({array.Type.Name}) and expression type ({type})");
        }

        if (index.Type != typeof(int))
        {
            throw new ArgumentException("Index expression must of type int", nameof(index));
        }

        Array = array;
        Index = index;
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="array">The <see cref="Array" /> property of the result.</param>
    /// <param name="index">The <see cref="Index" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual PostgresArrayIndexExpression Update(SqlExpression array, SqlExpression index)
        => array == Array && index == Index
            ? this
            : new PostgresArrayIndexExpression(array, index, Type, TypeMapping);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(Array), (SqlExpression)visitor.Visit(Index));

    /// <inheritdoc />
    public virtual bool Equals(PostgresArrayIndexExpression? other)
        => ReferenceEquals(this, other) ||
            other is not null &&
            base.Equals(other) &&
            Array.Equals(other.Array) &&
            Index.Equals(other.Index);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PostgresArrayIndexExpression e && Equals(e);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Array, Index);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Array);
        expressionPrinter.Append("[");
        expressionPrinter.Visit(Index);
        expressionPrinter.Append("]");
    }

    /// <inheritdoc />
    public override string ToString() => $"{Array}[{Index}]";
}
