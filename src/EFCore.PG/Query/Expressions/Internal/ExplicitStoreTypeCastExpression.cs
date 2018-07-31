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
    /// Represents a SQL CAST expression to a store type specified as a string rather than a CLR type.
    /// </summary>
    public class ExplicitStoreTypeCastExpression : Expression, IEquatable<ExplicitStoreTypeCastExpression>
    {
        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>
        /// The operand.
        /// </summary>
        [NotNull]
        public virtual Expression Operand { get; }

        /// <summary>
        /// The store type name.
        /// </summary>
        [NotNull]
        public string StoreType { get; }

        /// <summary>
        /// Constructs a <see cref="ExplicitStoreTypeCastExpression"/>.
        /// </summary>
        /// <param name="operand">The operand.</param>
        /// <param name="type">The target type.</param>
        /// <param name="storeType">The store type name.</param>
        public ExplicitStoreTypeCastExpression(
            [NotNull] Expression operand,
            [NotNull] Type type,
            [NotNull] string storeType)
        {
            Operand = Check.NotNull(operand, nameof(operand));
            Type = Check.NotNull(type, nameof(type));
            StoreType = Check.NotNull(storeType, nameof(storeType));
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitExplicitStoreTypeCast(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var operand = visitor.Visit(Operand) ?? Operand;

            return
                operand != Operand
                    ? new ExplicitStoreTypeCastExpression(operand, Type, StoreType)
                    : this;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is ExplicitStoreTypeCastExpression other && Equals(other);

        /// <inheritdoc />
        public bool Equals(ExplicitStoreTypeCastExpression other)
            => other != null &&
               Type == other.Type &&
               StoreType == other.StoreType &&
               Equals(Operand, other.Operand);

        /// <inheritdoc />
        public override int GetHashCode()
            => unchecked((Type.GetHashCode() * 397) ^ (StoreType.GetHashCode() * 397) ^ Operand.GetHashCode());

        /// <inheritdoc />
        public override string ToString() => $"CAST({Operand} AS {StoreType})";
    }
}
