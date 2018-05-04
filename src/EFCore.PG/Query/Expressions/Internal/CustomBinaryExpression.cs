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
    /// types for each, this type represents an arbitrary expression with two operands and an operator.
    /// </summary>
    public class CustomBinaryExpression : Expression
    {
        public CustomBinaryExpression(
            [NotNull] Expression left,
            [NotNull] Expression right,
            [NotNull] string binaryOperator,
            [NotNull] Type type)
        {
            Check.NotNull(right, nameof(right));
            Check.NotNull(left, nameof(left));
            Check.NotEmpty(binaryOperator, nameof(binaryOperator));
            Check.NotNull(type, nameof(type));

            Left = left;
            Right = right;
            Operator = binaryOperator;
            Type = type;
        }

        public Expression Left { get; }
        public Expression Right { get; }
        public string Operator { get; }
        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitCustomBinary(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newLeft = visitor.Visit(Left);
            var newRight = visitor.Visit(Right);

            return newLeft != Left || newRight != Right
                ? new CustomBinaryExpression(newLeft, newRight, Operator, Type)
                : this;
        }

        public override string ToString() => $"{Left} {Operator} {Right}";
    }
}
