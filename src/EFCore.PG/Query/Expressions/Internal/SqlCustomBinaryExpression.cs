using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// PostgreSQL has quite a few custom operators (full text, JSON and many others). Rather than creating expression
    /// types for each, this type represents an arbitrary expression with two operands and an operator.
    /// </summary>
    public class SqlCustomBinaryExpression : SqlExpression, IEquatable<SqlCustomBinaryExpression>
    {
        /// <summary>
        /// The left-hand expression.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Left { get; }

        /// <summary>
        /// The right-hand expression.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Right { get; }

        /// <summary>
        /// The operator.
        /// </summary>
        [NotNull]
        public virtual string Operator { get; }

        /// <summary>
        /// Constructs a <see cref="SqlCustomBinaryExpression"/>.
        /// </summary>
        /// <param name="left">The left-hand expression.</param>
        /// <param name="right">The right-hand expression.</param>
        /// <param name="binaryOperator">The operator symbol acting on the expression.</param>
        /// <param name="type">The result type.</param>
        /// <exception cref="ArgumentNullException" />
        public SqlCustomBinaryExpression(
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [NotNull] string binaryOperator,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping = null)
            : base(type, typeMapping)
        {
            Left = Check.NotNull(left, nameof(left));
            Right = Check.NotNull(right, nameof(right));
            Operator = Check.NotEmpty(binaryOperator, nameof(binaryOperator));
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitCustomBinary(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Left), (SqlExpression)visitor.Visit(Right));

        public virtual SqlCustomBinaryExpression Update([NotNull] SqlExpression left, [NotNull] SqlExpression right)
            => left == Left && right == Right
                ? this
                : new SqlCustomBinaryExpression(left, right, Operator, Type, TypeMapping);

        public virtual bool Equals(SqlCustomBinaryExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               Left.Equals(other.Left) &&
               Right.Equals(other.Right) &&
               Operator == other.Operator;

        public override bool Equals(object obj) => obj is SqlCustomBinaryExpression e && Equals(e);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Left, Right, Operator);

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Left);
            expressionPrinter.Append(Operator);
            expressionPrinter.Visit(Right);
        }

        /// <inheritdoc />
        public override string ToString() => $"{Left} {Operator} {Right}";
    }
}
