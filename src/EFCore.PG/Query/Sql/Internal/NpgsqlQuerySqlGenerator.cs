using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using Remotion.Linq.Clauses;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal
{
    /// <summary>
    /// The default query SQL generator for Npgsql.
    /// </summary>
    public class NpgsqlQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        /// <summary>
        /// True if null ordering is reversed; otherwise false.
        /// </summary>
        readonly bool _reverseNullOrderingEnabled;

        /// <summary>
        /// The type mapping source.
        /// </summary>
        IRelationalTypeMappingSource TypeMappingSource => Dependencies.TypeMappingSource;

        /// <inheritdoc />
        protected override string TypedTrueLiteral { get; } = "TRUE::bool";

        /// <inheritdoc />
        protected override string TypedFalseLiteral { get; } = "FALSE::bool";

        /// <inheritdoc />
        public NpgsqlQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] SelectExpression selectExpression,
            bool reverseNullOrderingEnabled)
            : base(dependencies, selectExpression)
            => _reverseNullOrderingEnabled = reverseNullOrderingEnabled;

        #region Generators

        /// <inheritdoc />
        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null)
            {
                Sql.AppendLine().Append("LIMIT ");
                Visit(selectExpression.Limit);
            }

            if (selectExpression.Offset != null)
            {
                if (selectExpression.Limit == null)
                    Sql.AppendLine();
                else
                    Sql.Append(' ');

                Sql.Append("OFFSET ");
                Visit(selectExpression.Offset);
            }
        }

        /// <summary>
        /// PostgreSQL array indexing is 1-based. If the index happens to be a constant,
        /// just increment it. Otherwise, append a +1 in the SQL.
        /// </summary>
        protected virtual Expression GenerateOneBasedIndexExpression([NotNull] Expression expression)
            => expression is ConstantExpression constantExpression
                ? Expression.Constant(Convert.ToInt32(constantExpression.Value) + 1)
                : (Expression)Expression.Add(expression, Expression.Constant(1));

        /// <inheritdoc />
        protected override string GenerateOperator(Expression expression)
        {
            switch (expression.NodeType)
            {
            case ExpressionType.Add:
                if (expression.Type == typeof(string))
                    return " || ";
                goto default;

            case ExpressionType.And:
                if (expression.Type == typeof(bool))
                    return " AND ";
                goto default;

            case ExpressionType.Or:
                if (expression.Type == typeof(bool))
                    return " OR ";
                goto default;

            default:
                return base.GenerateOperator(expression);
            }
        }

        /// <inheritdoc />
        protected override void GenerateOrdering(Ordering ordering)
        {
            base.GenerateOrdering(ordering);

            if (_reverseNullOrderingEnabled)
                Sql.Append(ordering.OrderingDirection == OrderingDirection.Asc ? " NULLS FIRST" : " NULLS LAST");
        }

        /// <inheritdoc />
        protected override void GenerateTop(SelectExpression selectExpression)
        {
            // No TOP() in PostgreSQL, see GenerateLimitOffset
        }

        #endregion

        #region Visitors

        /// <inheritdoc />
        public override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            var expr = base.VisitSqlFunction(sqlFunctionExpression);

            // Note that PostgreSQL COUNT(*) is BIGINT (64-bit). For 32-bit Count() expressions we cast.
            if (sqlFunctionExpression.FunctionName == "COUNT"
                && sqlFunctionExpression.Type == typeof(int))
            {
                Sql.Append("::INT");
                return expr;
            }

            // In PostgreSQL SUM() doesn't return the same type as its argument for smallint, int and bigint.
            // Cast to get the same type.
            // http://www.postgresql.org/docs/current/static/functions-aggregate.html
            if (sqlFunctionExpression.FunctionName == "SUM")
            {
                if (sqlFunctionExpression.Type == typeof(int))
                    Sql.Append("::INT");
                else if (sqlFunctionExpression.Type == typeof(short))
                    Sql.Append("::SMALLINT");
                return expr;
            }

            return expr;
        }

        /// <inheritdoc />
        protected override Expression VisitBinary(BinaryExpression expression)
        {
            switch (expression.NodeType)
            {
            case ExpressionType.Add:
            {
                // PostgreSQL 9.4 and below has some weird operator precedence fixed in 9.5 and described here:
                // http://git.postgresql.org/gitweb/?p=postgresql.git&a=commitdiff&h=c6b3c939b7e0f1d35f4ed4996e71420a993810d2
                // As a result we must surround string concatenation with parentheses
                if (expression.Left.Type == typeof(string) &&
                    expression.Right.Type == typeof(string))
                {
                    Sql.Append("(");
                    var exp = base.VisitBinary(expression);
                    Sql.Append(")");
                    return exp;
                }

                goto default;
            }

            case ExpressionType.ArrayIndex:
                return VisitArrayIndex(expression);

            default:
                return base.VisitBinary(expression);
            }
        }

        /// <inheritdoc />
        protected override Expression VisitDefault(DefaultExpression e)
        {
            // LOWER(range) and UPPER(range) return null on empty or infinite bounds.
            // When this happens, we need to ensure the database null is coalesced
            // back to the CLR default bound value (e.g. NpgsqlRange<int>.LowerBound is `int` not `int?`).
            var instance = e.Type.IsNullableType() ? null : Activator.CreateInstance(e.Type);

            Sql.Append(TypeMappingSource.FindMapping(e.Type).GenerateSqlLiteral(instance));

            return e;
        }

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.ArrayLength)
            {
                VisitSqlFunction(new SqlFunctionExpression("array_length", typeof(int), new[] { expression.Operand, Expression.Constant(1) }));
                return expression;
            }

            return base.VisitUnary(expression);
        }

        /// <inheritdoc />
        protected override Expression VisitIndex(IndexExpression expression)
        {
            // text cannot be subscripted.
            if (expression.Object.Type == typeof(string))
            {
                return VisitSqlFunction(
                    new SqlFunctionExpression(
                        "substr",
                        typeof(char),
                        new[] { expression.Object, GenerateOneBasedIndexExpression(expression.Arguments[0]), Expression.Constant(1) }));
            }

            Visit(expression.Object);
            for (int i = 0; i < expression.Arguments.Count; i++)
            {
                Sql.Append('[');
                Visit(GenerateOneBasedIndexExpression(expression.Arguments[i]));
                Sql.Append(']');
            }

            return expression;
        }

        /// <summary>
        /// Visits the children of an <see cref="T:ExpressionType.ArrayIndex"/> node.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        [NotNull]
        protected virtual Expression VisitArrayIndex([NotNull] BinaryExpression expression)
        {
            Debug.Assert(expression.NodeType == ExpressionType.ArrayIndex);

            // bytea cannot be subscripted, but there's get_byte.
            if (expression.Left.Type == typeof(byte[]))
            {
                return VisitSqlFunction(
                    new SqlFunctionExpression(
                        "get_byte",
                        typeof(byte),
                        new[] { expression.Left, expression.Right }));
            }

            // Regular array from here
            Visit(expression.Left);
            Sql.Append('[');
            Visit(GenerateOneBasedIndexExpression(expression.Right));
            Sql.Append(']');
            return expression;
        }

        /// <summary>
        /// Produces expressions like: 1 = ANY ('{0,1,2}') or 'cat' LIKE ANY ('{a%,b%,c%}').
        /// </summary>
        [NotNull]
        public virtual Expression VisitArrayAnyAll([NotNull] ArrayAnyAllExpression expression)
        {
            Visit(expression.Operand);
            Sql.Append(' ');
            Sql.Append(expression.Operator);
            Sql.Append(' ');
            Sql.Append(expression.ArrayComparisonType.ToString());
            Sql.Append(" (");
            Visit(expression.Array);
            Sql.Append(')');
            return expression;
        }

        /// <summary>
        /// Visits the children of a <see cref="RegexMatchExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        /// <remarks>
        /// See: http://www.postgresql.org/docs/current/static/functions-matching.html
        /// </remarks>
        [NotNull]
        public virtual Expression VisitRegexMatch([NotNull] RegexMatchExpression expression)
        {
            var options = expression.Options;

            Visit(expression.Match);
            Sql.Append(" ~ ");

            // PG regexps are single-line by default
            if (options == RegexOptions.Singleline)
            {
                Visit(expression.Pattern);
                return expression;
            }

            Sql.Append("('(?");
            if (options.HasFlag(RegexOptions.IgnoreCase))
                Sql.Append('i');

            if (options.HasFlag(RegexOptions.Multiline))
                Sql.Append('n');
            else if (!options.HasFlag(RegexOptions.Singleline))
                // In .NET's default mode, . doesn't match newlines but PostgreSQL it does.
                Sql.Append('p');

            if (options.HasFlag(RegexOptions.IgnorePatternWhitespace))
                Sql.Append('x');

            Sql.Append(")' || ");
            Visit(expression.Pattern);
            Sql.Append(')');

            return expression;
        }

        /// <summary>
        /// Visits the children of an <see cref="AtTimeZoneExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        [NotNull]
        public virtual Expression VisitAtTimeZone([NotNull] AtTimeZoneExpression expression)
        {
            Visit(expression.Timestamp);

            Sql.Append(" AT TIME ZONE '");
            Sql.Append(expression.TimeZone);
            Sql.Append('\'');

            return expression;
        }

        /// <summary>
        /// Visits the children of an <see cref="ILikeExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        [NotNull]
        public virtual Expression VisitILike([NotNull] ILikeExpression expression)
        {
            //var parentTypeMapping = _typeMapping;
            //_typeMapping = InferTypeMappingFromColumn(expression.Match) ?? parentTypeMapping;

            Visit(expression.Match);

            Sql.Append(" ILIKE ");

            Visit(expression.Pattern);

            if (expression.EscapeChar != null)
            {
                Sql.Append(" ESCAPE ");
                Visit(expression.EscapeChar);
            }

            //_typeMapping = parentTypeMapping;

            return expression;
        }

        /// <summary>
        /// Visits the children of an <see cref="ExplicitStoreTypeCastExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        [NotNull]
        public virtual Expression VisitExplicitStoreTypeCast([NotNull] ExplicitStoreTypeCastExpression expression)
        {
            Sql.Append("CAST(");

            //var parentTypeMapping = _typeMapping;
            //_typeMapping = InferTypeMappingFromColumn(expression.Operand);

            Visit(expression.Operand);

            Sql.Append(" AS ")
               .Append(expression.StoreType)
               .Append(")");

            //_typeMapping = parentTypeMapping;

            return expression;
        }

        /// <summary>
        /// Visits the children of a <see cref="CustomBinaryExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        [NotNull]
        public virtual Expression VisitCustomBinary([NotNull] CustomBinaryExpression expression)
        {
            Sql.Append('(');
            Visit(expression.Left);
            Sql.Append(' ');
            Sql.Append(expression.Operator);
            Sql.Append(' ');
            Visit(expression.Right);
            Sql.Append(')');

            return expression;
        }

        /// <summary>
        /// Visits the children of a <see cref="CustomUnaryExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        [NotNull]
        public virtual Expression VisitCustomUnary([NotNull] CustomUnaryExpression expression)
        {
            if (expression.Postfix)
            {
                Visit(expression.Operand);
                Sql.Append(expression.Operator);
            }
            else
            {
                Sql.Append(expression.Operator);
                Visit(expression.Operand);
            }

            return expression;
        }

        /// <summary>
        /// Visits the children of a <see cref="PgFunctionExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        [NotNull]
        public virtual Expression VisitPgFunction([NotNull] PgFunctionExpression expression)
        {
            //var parentTypeMapping = _typeMapping;

            //_typeMapping = null;

            var wroteSchema = false;

            if (expression.Instance != null)
            {
                Visit(expression.Instance);

                Sql.Append(".");
            }
            else if (!string.IsNullOrWhiteSpace(expression.Schema))
            {
                Sql.Append(SqlGenerator.DelimitIdentifier(expression.Schema))
                   .Append(".");

                wroteSchema = true;
            }

            Sql.Append(
                wroteSchema
                    ? SqlGenerator.DelimitIdentifier(expression.FunctionName)
                    : expression.FunctionName);

            Sql.Append("(");

            //_typeMapping = null;

            GenerateList(expression.PositionalArguments);

            bool hasArguments = expression.PositionalArguments.Count > 0 && expression.NamedArguments.Count > 0;

            foreach (var kv in expression.NamedArguments)
            {
                if (hasArguments)
                    Sql.Append(", ");
                else
                    hasArguments = true;

                Sql.Append(kv.Key)
                   .Append(" => ");

                Visit(kv.Value);
            }

            Sql.Append(")");
            //_typeMapping = parentTypeMapping;

            return expression;
        }

        #endregion
    }
}
