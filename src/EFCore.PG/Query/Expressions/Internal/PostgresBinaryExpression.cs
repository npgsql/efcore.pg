using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// An expression that represents a PostgreSQL-specific binary operation in a SQL tree.
    /// </summary>
    public class PostgresBinaryExpression : SqlExpression
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PostgresBinaryExpression" /> class.
        /// </summary>
        /// <param name="operatorType">The operator to apply.</param>
        /// <param name="left">An expression which is left operand.</param>
        /// <param name="right">An expression which is right operand.</param>
        /// <param name="type">The <see cref="Type"/> of the expression.</param>
        /// <param name="typeMapping">The <see cref="RelationalTypeMapping"/> associated with the expression.</param>
        public PostgresBinaryExpression(
            PostgresExpressionType operatorType,
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            OperatorType = operatorType;
            Left = left;
            Right = right;
        }

        /// <summary>
        /// The operator of this PostgreSQL binary operation.
        /// </summary>
        public virtual PostgresExpressionType OperatorType { get; }
        /// <summary>
        /// The left operand.
        /// </summary>
        public virtual SqlExpression Left { get; }
        /// <summary>
        /// The right operand.
        /// </summary>
        public virtual SqlExpression Right { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var left = (SqlExpression)visitor.Visit(Left);
            var right = (SqlExpression)visitor.Visit(Right);

            return Update(left, right);
        }

        /// <summary>
        /// Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        /// return this expression.
        /// </summary>
        /// <param name="left">The <see cref="Left"/> property of the result.</param>
        /// <param name="right">The <see cref="Right"/> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public virtual PostgresBinaryExpression Update([NotNull] SqlExpression left, [NotNull] SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return left != Left || right != Right
                ? new PostgresBinaryExpression(OperatorType, left, right, Type, TypeMapping)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            var requiresBrackets = RequiresBrackets(Left);

            if (requiresBrackets)
            {
                expressionPrinter.Append("(");
            }

            expressionPrinter.Visit(Left);

            if (requiresBrackets)
            {
                expressionPrinter.Append(")");
            }

            expressionPrinter
                .Append(" ")
                .Append(OperatorType switch
                {
                    PostgresExpressionType.Contains    => "@>",
                    PostgresExpressionType.ContainedBy => "<@",
                    PostgresExpressionType.Overlaps    => "&&",

                    PostgresExpressionType.AtTimeZone => "AT TIME ZONE",

                    PostgresExpressionType.NetworkContainedByOrEqual    => "<<=",
                    PostgresExpressionType.NetworkContainsOrEqual       => ">>=",
                    PostgresExpressionType.NetworkContainsOrContainedBy => "&&",

                    PostgresExpressionType.RangeIsStrictlyLeftOf     => "<<",
                    PostgresExpressionType.RangeIsStrictlyRightOf    => ">>",
                    PostgresExpressionType.RangeDoesNotExtendRightOf => "&<",
                    PostgresExpressionType.RangeDoesNotExtendLeftOf  => "&>",
                    PostgresExpressionType.RangeIsAdjacentTo         => "-|-",
                    PostgresExpressionType.RangeUnion                => "+",
                    PostgresExpressionType.RangeIntersect            => "*",
                    PostgresExpressionType.RangeExcept               => "-",

                    PostgresExpressionType.TextSearchMatch => "@@",
                    PostgresExpressionType.TextSearchAnd   => "&&",
                    PostgresExpressionType.TextSearchOr    => "||",

                    PostgresExpressionType.JsonExists    => "?",
                    PostgresExpressionType.JsonExistsAny => "?|",
                    PostgresExpressionType.JsonExistsAll => "?&",

                    _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {OperatorType}")
                })
                .Append(" ");

            requiresBrackets = RequiresBrackets(Right);

            if (requiresBrackets)
            {
                expressionPrinter.Append("(");
            }

            expressionPrinter.Visit(Right);

            if (requiresBrackets)
            {
                expressionPrinter.Append(")");
            }

            static bool RequiresBrackets(SqlExpression expression) => expression is PostgresBinaryExpression || expression is LikeExpression;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is PostgresBinaryExpression sqlBinaryExpression
                    && Equals(sqlBinaryExpression));

        bool Equals(PostgresBinaryExpression sqlBinaryExpression)
            => base.Equals(sqlBinaryExpression)
                && OperatorType == sqlBinaryExpression.OperatorType
                && Left.Equals(sqlBinaryExpression.Left)
                && Right.Equals(sqlBinaryExpression.Right);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), OperatorType, Left, Right);
    }
}
