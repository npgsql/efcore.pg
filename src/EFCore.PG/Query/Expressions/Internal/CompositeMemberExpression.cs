using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable ArgumentsStyleLiteral
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a member access on a mapped PostgreSQL composite type.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class CompositeMemberExpression : Expression, IEquatable<CompositeMemberExpression>
    {
        /// <summary>
        /// Gets the containing object of the composite member.
        /// </summary>
        [NotNull]
        public virtual Expression Expression { get; }

        /// <value>
        /// Gets the composite member to be accessed.
        /// </value>
        [NotNull]
        public virtual string Member { get; }

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeMemberExpression" /> class.
        /// </summary>
        public CompositeMemberExpression(
            [NotNull] Expression expression,
            [NotNull] string member,
            [NotNull] Type returnType)
        {
            Expression = expression;
            Member = member;
            Type = returnType;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitCompositeMember(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var e = visitor.Visit(Expression) ?? Expression;
            return e != Expression ? new CompositeMemberExpression(e, Member, Type) : this;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is CompositeMemberExpression e && Equals(e);

        /// <inheritdoc />
        public bool Equals(CompositeMemberExpression other)
            => other != null && Member == other.Member && Expression.Equals(other.Expression);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Expression.GetHashCode() * 397) ^ Member.GetHashCode();
            }
        }

        /// <inheritdoc />
        public override string ToString() => Expression + "." + Member;
    }
}
