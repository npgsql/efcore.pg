namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
/// Represents creating a new PostgreSQL array.
/// </summary>
public class PostgresNewArrayExpression : SqlExpression
{
    /// <summary>
    /// Creates a new instance of the <see cref="PostgresNewArrayExpression" /> class.
    /// </summary>
    /// <param name="expressions">The values to initialize the elements of the new array.</param>
    /// <param name="type">The <see cref="Type"/> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping"/> associated with the expression.</param>
    public PostgresNewArrayExpression(
        IReadOnlyList<SqlExpression> expressions,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        Check.NotNull(expressions, nameof(expressions));

        if (!type.IsArrayOrGenericList())
        {
            throw new ArgumentException($"{nameof(PostgresNewArrayExpression)} must have an array type");
        }

        Expressions = expressions;
    }

    /// <summary>
    ///     The operator of this PostgreSQL binary operation.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Expressions { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Check.NotNull(visitor, nameof(visitor));

        List<SqlExpression>? newExpressions = null;
        for (var i = 0; i < Expressions.Count; i++)
        {
            var expression = Expressions[i];
            var visitedExpression = (SqlExpression)visitor.Visit(expression);
            if (visitedExpression != expression && newExpressions is null)
            {
                newExpressions = new List<SqlExpression>();
                for (var j = 0; j < i; j++)
                {
                    newExpressions.Add(visitedExpression);
                }
            }

            newExpressions?.Add(visitedExpression);
        }

        return newExpressions is null
            ? this
            : new PostgresNewArrayExpression(newExpressions, Type, TypeMapping);
    }

    /// <summary>
    /// Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    /// return this expression.
    /// </summary>
    /// <param name="expressions">The values to initialize the elements of the new array.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual PostgresNewArrayExpression Update(IReadOnlyList<SqlExpression> expressions)
    {
        Check.NotNull(expressions, nameof(expressions));

        return expressions == Expressions
            ? this
            : new PostgresNewArrayExpression(expressions, Type, TypeMapping);
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        Check.NotNull(expressionPrinter, nameof(expressionPrinter));

        expressionPrinter.Append("ARRAY[");

        var first = true;
        foreach (var expression in Expressions)
        {
            if (!first)
            {
                expressionPrinter.Append(", ");
            }

            first = false;

            expressionPrinter.Visit(expression);
        }

        expressionPrinter.Append("]");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is not null
            && (ReferenceEquals(this, obj)
                || obj is PostgresNewArrayExpression sqlBinaryExpression
                && Equals(sqlBinaryExpression));

    private bool Equals(PostgresNewArrayExpression postgresNewArrayExpression)
        => base.Equals(postgresNewArrayExpression)
            && Expressions.SequenceEqual(postgresNewArrayExpression.Expressions);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(base.GetHashCode());
        for (var i = 0; i < Expressions.Count; i++)
        {
            hash.Add(Expressions[i]);
        }

        return hash.ToHashCode();
    }
}