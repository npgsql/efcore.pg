namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
///     An SQL expression that represents an indexing into a PostgreSQL array.
/// </summary>
/// <remarks>
///     <see cref="SqlBinaryExpression" /> specifically disallows having an <see cref="SqlBinaryExpression.OperatorType" />
///     of value <see cref="ExpressionType.ArrayIndex" /> as arrays are a PostgreSQL-only feature.
/// </remarks>
public class PgArrayIndexExpression : SqlExpression, IEquatable<PgArrayIndexExpression>
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     The array being indexed.
    /// </summary>
    public virtual SqlExpression Array { get; }

    /// <summary>
    ///     The index in the array.
    /// </summary>
    public virtual SqlExpression Index { get; }

    /// <summary>
    ///     Whether the expression is nullable.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="PgArrayIndexExpression" /> class.
    /// </summary>
    /// <param name="array">The array tp index into.</param>
    /// <param name="index">An position in the array to index into.</param>
    /// <param name="nullable">Whether the expression is nullable.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public PgArrayIndexExpression(
        SqlExpression array,
        SqlExpression index,
        bool nullable,
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
        IsNullable = nullable;
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="array">The <see cref="Array" /> property of the result.</param>
    /// <param name="index">The <see cref="Index" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual PgArrayIndexExpression Update(SqlExpression array, SqlExpression index)
        => array == Array && index == Index
            ? this
            : new PgArrayIndexExpression(array, index, IsNullable, Type, TypeMapping);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(PgArrayIndexExpression).GetConstructor(
                [typeof(SqlExpression), typeof(SqlExpression), typeof(bool), typeof(Type), typeof(RelationalTypeMapping)])!,
            Array.Quote(),
            Index.Quote(),
            Constant(IsNullable),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(Array), (SqlExpression)visitor.Visit(Index));

    /// <inheritdoc />
    public virtual bool Equals(PgArrayIndexExpression? other)
        => ReferenceEquals(this, other)
            || other is not null
            && base.Equals(other)
            && Array.Equals(other.Array)
            && Index.Equals(other.Index)
            && IsNullable == other.IsNullable;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is PgArrayIndexExpression e && Equals(e);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Array, Index, IsNullable);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Array);
        expressionPrinter.Append("[");
        expressionPrinter.Visit(Index);
        expressionPrinter.Append("]");
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{Array}[{Index}]";
}
