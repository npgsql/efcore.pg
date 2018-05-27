#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL ANY expression (e.g. 1 = ANY ('{0,1,2}'), 'cat' LIKE ANY ('{a%,b%,c%}')).
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/functions-comparisons.html
    /// </remarks>
    public class CustomArrayExpression : Expression, IEquatable<CustomArrayExpression>
    {
        /// <summary>
        /// True if the operator is modified by ANY; otherwise, false to modify with ALL.
        /// </summary>
        readonly bool _any;

        /// <inheritdoc />
        public override ExpressionType NodeType { get; } = ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; } = typeof(bool);

        /// <summary>
        /// The value to test against the <see cref="Collection"/>.
        /// </summary>
        public virtual Expression Operand { get; }

        /// <summary>
        /// The collection of values or patterns to test for the <see cref="Operand"/>.
        /// </summary>
        public virtual Expression Collection { get; }

        /// <summary>
        /// The operator.
        /// </summary>
        public virtual string Operator { get; }

        /// <summary>
        /// The type of the operator expression (ANY or ALL).
        /// </summary>
        public virtual string OperatorType => _any ? "ANY" : "ALL";

        /// <summary>
        /// Constructs a <see cref="CustomArrayExpression"/>.
        /// </summary>
        /// <param name="operand">The value to find.</param>
        /// <param name="collection">The collection to search.</param>
        /// <param name="operatorSymbol">The operator symbol to the array expression.</param>
        /// <param name="any">True for ANY; false for ALL.</param>
        /// <exception cref="ArgumentNullException" />
        public CustomArrayExpression([NotNull] Expression operand, [NotNull] Expression collection, [NotNull] string operatorSymbol, bool any)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(collection, nameof(collection));

            Operand = operand;
            Operator = operatorSymbol;
            Collection = collection;
            _any = any;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npsgqlGenerator
                ? npsgqlGenerator.VisitArrayOperator(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (!(visitor.Visit(Operand) is Expression operand))
                throw new ArgumentException($"The {nameof(operand)} of a {nameof(CustomArrayExpression)} cannot be null.");

            if (!(visitor.Visit(Collection) is Expression collection))
                throw new ArgumentException($"The {nameof(collection)} of a {nameof(CustomArrayExpression)} cannot be null.");

            return
                operand == Operand && collection == Collection
                    ? this
                    : new CustomArrayExpression(operand, collection, Operator, _any);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is CustomArrayExpression likeAnyExpression && Equals(likeAnyExpression);

        /// <inheritdoc />
        public bool Equals(CustomArrayExpression other)
            => Operand.Equals(other?.Operand) && Collection.Equals(other?.Collection);

        /// <inheritdoc />
        public override int GetHashCode()
            => unchecked((397 * Operand.GetHashCode()) ^
                         (397 * Operator.GetHashCode()) ^
                         (397 * OperatorType.GetHashCode()) ^
                         (397 * Collection.GetHashCode()));

        /// <inheritdoc />
        public override string ToString()
            => $"{Operand} {Operator} {OperatorType} ({Collection})";
    }
}
