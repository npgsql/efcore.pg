namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

public sealed class PostgresDeleteExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     The tables that rows are to be deleted from.
    /// </summary>
    public TableExpression Table { get; }

    /// <summary>
    ///     Additional tables which can be referenced in the predicate.
    /// </summary>
    public IReadOnlyList<TableExpressionBase> FromItems { get; }

    /// <summary>
    ///     The WHERE predicate for the DELETE.
    /// </summary>
    public SqlExpression? Predicate { get; }

    public PostgresDeleteExpression(TableExpression table, IReadOnlyList<TableExpressionBase> fromItems, SqlExpression? predicate)
        => (Table, FromItems, Predicate) = (table, fromItems, predicate);

    /// <inheritdoc />
    public override Type Type
        => typeof(object);

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Predicate is null
            ? this
            : Update((SqlExpression?)visitor.Visit(Predicate));

    public PostgresDeleteExpression Update(SqlExpression? predicate)
        => predicate == Predicate
            ? this
            : new PostgresDeleteExpression(Table, FromItems, predicate);

    public void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine($"DELETE FROM {Table.Name} AS {Table.Alias}");

        if (FromItems.Count > 0)
        {
            var first = true;
            foreach (var fromItem in FromItems)
            {
                if (first)
                {
                    expressionPrinter.Append("USING ");
                    first = false;
                }
                else
                {
                    expressionPrinter.Append(", ");
                }

                expressionPrinter.Visit(fromItem);
            }
        }

        if (Predicate is not null)
        {
            expressionPrinter.Append("WHERE ");
            expressionPrinter.Visit(Predicate);
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is PostgresDeleteExpression pgDeleteExpression
                && Equals(pgDeleteExpression));

    private bool Equals(PostgresDeleteExpression pgDeleteExpression)
        => Table == pgDeleteExpression.Table
            && FromItems.SequenceEqual(pgDeleteExpression.FromItems)
            && (Predicate is null ? pgDeleteExpression.Predicate is null : Predicate.Equals(pgDeleteExpression.Predicate));

    /// <inheritdoc />
    public override int GetHashCode() => Table.GetHashCode();
}
