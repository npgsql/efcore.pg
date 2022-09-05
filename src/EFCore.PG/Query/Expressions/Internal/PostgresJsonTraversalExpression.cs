namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
/// Represents a PostgreSQL JSON operator traversing a JSON document with a path (i.e. x#>y or x#>>y)
/// </summary>
public class PostgresJsonTraversalExpression : SqlExpression, IEquatable<PostgresJsonTraversalExpression>
{
    /// <summary>
    /// The match expression.
    /// </summary>
    public virtual SqlExpression Expression { get; }

    /// <summary>
    /// The pattern to match.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Path { get; }

    /// <summary>
    /// Whether the text-returning operator (x#>>y) or the object-returning operator (x#>y) is used.
    /// </summary>
    public virtual bool ReturnsText { get; }

    /// <summary>
    /// Constructs a <see cref="PostgresJsonTraversalExpression"/>.
    /// </summary>
    public PostgresJsonTraversalExpression(
        SqlExpression expression,
        IReadOnlyList<SqlExpression> path,
        bool returnsText,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        if (returnsText && type != typeof(string))
        {
            throw new ArgumentException($"{nameof(type)} must be string", nameof(type));
        }

        Expression = expression;
        Path = path;
        ReturnsText = returnsText;
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update(
            (SqlExpression)visitor.Visit(Expression),
            Path.Select(p => (SqlExpression)visitor.Visit(p)).ToArray());

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual PostgresJsonTraversalExpression Update(SqlExpression expression, IReadOnlyList<SqlExpression> path)
        => expression == Expression &&
            path.Count == Path.Count &&
            path.Zip(Path, (x, y) => (x, y)).All(tup => tup.x == tup.y)
                ? this
                : new PostgresJsonTraversalExpression(expression, path, ReturnsText, Type, TypeMapping);

    /// <summary>
    ///     Appends an additional path component to this <see cref="PostgresJsonTraversalExpression" /> and returns the result.
    /// </summary>
    public virtual PostgresJsonTraversalExpression Append(SqlExpression pathComponent)
    {
        var newPath = new SqlExpression[Path.Count + 1];
        for (var i = 0; i < Path.Count(); i++)
        {
            newPath[i] = Path[i];
        }

        newPath[newPath.Length - 1] = pathComponent;
        return new PostgresJsonTraversalExpression(Expression, newPath, ReturnsText, Type, TypeMapping);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as PostgresJsonTraversalExpression);

    /// <inheritdoc />
    public virtual bool Equals(PostgresJsonTraversalExpression? other)
        => ReferenceEquals(this, other) ||
            other is not null &&
            base.Equals(other) &&
            Equals(Expression, other.Expression) &&
            Path.Count == other.Path.Count &&
            Path.Zip(other.Path, (x, y) => (x, y)).All(tup => tup.x == tup.y);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Expression, Path);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Expression);
        expressionPrinter.Append(ReturnsText ? "#>>" : "#>");
        expressionPrinter.Append("{");
        for (var i = 0; i < Path.Count; i++)
        {
            expressionPrinter.Visit(Path[i]);
            if (i < Path.Count - 1)
            {
                expressionPrinter.Append(",");
            }
        }
        expressionPrinter.Append("}");
    }

    /// <inheritdoc />
    public override string ToString() => $"{Expression}{(ReturnsText ? "#>>" : "#>")}{Path}";
}
