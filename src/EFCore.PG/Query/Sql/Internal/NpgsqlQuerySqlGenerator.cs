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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Sql.Internal
{
    public class NpgsqlQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        readonly bool _nullFirstOrderingEnabled;

        protected override string TypedTrueLiteral => "TRUE::bool";
        protected override string TypedFalseLiteral => "FALSE::bool";

        public NpgsqlQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] SelectExpression selectExpression,
            bool nullFirstOrderingEnabledEnabled)
            : base(dependencies, selectExpression)
        {
            _nullFirstOrderingEnabled = nullFirstOrderingEnabledEnabled;
        }

        protected override void GenerateTop(SelectExpression selectExpression)
        {
            // No TOP() in PostgreSQL, see GenerateLimitOffset
        }

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
                if (selectExpression.Limit == null) {
                    Sql.AppendLine();
                } else {
                    Sql.Append(' ');
                }
                Sql.Append("OFFSET ");
                Visit(selectExpression.Offset);
            }
        }

        public override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            var expr = base.VisitSqlFunction(sqlFunctionExpression);

            // Note that PostgreSQL COUNT(*) is BIGINT (64-bit). For 32-bit Count() expressions we cast.
            if (sqlFunctionExpression.FunctionName == "COUNT"
                && sqlFunctionExpression.Type == typeof(int))
            {
                Sql.Append("::INT4");
                return expr;
            }

            // In PostgreSQL SUM() doesn't return the same type as its argument for smallint, int and bigint.
            // Cast to get the same type.
            // http://www.postgresql.org/docs/current/static/functions-aggregate.html
            if (sqlFunctionExpression.FunctionName == "SUM")
            {
                if (sqlFunctionExpression.Type == typeof(int))
                    Sql.Append("::INT4");
                else if (sqlFunctionExpression.Type == typeof(short))
                    Sql.Append("::INT2");
                return expr;
            }

            return expr;
        }

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
                break;
            }

            case ExpressionType.ArrayIndex:
                GenerateArrayIndex(expression);
                return expression;
            }

            return base.VisitBinary(expression);
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.ArrayLength)
            {
                VisitSqlFunction(new SqlFunctionExpression("array_length", typeof(int), new[] { expression.Operand, Expression.Constant(1) }));
                return expression;
            }

            return base.VisitUnary(expression);
        }

        void GenerateArrayIndex([NotNull] BinaryExpression expression)
        {
            Debug.Assert(expression.NodeType == ExpressionType.ArrayIndex);

            if (expression.Left.Type == typeof(byte[]))
            {
                // bytea cannot be subscripted, but there's get_byte
                VisitSqlFunction(new SqlFunctionExpression("get_byte", typeof(byte),
                    new[] { expression.Left, expression.Right }));
                return;
            }

            if (expression.Left.Type == typeof(string))
            {
                // text cannot be subscripted, use substr
                // PostgreSQL substr() is 1-based.

                VisitSqlFunction(new SqlFunctionExpression("substr", typeof(char),
                    new[] { expression.Left, expression.Right, Expression.Constant(1) }));
                return;
            }

            // Regular array from here
            Visit(expression.Left);
            Sql.Append('[');
            Visit(GenerateOneBasedIndexExpression(expression.Right));
            Sql.Append(']');
        }

        public Expression VisitArrayAny(ArrayAnyExpression arrayAnyExpression)
        {
            Visit(arrayAnyExpression.Operand);
            Sql.Append(" = ANY (");
            Visit(arrayAnyExpression.Array);
            Sql.Append(")");
            return arrayAnyExpression;
        }

        // PostgreSQL array indexing is 1-based. If the index happens to be a constant,
        // just increment it. Otherwise, append a +1 in the SQL.
        Expression GenerateOneBasedIndexExpression(Expression expression)
            => expression is ConstantExpression constantExpression
                ? Expression.Constant(Convert.ToInt32(constantExpression.Value) + 1)
                : (Expression)Expression.Add(expression, Expression.Constant(1));

        // See http://www.postgresql.org/docs/current/static/functions-matching.html
        public Expression VisitRegexMatch([NotNull] RegexMatchExpression regexMatchExpression)
        {
            Check.NotNull(regexMatchExpression, nameof(regexMatchExpression));
            var options = regexMatchExpression.Options;

            Visit(regexMatchExpression.Match);
            Sql.Append(" ~ ");

            // PG regexps are singleline by default
            if (options == RegexOptions.Singleline)
            {
                Visit(regexMatchExpression.Pattern);
                return regexMatchExpression;
            }

            Sql.Append("('(?");
            if (options.HasFlag(RegexOptions.IgnoreCase)) {
                Sql.Append('i');
            }

            if (options.HasFlag(RegexOptions.Multiline)) {
                Sql.Append('n');
            }
            else if (!options.HasFlag(RegexOptions.Singleline)) {
                // In .NET's default mode, . doesn't match newlines but PostgreSQL it does.
                Sql.Append('p');
            }

            if (options.HasFlag(RegexOptions.IgnorePatternWhitespace))
            {
                Sql.Append('x');
            }

            Sql.Append(")' || ");
            Visit(regexMatchExpression.Pattern);
            Sql.Append(')');

            return regexMatchExpression;
        }

        public Expression VisitAtTimeZone([NotNull] AtTimeZoneExpression atTimeZoneExpression)
        {
            Check.NotNull(atTimeZoneExpression, nameof(atTimeZoneExpression));

            Visit(atTimeZoneExpression.TimestampExpression);

            Sql.Append(" AT TIME ZONE '");
            Sql.Append(atTimeZoneExpression.TimeZone);
            Sql.Append('\'');

            return atTimeZoneExpression;
        }

        public virtual Expression VisitILike(ILikeExpression iLikeExpression)
        {
            Check.NotNull(iLikeExpression, nameof(iLikeExpression));

            //var parentTypeMapping = _typeMapping;
            //_typeMapping = InferTypeMappingFromColumn(iLikeExpression.Match) ?? parentTypeMapping;

            Visit(iLikeExpression.Match);

            Sql.Append(" ILIKE ");

            Visit(iLikeExpression.Pattern);

            if (iLikeExpression.EscapeChar != null)
            {
                Sql.Append(" ESCAPE ");
                Visit(iLikeExpression.EscapeChar);
            }

            //_typeMapping = parentTypeMapping;

            return iLikeExpression;
        }

        public Expression VisitExplicitStoreTypeCast([NotNull] ExplicitStoreTypeCastExpression castExpression)
        {
            Sql.Append("CAST(");

            //var parentTypeMapping = _typeMapping;
            //_typeMapping = InferTypeMappingFromColumn(castExpression.Operand);

            Visit(castExpression.Operand);

            Sql
                .Append(" AS ")
                .Append(castExpression.StoreType)
                .Append(")");

            //_typeMapping = parentTypeMapping;

            return castExpression;
        }

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

        protected override void GenerateOrdering([NotNull] Ordering ordering)
        {
            base.GenerateOrdering(ordering);
            if (_nullFirstOrderingEnabled)
                Sql.Append(" NULLS FIRST");
        }
    }
}
