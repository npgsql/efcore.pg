using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// An SQL expression that represents an indexing into a PostgreSQL array.
    /// </summary>
    /// <remarks>
    /// <see cref="SqlBinaryExpression"/> specifically disallows having an <see cref="SqlBinaryExpression.OperatorType"/>
    /// of value <see cref="ExpressionType.ArrayIndex"/> as arrays are a PostgreSQL-only feature.
    /// </remarks>
    public class ArrayIndexExpression : SqlExpression, IEquatable<ArrayIndexExpression>
    {
        public ArrayIndexExpression(
            [NotNull] SqlExpression array,
            [NotNull] SqlExpression index,
            Type type,
            RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Check.NotNull(array, nameof(array));
            Check.NotNull(index, nameof(index));

            if (!array.Type.IsArray && !array.Type.IsGenericList())
                throw new ArgumentException("Array expression must of an array type", nameof(array));
            if (index.Type != typeof(int))
                throw new ArgumentException("Index expression must of type int", nameof(index));

            Array = array;
            Index = index;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitArrayIndex(this)
                : base.Accept(visitor);

        public ArrayIndexExpression Update(SqlExpression array, SqlExpression index)
            => array == Array && index == Index ? this : new ArrayIndexExpression(array, index, Type, TypeMapping);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Array), (SqlExpression)visitor.Visit(Index));

        /// <summary>
        /// The array being indexes.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Array { get; }

        /// <summary>
        /// The index in the array.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Index { get; }

        public bool Equals(ArrayIndexExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               base.Equals(other) &&
               Array.Equals(other.Array) &&
               Index.Equals(other.Index);

        public override bool Equals(object obj) => obj is ArrayIndexExpression e && Equals(e);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Array, Index);

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Array);
            expressionPrinter.Append("[");
            expressionPrinter.Visit(Index);
            expressionPrinter.Append("]");
        }

        public override string ToString() => $"{Array}[{Index}]";
    }
}
