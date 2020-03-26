using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    public class CollateExpression : SqlExpression, IEquatable<CollateExpression>
    {
        /// <summary>
        /// The operand on which to apply the explicit collation.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Operand { get; }

        /// <summary>
        /// The collation to apply to <see cref="Operand"/>.
        /// </summary>
        [NotNull]
        public virtual string Collation { get; }

        public CollateExpression([NotNull] SqlExpression operand, [NotNull] string collation)
            : base(operand.Type, operand.TypeMapping)
        {
            Operand = operand;
            Collation = collation;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitCollation(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Operand));

        public CollateExpression Update(SqlExpression operand)
            => operand == Operand
                ? this
                : new CollateExpression(operand, Collation);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is CollateExpression other && Equals(other);

        /// <inheritdoc />
        public bool Equals(CollateExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               base.Equals(other) &&
               Equals(Operand, other.Operand) &&
               Collation == other.Collation;

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Operand, Collation);

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Operand);
            expressionPrinter.Append(@$" COLLATE ""{Collation}""");
        }

        /// <inheritdoc />
        public override string ToString() => $@"{Operand} COLLATE ""{Collation}""";
    }
}
