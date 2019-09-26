using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// PostgreSQL has quite a few custom operators (full text, JSON and many others). Rather than creating expression
    /// types for each, this type represents an arbitrary expression with one operand and an operator.
    /// </summary>
    public class CustomUnaryExpression : Expression
    {
        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>
        /// The expression acted on by the operator.
        /// </summary>
        [NotNull]
        public Expression Operand { get; }

        /// <summary>
        /// The operator.
        /// </summary>
        [NotNull]
        public string Operator { get; }

        /// <summary>
        /// True if the operator follows the operand; otherwise, false.
        /// </summary>
        public bool Postfix { get; }

        /// <summary>
        /// Constructs a <see cref="CustomUnaryExpression"/>.
        /// </summary>
        /// <param name="operand">The expression acted on by the <paramref name="unaryOperator"/>.</param>
        /// <param name="unaryOperator">The operator symbol acting on the expression.</param>
        /// <param name="type">The result type.</param>
        /// <param name="postfix">True if the <paramref name="unaryOperator"/> follows the operand; otherwise, false.</param>
        /// <exception cref="ArgumentNullException" />
        public CustomUnaryExpression(
            [NotNull] Expression operand,
            [NotNull] string unaryOperator,
            [NotNull] Type type,
            bool postfix = false)
        {
            Operand = Check.NotNull(operand, nameof(operand));
            Operator = Check.NotEmpty(unaryOperator, nameof(unaryOperator));
            Type = Check.NotNull(type, nameof(type));
            Postfix = postfix;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitCustomUnary(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var operand = visitor.Visit(Operand) ?? Operand;

            return
                operand != Operand
                    ? new CustomUnaryExpression(operand, Operator, Type)
                    : this;
        }

        /// <inheritdoc />
        public override string ToString() => Postfix ? $"{Operator}{Operand}" : $"{Operand}{Operator}";
    }
}
