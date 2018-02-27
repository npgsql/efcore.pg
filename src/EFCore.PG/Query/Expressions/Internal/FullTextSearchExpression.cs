using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    public class FullTextSearchExpression : Expression
    {
        private FullTextSearchExpression(
            string binaryOperator,
            [NotNull] Expression left,
            [CanBeNull] Expression right,
            [NotNull] Type type)
        {
            Check.NotEmpty(binaryOperator, nameof(binaryOperator));
            Check.NotNull(left, nameof(left));
            Check.NotNull(type, nameof(type));

            Operator = binaryOperator;
            Left = left;
            Right = right;
            Type = type;
        }

        public string Operator { get; }
        public Expression Left { get; }
        public Expression Right { get; }
        public override Type Type { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitFullTextSearch(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newLeft = visitor.Visit(Left);
            var newRight = Right != null ? visitor.Visit(Right) : null;

            return newLeft != Left
                   || newRight != Right
                ? new FullTextSearchExpression(Operator, newLeft, newRight, Type)
                : this;
        }

        public override string ToString() => Right != null
            ? $"{Left} {Operator} {Right}"
            : $"{Operator} {Left}";

        public static FullTextSearchExpression TsQueryAnd([NotNull] Expression left, [NotNull] Expression right)
        {
            ValidateArguments(left, typeof(NpgsqlTsQuery), right, typeof(NpgsqlTsQuery));
            return new FullTextSearchExpression("&&", left, right, typeof(NpgsqlTsQuery));
        }

        public static FullTextSearchExpression TsQueryOr([NotNull] Expression left, [NotNull] Expression right)
        {
            ValidateArguments(left, typeof(NpgsqlTsQuery), right, typeof(NpgsqlTsQuery));
            return new FullTextSearchExpression("||", left, right, typeof(NpgsqlTsQuery));
        }

        public static FullTextSearchExpression TsQueryNegate([NotNull] Expression expression)
        {
            ValidateArguments(expression, typeof(NpgsqlTsQuery), null);
            return new FullTextSearchExpression("!!", expression, null, typeof(NpgsqlTsQuery));
        }

        public static FullTextSearchExpression TsQueryContains([NotNull] Expression left, [NotNull] Expression right)
        {
            ValidateArguments(left, typeof(NpgsqlTsQuery), right, typeof(NpgsqlTsQuery));
            return new FullTextSearchExpression("@>", left, right, typeof(bool));
        }

        public static FullTextSearchExpression TsQueryIsContainedIn([NotNull] Expression left, [NotNull] Expression right)
        {
            ValidateArguments(left, typeof(NpgsqlTsQuery), right, typeof(NpgsqlTsQuery));
            return new FullTextSearchExpression("<@", left, right, typeof(bool));
        }

        public static FullTextSearchExpression TsVectorMatches([NotNull] Expression left, [NotNull] Expression right)
        {
            ValidateArguments(left, typeof(NpgsqlTsVector), right, typeof(NpgsqlTsQuery), typeof(string));
            return new FullTextSearchExpression("@@", left, right, typeof(bool));
        }

        private static void ValidateArguments(
            [NotNull] Expression left,
            [NotNull] Type validLeftType,
            Expression right,
            params Type[] validRightTypes)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(validLeftType, nameof(validLeftType));

            if (left.Type != validLeftType)
                throw new ArgumentException(
                    $"Expression must be of type {validLeftType.FullName}",
                    nameof(left));

            if (validRightTypes == null || validRightTypes.Length == 0) return;

            Check.NotNull(right, nameof(right));
            if (validRightTypes.All(x => right.Type != x))
                throw new ArgumentException(
                    $"Expression must be of one of types: {string.Join(", ", validRightTypes.Select(x => x.FullName))}",
                    nameof(right));
        }
    }
}
