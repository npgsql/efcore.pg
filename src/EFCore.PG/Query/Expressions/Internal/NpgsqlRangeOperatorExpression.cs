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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL range operator.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/functions-range.html
    /// </remarks>
    public class NpgsqlRangeOperatorExpression : Expression
    {
        /// <summary>
        /// Maps standard binary operators from <see cref="ExpressionType"/> to <see cref="NpgsqlRangeOperatorType"/>.
        /// </summary>
        [NotNull] static readonly Dictionary<ExpressionType, NpgsqlRangeOperatorType> ExpressionOperators =
            new Dictionary<ExpressionType, NpgsqlRangeOperatorType>
            {
                [ExpressionType.Equal] = NpgsqlRangeOperatorType.Equal,
                [ExpressionType.NotEqual] = NpgsqlRangeOperatorType.NotEqual,
                [ExpressionType.LessThan] = NpgsqlRangeOperatorType.LessThan,
                [ExpressionType.GreaterThan] = NpgsqlRangeOperatorType.GreaterThan,
                [ExpressionType.LessThanOrEqual] = NpgsqlRangeOperatorType.LessThanOrEqual,
                [ExpressionType.GreaterThanOrEqual] = NpgsqlRangeOperatorType.GreaterThanOrEqual
            };

        /// <summary>
        /// Maps the <see cref="NpgsqlRangeOperatorType"/> to a PostgreSQL operator symbol.
        /// </summary>
        [NotNull] static readonly Dictionary<NpgsqlRangeOperatorType, string> OperatorSymbols;

        /// <summary>
        /// Maps the <see cref="NpgsqlRangeOperatorType"/> to a return type.
        /// </summary>
        [NotNull] static readonly Dictionary<NpgsqlRangeOperatorType, Type> OperatorReturnTypes;

        /// <summary>
        /// Initializes instances of <see cref="NpgsqlOperatorAttribute"/> found on <see cref="NpgsqlRangeOperatorType"/>.
        /// </summary>
        static NpgsqlRangeOperatorExpression()
        {
            (NpgsqlRangeOperatorType Enum, NpgsqlOperatorAttribute Attribute)[] map =
                typeof(NpgsqlRangeOperatorType)
                    .GetEnumValues()
                    .Cast<NpgsqlRangeOperatorType>()
                    .Select(x => (Enum: x, Attribute: GetAttribute(x)))
                    .ToArray();

            OperatorSymbols = map.ToDictionary(x => x.Enum, x => x.Attribute.Symbol);
            OperatorReturnTypes = map.ToDictionary(x => x.Enum, x => x.Attribute.ReturnType);

            NpgsqlOperatorAttribute GetAttribute(NpgsqlRangeOperatorType operatorType) =>
                typeof(NpgsqlRangeOperatorType)
                    .GetMember(operatorType.ToString())
                    .Single()
                    .GetCustomAttribute<NpgsqlOperatorAttribute>();
        }

        /// <summary>
        /// The generic type definition for <see cref="NpgsqlRange{T}"/>.
        /// </summary>
        [NotNull] static readonly Type NpgsqlRangeType = typeof(NpgsqlRange<>);

        /// <inheritdoc />
        public override ExpressionType NodeType { get; } = ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>
        /// Gets the item to the left of the operator.
        /// </summary>
        public virtual Expression Left { get; }

        /// <summary>
        /// Gets the item to the right of the operator.
        /// </summary>
        public virtual Expression Right { get; }

        /// <summary>
        /// The operator.
        /// </summary>
        public virtual NpgsqlRangeOperatorType Operator { get; }

        /// <summary>
        /// The operator symbol.
        /// </summary>
        [NotNull]
        public virtual string OperatorSymbol => OperatorSymbols[Operator];

        /// <summary>
        /// Creates a new instance of <see cref="NpgsqlRangeOperatorExpression"/>.
        /// </summary>
        /// <param name="left">
        /// The item to the left of the operator.
        /// </param>
        /// <param name="right">
        /// The item to the right of the operator.
        /// </param>
        /// <param name="operatorType">
        /// The type of range operation.
        /// </param>
        public NpgsqlRangeOperatorExpression([NotNull] Expression left, [NotNull] Expression right, NpgsqlRangeOperatorType operatorType)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            Left = left;
            Right = right;
            Operator = operatorType;

            // TODO: will the left type arguments always be valid?
            Type type = OperatorReturnTypes[operatorType];
            Type = type.IsGenericType ? type.MakeGenericType(left.Type.GetGenericArguments()) : type;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return
                visitor is NpgsqlQuerySqlGenerator npsgqlGenerator
                    ? npsgqlGenerator.VisitRangeOperator(this)
                    : base.Accept(visitor);
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (!(visitor.Visit(Left) is Expression newLeft))
                throw new ArgumentNullException(nameof(newLeft));

            if (!(visitor.Visit(Right) is Expression newRight))
                throw new ArgumentNullException(nameof(newRight));

            return
                Left == newLeft && Right == newRight
                    ? this
                    : new NpgsqlRangeOperatorExpression(newLeft, newRight, Operator);
        }

        /// <summary>
        /// Returns a <see cref="NpgsqlRangeOperatorExpression"/> if applicable.
        /// </summary>
        /// <remarks>
        /// This returns a NpgsqlRangeOperatorExpression IFF:
        ///   1. Both left and right are <see cref="NpgsqlRange{T}"/>
        ///   2. The expression node type is one of:
        ///     - Equal
        ///     - NotEqual
        ///     - LessThan
        ///     - GreaterThan
        ///     - LessThanOrEqual
        ///     - GreaterThanOrEqual
        /// </remarks>
        /// <param name="expression">
        /// The binary expression to test.
        /// </param>
        /// <returns>
        /// A <see cref="NpgsqlRangeOperatorExpression"/> or null.
        /// </returns>
        [CanBeNull]
        public static NpgsqlRangeOperatorExpression TryVisitBinary([NotNull] BinaryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            Type leftType = expression.Left.Type;
            Type rightType = expression.Right.Type;

            if (!leftType.IsGenericType || leftType.GetGenericTypeDefinition() != NpgsqlRangeType)
                return null;
            if (!rightType.IsGenericType || rightType.GetGenericTypeDefinition() != NpgsqlRangeType)
                return null;

            return
                ExpressionOperators.TryGetValue(expression.NodeType, out NpgsqlRangeOperatorType operatorType)
                    ? new NpgsqlRangeOperatorExpression(expression.Left, expression.Right, operatorType)
                    : null;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Left} {OperatorSymbol} {Right}";

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj is NpgsqlRangeOperatorExpression other))
                return false;

            return
                Equals(Left, other.Left) &&
                Equals(Right, other.Right) &&
                NodeType == other.NodeType &&
                Operator == other.Operator &&
                Type == other.Type;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Left?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Right?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)NodeType;
                hashCode = (hashCode * 397) ^ (int)Operator;
                hashCode = (hashCode * 397) ^ Type.GetHashCode();
                return hashCode;
            }
        }
    }

    /// <summary>
    /// Indicates that a method can be translated to a PostgreSQL range operator.
    /// </summary>
    /// <remarks>
    /// This attribute allows other extension methods to hook into the range operator translations.
    /// Along with simplifying the code required to identify overloaded generics, this attribute provides
    /// a transparent way in which to transition from extension methods in the EF Core assembly to
    /// instance methods on <see cref="NpgsqlRange{T}"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class NpgsqlRangeOperatorAttribute : Attribute
    {
        /// <summary>
        /// The operator represented by the method.
        /// </summary>
        public NpgsqlRangeOperatorType OperatorType { get; }

        /// <summary>
        /// Indicates that a method can be translated to a PostgreSQL range operator.
        /// </summary>
        /// <param name="operatorType">
        /// The type of operator the method represents.
        /// </param>
        public NpgsqlRangeOperatorAttribute(NpgsqlRangeOperatorType operatorType)
        {
            OperatorType = operatorType;
        }
    }

    /// <summary>
    /// Represents information about a PostgreSQL operator.
    /// </summary>
    /// <remarks>
    /// This attribute should be applied to the members of an enum to record metadata for database translation.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class NpgsqlOperatorAttribute : Attribute
    {
        /// <summary>
        /// The operator symbol.
        /// </summary>
        [NotNull]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// The operator represented by the method.
        /// </summary>
        [NotNull]
        public Type ReturnType { get; set; } = typeof(void);
    }

    /// <summary>
    /// Describes the operator type of a range expression.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-range.html
    /// </remarks>
    public enum NpgsqlRangeOperatorType
    {
        /// <summary>
        /// No operator specified.
        /// </summary>
        [NpgsqlOperator] None,

        /// <summary>
        /// The = operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "=", ReturnType = typeof(bool))] Equal,

        /// <summary>
        /// The &lt;> operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "<>", ReturnType = typeof(bool))] NotEqual,

        /// <summary>
        /// The &lt; operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "<", ReturnType = typeof(bool))] LessThan,

        /// <summary>
        /// The > operator.
        /// </summary>
        [NpgsqlOperator(Symbol = ">", ReturnType = typeof(bool))] GreaterThan,

        /// <summary>
        /// The &lt;= operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "<=", ReturnType = typeof(bool))] LessThanOrEqual,

        /// <summary>
        /// The >= operator.
        /// </summary>
        [NpgsqlOperator(Symbol = ">=", ReturnType = typeof(bool))] GreaterThanOrEqual,

        /// <summary>
        /// The @> operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "@>", ReturnType = typeof(bool))] Contains,

        /// <summary>
        /// The &lt;@ operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "<@", ReturnType = typeof(bool))] ContainedBy,

        /// <summary>
        /// The && operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "&&", ReturnType = typeof(bool))] Overlaps,

        /// <summary>
        /// The &lt;&lt; operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "<<", ReturnType = typeof(bool))] IsStrictlyLeftOf,

        /// <summary>
        /// The >> operator.
        /// </summary>
        [NpgsqlOperator(Symbol = ">>", ReturnType = typeof(bool))] IsStrictlyRightOf,

        /// <summary>
        /// The &amp;&lt; operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "&<", ReturnType = typeof(bool))] DoesNotExtendRightOf,

        /// <summary>
        /// The &amp;&gt; operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "&>", ReturnType = typeof(bool))] DoesNotExtendLeftOf,

        /// <summary>
        /// The -|- operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "-|-", ReturnType = typeof(bool))] IsAdjacentTo,

        /// <summary>
        /// The + operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "+", ReturnType = typeof(NpgsqlRange<>))] Union,

        /// <summary>
        /// The * operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "*", ReturnType = typeof(NpgsqlRange<>))] Intersection,

        /// <summary>
        /// The - operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "-", ReturnType = typeof(NpgsqlRange<>))] Difference
    }
}
