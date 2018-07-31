using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// PostgreSQL has quite a few custom operators (full text, JSON and many others). Rather than creating expression
    /// types for each, this type represents an arbitrary expression with two operands and an operator.
    /// </summary>
    public class CustomBinaryExpression : Expression
    {
        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>
        /// The left-hand expression.
        /// </summary>
        [NotNull]
        public Expression Left { get; }

        /// <summary>
        /// The right-hand expression.
        /// </summary>
        [NotNull]
        public Expression Right { get; }

        /// <summary>
        /// The operator.
        /// </summary>
        [NotNull]
        public string Operator { get; }

        /// <summary>
        /// Constructs a <see cref="CustomBinaryExpression"/>.
        /// </summary>
        /// <param name="left">The left-hand expression.</param>
        /// <param name="right">The right-hand expression.</param>
        /// <param name="binaryOperator">The operator symbol acting on the expression.</param>
        /// <param name="type">The result type.</param>
        /// <exception cref="ArgumentNullException" />
        public CustomBinaryExpression(
            [NotNull] Expression left,
            [NotNull] Expression right,
            [NotNull] string binaryOperator,
            [NotNull] Type type)
        {
            Left = Check.NotNull(left, nameof(left));
            Right = Check.NotNull(right, nameof(right));
            Operator = Check.NotEmpty(binaryOperator, nameof(binaryOperator));
            Type = Check.NotNull(type, nameof(type));
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitCustomBinary(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var left = visitor.Visit(Left) ?? Left;
            var right = visitor.Visit(Right) ?? Right;

            return
                left != Left || right != Right
                    ? new CustomBinaryExpression(left, right, Operator, Type)
                    : this;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Left} {Operator} {Right}";
    }
}
