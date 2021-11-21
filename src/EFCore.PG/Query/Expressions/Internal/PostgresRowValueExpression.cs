using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
/// An expression that represents a PostgreSQL-specific row value expression in a SQL tree.
/// </summary>
/// <remarks>
/// See the <see href="https://www.postgresql.org/docs/current/sql-expressions.html#SQL-SYNTAX-ROW-CONSTRUCTORS">PostgreSQL docs</see>
/// for more information.
/// </remarks>
public class PostgresRowValueExpression : SqlExpression, IEquatable<PostgresRowValueExpression>
{
    /// <summary>
    /// The operator of this PostgreSQL binary operation.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> RowValues { get; }

    /// <inheritdoc />
    public PostgresRowValueExpression(IReadOnlyList<SqlExpression> rowValues)
        : base(typeof(Array), typeMapping: null)
    {
        Check.NotNull(rowValues, nameof(rowValues));

        RowValues = rowValues;
    }

    public virtual PostgresRowValueExpression Prepend(SqlExpression expression)
    {
        var newRowValues = new SqlExpression[RowValues.Count + 1];
        newRowValues[0] = expression;
        for (var i = 1; i < newRowValues.Length; i++)
        {
            newRowValues[i] = RowValues[i - 1];
        }

        return new PostgresRowValueExpression(newRowValues);
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Check.NotNull(visitor, nameof(visitor));

        SqlExpression[]? newRowValues = null;

        for (var i = 0; i < RowValues.Count; i++)
        {
            var rowValue = RowValues[i];
            var visited = (SqlExpression)visitor.Visit(rowValue);
            if (visited != rowValue && newRowValues is null)
            {
                newRowValues = new SqlExpression[RowValues.Count];
                for (var j = 0; j < i; i++)
                {
                    newRowValues[j] = RowValues[j];
                }
            }

            if (newRowValues is not null)
            {
                newRowValues[i] = visited;
            }
        }

        return newRowValues is null ? this : new PostgresRowValueExpression(newRowValues);
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("(");

        var count = RowValues.Count;
        for (var i = 0; i < count; i++)
        {
            expressionPrinter.Visit(RowValues[i]);

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
        if (other is null || !base.Equals(other) || other.RowValues.Count != RowValues.Count)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        for (var i = 0; i < RowValues.Count; i++)
        {
            if (other.RowValues[i].Equals(RowValues[i]))
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

        foreach (var rowValue in RowValues)
        {
            hashCode.Add(rowValue);
        }

        return hashCode.ToHashCode();
    }
}