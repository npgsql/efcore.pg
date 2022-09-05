// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
///     An expression that represents a PostgreSQL-specific row value expression in a SQL tree.
/// </summary>
/// <remarks>
///     See the <see href="https://www.postgresql.org/docs/current/sql-expressions.html#SQL-SYNTAX-ROW-CONSTRUCTORS">PostgreSQL docs</see>
///     for more information.
/// </remarks>
public class PostgresRowValueExpression : SqlExpression, IEquatable<PostgresRowValueExpression>
{
    /// <summary>
    /// The values of this PostgreSQL row value expression.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Values { get; }

    /// <summary>
    ///     A type mapping representing a row value type.
    /// </summary>
    public static RelationalTypeMapping TypeMappingInstance => RowValueTypeMapping.Instance;

    /// <inheritdoc />
    public PostgresRowValueExpression(IReadOnlyList<SqlExpression> values, Type type, RelationalTypeMapping? typeMapping = null)
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
                for (var j = 0; j < i; i++)
                {
                    newRowValues[j] = Values[j];
                }
            }

            if (newRowValues is not null)
            {
                newRowValues[i] = visited;
            }
        }

        return newRowValues is null ? this : new PostgresRowValueExpression(newRowValues, Type);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual PostgresRowValueExpression Update(IReadOnlyList<SqlExpression> values)
        => values.Count == Values.Count && values.Zip(Values, (x, y) => (x, y)).All(tup => tup.x == tup.y)
            ? this
            : new PostgresRowValueExpression(values, Type);

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
        => obj is PostgresRowValueExpression other && Equals(other);

    /// <inheritdoc />
    public virtual bool Equals(PostgresRowValueExpression? other)
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

    /// <summary>
    /// Every node in the SQL tree must have a type mapping, but row values aren't actual values (in the sense that they can be sent as
    /// parameters, or have a literal representation). So we have a dummy type mapping for that.
    /// </summary>
    private sealed class RowValueTypeMapping : RelationalTypeMapping
    {
        internal static RowValueTypeMapping Instance { get; } = new();

        private RowValueTypeMapping()
            : base(new(new(), storeType: "rowvalue"))
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => this;
    }
}
