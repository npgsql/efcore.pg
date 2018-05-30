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
    /// Represents a PostgreSQL array ANY or ALL expression.
    /// </summary>
    /// <example>
    /// 1 = ANY ('{0,1,2}'), 'cat' LIKE ANY ('{a%,b%,c%}')
    /// </example>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/functions-comparisons.html
    /// </remarks>
    public class ArrayAnyAllExpression : Expression, IEquatable<ArrayAnyAllExpression>
    {
        /// <inheritdoc />
        public override ExpressionType NodeType { get; } = ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; } = typeof(bool);

        /// <summary>
        /// The value to test against the <see cref="Array"/>.
        /// </summary>
        public virtual Expression Operand { get; }

        /// <summary>
        /// The array of values or patterns to test for the <see cref="Operand"/>.
        /// </summary>
        public virtual Expression Array { get; }

        /// <summary>
        /// The operator.
        /// </summary>
        public virtual string Operator { get; }

        /// <summary>
        /// The comparison type.
        /// </summary>
        public virtual ArrayComparisonType ArrayComparisonType { get; }

        /// <summary>
        /// True if this instance represents: {operand} = ANY ({array})".
        /// </summary>
        public bool IsAnyEquals => ArrayComparisonType is ArrayComparisonType.ANY && Operator is "=";

        /// <summary>
        /// Constructs a <see cref="ArrayAnyAllExpression"/>.
        /// </summary>
        /// <param name="arrayComparisonType">The comparison type.</param>
        /// <param name="operatorSymbol">The operator symbol to the array expression.</param>
        /// <param name="operand">The value to find.</param>
        /// <param name="array">The array to search.</param>
        /// <exception cref="ArgumentNullException" />
        public ArrayAnyAllExpression(
            ArrayComparisonType arrayComparisonType,
            [NotNull] string operatorSymbol,
            [NotNull] Expression operand,
            [NotNull] Expression array)
        {
            Check.NotNull(array, nameof(operatorSymbol));
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(array, nameof(array));

            ArrayComparisonType = arrayComparisonType;
            Operator = operatorSymbol;
            Operand = operand;
            Array = array;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npsgqlGenerator
                ? npsgqlGenerator.VisitArrayAnyAll(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (!(visitor.Visit(Operand) is Expression operand))
                throw new ArgumentException($"The {nameof(operand)} of a {nameof(ArrayAnyAllExpression)} cannot be null.");

            if (!(visitor.Visit(Array) is Expression collection))
                throw new ArgumentException($"The {nameof(collection)} of a {nameof(ArrayAnyAllExpression)} cannot be null.");

            return
                operand == Operand && collection == Array
                    ? this
                    : new ArrayAnyAllExpression(ArrayComparisonType, Operator, operand, collection);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is ArrayAnyAllExpression likeAnyExpression && Equals(likeAnyExpression);

        /// <inheritdoc />
        public bool Equals(ArrayAnyAllExpression other)
            => Operand.Equals(other?.Operand) &&
               Operator.Equals(other?.Operator) &&
               ArrayComparisonType.Equals(other?.ArrayComparisonType) &&
               Array.Equals(other?.Array);

        /// <inheritdoc />
        public override int GetHashCode()
            => unchecked((397 * Operand.GetHashCode()) ^
                         (397 * Operator.GetHashCode()) ^
                         (397 * ArrayComparisonType.GetHashCode()) ^
                         (397 * Array.GetHashCode()));

        /// <inheritdoc />
        public override string ToString()
            => $"{Operand} {Operator} {ArrayComparisonType.ToString()} ({Array})";
    }

    /// <summary>
    /// Represents whether an array comparison is ANY or ALL.
    /// </summary>
    public enum ArrayComparisonType
    {
        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Represents an ANY array comparison.
        /// </summary>
        ANY,

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Represents an ALL array comparison.
        /// </summary>
        ALL
    }
}
