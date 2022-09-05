namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
/// A binary expression only to be used by plugins, since new expressions can only be added (and handled)
/// within the provider itself. Allows defining the operator as a string within the expression, and has
/// default (i.e. propagating) nullability semantics.
/// All type mappings must be applied to the operands before the expression is constructed, since there's
/// no inference logic for it in <see cref="NpgsqlSqlExpressionFactory" />.
/// </summary>
public class PostgresUnknownBinaryExpression : SqlExpression, IEquatable<PostgresUnknownBinaryExpression>
{
    /// <summary>
    /// The left-hand expression.
    /// </summary>
    public virtual SqlExpression Left { get; }

    /// <summary>
    /// The right-hand expression.
    /// </summary>
    public virtual SqlExpression Right { get; }

    /// <summary>
    /// The operator.
    /// </summary>
    public virtual string Operator { get; }

    /// <summary>
    /// Constructs a <see cref="PostgresUnknownBinaryExpression"/>.
    /// </summary>
    /// <param name="left">The left-hand expression.</param>
    /// <param name="right">The right-hand expression.</param>
    /// <param name="binaryOperator">The operator symbol acting on the expression.</param>
    /// <param name="type">The result type.</param>
    /// <param name="typeMapping">The type mapping for the expression.</param>
    /// <exception cref="ArgumentNullException" />
    public PostgresUnknownBinaryExpression(
        SqlExpression left,
        SqlExpression right,
        string binaryOperator,
        Type type,
        RelationalTypeMapping? typeMapping = null)
        : base(type, typeMapping)
    {
        Left = Check.NotNull(left, nameof(left));
        Right = Check.NotNull(right, nameof(right));
        Operator = Check.NotEmpty(binaryOperator, nameof(binaryOperator));
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(Left), (SqlExpression)visitor.Visit(Right));

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual PostgresUnknownBinaryExpression Update(SqlExpression left, SqlExpression right)
        => left == Left && right == Right
            ? this
            : new PostgresUnknownBinaryExpression(left, right, Operator, Type, TypeMapping);

    /// <inheritdoc />
    public virtual bool Equals(PostgresUnknownBinaryExpression? other)
        => ReferenceEquals(this, other) ||
            other is not null &&
            Left.Equals(other.Left) &&
            Right.Equals(other.Right) &&
            Operator == other.Operator;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PostgresUnknownBinaryExpression e && Equals(e);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Left, Right, Operator);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Left);
        expressionPrinter.Append(Operator);
        expressionPrinter.Visit(Right);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Left} {Operator} {Right}";
}
