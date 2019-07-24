using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Pipeline;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL array ANY or ALL expression.
    /// </summary>
    /// <example>
    /// 1 = ANY ('{0,1,2}'), 'cat' LIKE ANY ('{a%,b%,c%}')
    /// </example>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/functions-comparisons.html
    /// </remarks>
    public class ArrayAnyAllExpression : SqlExpression, IEquatable<ArrayAnyAllExpression>
    {
        /// <inheritdoc />
        public override Type Type => typeof(bool);

        /// <summary>
        /// The value to test against the <see cref="Array"/>.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Operand { get; }

        /// <summary>
        /// The array of values or patterns to test for the <see cref="Operand"/>.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Array { get; }

        /// <summary>
        /// The operator.
        /// </summary>
        [NotNull]
        public virtual string Operator { get; }

        /// <summary>
        /// The comparison type.
        /// </summary>
        public virtual ArrayComparisonType ArrayComparisonType { get; }

        /// <summary>
        /// Constructs a <see cref="ArrayAnyAllExpression"/>.
        /// </summary>
        /// <param name="arrayComparisonType">The comparison type.</param>
        /// <param name="operator">The operator symbol to the array expression.</param>
        /// <param name="operand">The value to find.</param>
        /// <param name="array">The array to search.</param>
        /// <param name="typeMapping">The type mapping for the expression.</param>
        public ArrayAnyAllExpression(
            [NotNull] SqlExpression operand,
            [NotNull] SqlExpression array,
            ArrayComparisonType arrayComparisonType,
            [NotNull] string @operator,
            RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            if (!array.Type.IsArray && !array.Type.IsGenericType && array.Type.GetGenericTypeDefinition() != typeof(List<>))
                throw new ArgumentException("Array expression must be of type array or List<>", nameof(array));

            Operand = operand;
            Array = array;
            ArrayComparisonType = arrayComparisonType;
            Operator = @operator;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitArrayAnyAll(this)
                : base.Accept(visitor);

        public ArrayAnyAllExpression Update(SqlExpression operand, SqlExpression array)
            => operand != Operand || array != Array
                ? new ArrayAnyAllExpression(operand, array, ArrayComparisonType, Operator, TypeMapping)
                : this;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Operand), (SqlExpression)visitor.Visit(Array));

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is ArrayAnyAllExpression e && Equals(e);

        /// <inheritdoc />
        public bool Equals(ArrayAnyAllExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               base.Equals(other) &&
               Operand.Equals(other.Operand) &&
               Array.Equals(other.Array) &&
               ArrayComparisonType.Equals(other.ArrayComparisonType) &&
               Operator.Equals(other.Operator);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Operand, Array, ArrayComparisonType, Operator);

        /// <inheritdoc />
        public override void Print(ExpressionPrinter expressionPrinter)
        {
#pragma warning disable EF1001
            expressionPrinter.Visit(Operand);
            expressionPrinter.StringBuilder.Append($" = {Operator} (");
            expressionPrinter.Visit(Array);
            expressionPrinter.StringBuilder.Append(')');
#pragma warning restore EF1001
        }

        /// <inheritdoc />
        public override string ToString() => $"{Operand} {Operator} {ArrayComparisonType.ToString()} ({Array})";
    }

    /// <summary>
    /// Represents whether an array comparison is ANY or ALL.
    /// </summary>
    public enum ArrayComparisonType
    {
        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Represents an ANY array comparison.
        /// </summary>
        Any,

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Represents an ALL array comparison.
        /// </summary>
        All
    }
}
