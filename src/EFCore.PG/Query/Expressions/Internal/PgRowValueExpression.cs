using System.Runtime.CompilerServices;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
///     An expression that represents a PostgreSQL-specific row value expression in a SQL tree.
/// </summary>
/// <remarks>
///     See the <see href="https://www.postgresql.org/docs/current/sql-expressions.html#SQL-SYNTAX-ROW-CONSTRUCTORS">PostgreSQL docs</see>
///     for more information.
/// </remarks>
public class PgRowValueExpression : SqlExpression, IEquatable<PgRowValueExpression>
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     The values of this PostgreSQL row value expression.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Values { get; }

    /// <inheritdoc />
    public PgRowValueExpression(
        IReadOnlyList<SqlExpression> values,
        Type type,
        RelationalTypeMapping? typeMapping = null)
        : base(type, typeMapping)
    {
        Check.NotNull(values, nameof(values));
        Check.DebugAssert(type.IsAssignableTo(typeof(ITuple)), $"Type '{type}' isn't an ITuple");

        Values = values;
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Check.NotNull(visitor, nameof(visitor));

        SqlExpression[]? newRowValues = null;

        for (var i = 0; i < Values.Count; i++)
        {
            var rowValue = Values[i];
            var visited = (SqlExpression)visitor.Visit(rowValue);
            if (visited != rowValue && newRowValues is null)
            {
                newRowValues = new SqlExpression[Values.Count];
                for (var j = 0; j < i; j++)
                {
                    newRowValues[j] = Values[j];
                }
            }

            if (newRowValues is not null)
            {
                newRowValues[i] = visited;
            }
        }

        return newRowValues is null ? this : new PgRowValueExpression(newRowValues, Type);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual PgRowValueExpression Update(IReadOnlyList<SqlExpression> values)
        => values.Count == Values.Count && values.Zip(Values, (x, y) => (x, y)).All(tup => tup.x == tup.y)
            ? this
            : new PgRowValueExpression(values, Type);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(PgRowValueExpression).GetConstructor(
                [typeof(IReadOnlyList<SqlExpression>), typeof(Type), typeof(RelationalTypeMapping)])!,
            NewArrayInit(typeof(SqlExpression), initializers: Values.Select(a => a.Quote())),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("(");

        var count = Values.Count;
        for (var i = 0; i < count; i++)
        {
            expressionPrinter.Visit(Values[i]);

            if (i < count - 1)
            {
                expressionPrinter.Append(", ");
            }
        }

        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is PgRowValueExpression other && Equals(other);

    /// <inheritdoc />
    public virtual bool Equals(PgRowValueExpression? other)
    {
        if (other is null || !base.Equals(other) || other.Values.Count != Values.Count)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        for (var i = 0; i < Values.Count; i++)
        {
            if (!other.Values[i].Equals(Values[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (var rowValue in Values)
        {
            hashCode.Add(rowValue);
        }

        return hashCode.ToHashCode();
    }
}
