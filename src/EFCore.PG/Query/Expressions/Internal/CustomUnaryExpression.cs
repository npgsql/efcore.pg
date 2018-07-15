using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// PostgreSQL has quite a few custom operators (full text, JSON and many others). Rather than creating expression
    /// types for each, this type represents an arbitrary expression with one operand and an operator.
    /// </summary>
    public class CustomUnaryExpression : Expression
    {
        public CustomUnaryExpression(
            [NotNull] Expression operand,
            [NotNull] string @operator,
            [NotNull] Type type,
            bool postfix=false)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotEmpty(@operator, nameof(@operator));
            Check.NotNull(type, nameof(type));

            Operand = operand;
            Operator = @operator;
            Type = type;
            Postfix = postfix;
        }

        public Expression Operand { get; }
        public string Operator { get; }
        public override Type Type { get; }
        public bool Postfix { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitCustomUnary(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newOperand = visitor.Visit(Operand);

            return newOperand != Operand
                ? new CustomUnaryExpression(newOperand, Operator, Type)
                : this;
        }

        public override string ToString() => Postfix
            ? $"{Operator}{Operand}"
            : $"{Operand}{Operator}";
    }
}
