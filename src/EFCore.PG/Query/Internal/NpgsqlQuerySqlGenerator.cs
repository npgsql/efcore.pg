using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    /// <summary>
    /// The default query SQL generator for Npgsql.
    /// </summary>
    public class NpgsqlQuerySqlGenerator : QuerySqlGenerator
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        /// <summary>
        /// True if null ordering is reversed; otherwise false.
        /// </summary>
        private readonly bool _reverseNullOrderingEnabled;

        /// <summary>
        /// The backend version to target. If null, it means the user hasn't set a compatibility version, and the
        /// latest should be targeted.
        /// </summary>
        private readonly Version _postgresVersion;

        /// <inheritdoc />
        public NpgsqlQuerySqlGenerator(
            QuerySqlGeneratorDependencies dependencies,
            bool reverseNullOrderingEnabled,
            Version postgresVersion)
            : base(dependencies)
        {
            _sqlGenerationHelper = dependencies.SqlGenerationHelper;
            _reverseNullOrderingEnabled = reverseNullOrderingEnabled;
            _postgresVersion = postgresVersion;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression switch
            {
                PostgresAllExpression allExpression                     => VisitArrayAll(allExpression),
                PostgresAnyExpression anyExpression                     => VisitArrayAny(anyExpression),
                PostgresArrayIndexExpression arrayIndexExpression       => VisitArrayIndex(arrayIndexExpression),
                PostgresBinaryExpression binaryExpression               => VisitPostgresBinary(binaryExpression),
                PostgresFunctionExpression functionExpression           => VisitPgFunction(functionExpression),
                PostgresILikeExpression iLikeExpression                 => VisitILike(iLikeExpression),
                PostgresJsonTraversalExpression jsonTraversalExpression => VisitJsonPathTraversal(jsonTraversalExpression),
                PostgresNewArrayExpression newArrayExpression           => VisitPostgresNewArray(newArrayExpression),
                PostgresRegexMatchExpression regexMatchExpression       => VisitRegexMatch(regexMatchExpression),
                PostgresUnknownBinaryExpression unknownBinaryExpression => VisitUnknownBinary(unknownBinaryExpression),
                _                                                       => base.VisitExtension(extensionExpression)
            };

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
                    Sql.Append(" ");

                Sql.Append("OFFSET ");
                Visit(selectExpression.Offset);
            }
        }

        /// <inheritdoc />
        protected override string GetOperator(SqlBinaryExpression e)
            => e.OperatorType switch
            {
                // PostgreSQL has a special string concatenation operator: ||
                // We switch to it if the expression itself has type string, or if one of the sides has a
                // string type mapping. Same for full-text search's TsVector.
                ExpressionType.Add when
                    e.Type == typeof(string) || e.Left.TypeMapping?.ClrType == typeof(string) || e.Right.TypeMapping?.ClrType == typeof(string) ||
                    e.Type == typeof(NpgsqlTsVector) || e.Left.TypeMapping?.ClrType == typeof(NpgsqlTsVector) || e.Right.TypeMapping?.ClrType == typeof(NpgsqlTsVector)
                    => " || ",
                ExpressionType.And when e.Type == typeof(bool)   => " AND ",
                ExpressionType.Or  when e.Type == typeof(bool)   => " OR ",
                _ => base.GetOperator(e)
            };

        /// <inheritdoc />
        protected override Expression VisitOrdering(OrderingExpression ordering)
        {
            var result = base.VisitOrdering(ordering);

            if (_reverseNullOrderingEnabled)
                Sql.Append(ordering.IsAscending ? " NULLS FIRST" : " NULLS LAST");

            return result;
        }

        /// <inheritdoc />
        protected override void GenerateTop(SelectExpression selectExpression)
        {
            // No TOP() in PostgreSQL, see GenerateLimitOffset
        }

        protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        {
            Sql.Append("JOIN LATERAL ");

            if (crossApplyExpression.Table is TableExpression table)
            {
                // PostgreSQL doesn't support LATERAL JOIN over table, and it doesn't really make sense to do it - but EF Core
                // will sometimes generate that. #1560
                Sql
                    .Append("(SELECT * FROM ")
                    .Append(_sqlGenerationHelper.DelimitIdentifier(table.Name, table.Schema))
                    .Append(")")
                    .Append(AliasSeparator)
                    .Append(_sqlGenerationHelper.DelimitIdentifier(table.Alias));
            }
            else
                Visit(crossApplyExpression.Table);

            Sql.Append(" ON TRUE");
            return crossApplyExpression;
        }

        protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        {
            Sql.Append("LEFT JOIN LATERAL ");

            if (outerApplyExpression.Table is TableExpression table)
            {
                // PostgreSQL doesn't support LATERAL JOIN over table, and it doesn't really make sense to do it - but EF Core
                // will sometimes generate that. #1560
                Sql
                    .Append("(SELECT * FROM ")
                    .Append(_sqlGenerationHelper.DelimitIdentifier(table.Name, table.Schema))
                    .Append(")")
                    .Append(AliasSeparator)
                    .Append(_sqlGenerationHelper.DelimitIdentifier(table.Alias));
            }
            else
                Visit(outerApplyExpression.Table);

            Sql.Append(" ON TRUE");
            return outerApplyExpression;
        }

        /// <inheritdoc />
        protected override void CheckComposableSql(string sql)
        {
            // PostgreSQL supports CTE (WITH) expressions within subqueries, as well as INSERT ... RETURNING ...,
            // so we allow any raw SQL to be composed over. PostgreSQL will error if it doesn't work.
        }

        /// <inheritdoc />
        protected override Expression VisitSqlBinary(SqlBinaryExpression binary)
        {
            switch (binary.OperatorType)
            {
            case ExpressionType.Add:
            {
                if (_postgresVersion >= new Version(9, 5))
                    return base.VisitSqlBinary(binary);

                // PostgreSQL 9.4 and below has some weird operator precedence fixed in 9.5 and described here:
                // http://git.postgresql.org/gitweb/?p=postgresql.git&a=commitdiff&h=c6b3c939b7e0f1d35f4ed4996e71420a993810d2
                // As a result we must surround string concatenation with parentheses
                if (binary.Left.Type == typeof(string) &&
                    binary.Right.Type == typeof(string))
                {
                    Sql.Append("(");
                    var exp = base.VisitSqlBinary(binary);
                    Sql.Append(")");
                    return exp;
                }

                return base.VisitSqlBinary(binary);
            }

            case ExpressionType.ArrayIndex:
                return VisitArrayIndex(binary);

            default:
                return base.VisitSqlBinary(binary);
            }
        }

        protected virtual Expression VisitPostgresNewArray(PostgresNewArrayExpression postgresNewArrayExpression)
        {
            Debug.Assert(postgresNewArrayExpression.TypeMapping is not null);

            Sql.Append("ARRAY[");
            var first = true;
            foreach (var initializer in postgresNewArrayExpression.Expressions)
            {
                if (!first)
                {
                    Sql.Append(",");
                }

                first = false;
                Visit(initializer);
            }

            // Not sure if the explicit store type is necessary, but just to be sure.
            Sql
                .Append("]::")
                .Append(postgresNewArrayExpression.TypeMapping.StoreType);

            return postgresNewArrayExpression;
        }

        protected virtual Expression VisitPostgresBinary(PostgresBinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            var requiresBrackets = RequiresBrackets(binaryExpression.Left);

            if (requiresBrackets)
                Sql.Append("(");

            Visit(binaryExpression.Left);

            if (requiresBrackets)
                Sql.Append(")");

            Debug.Assert(binaryExpression.Left?.TypeMapping is not null);
            Debug.Assert(binaryExpression.Right?.TypeMapping is not null);

            Sql
                .Append(" ")
                .Append(binaryExpression.OperatorType switch
                {
                    PostgresExpressionType.Contains
                    when binaryExpression.Left.TypeMapping is NpgsqlInetTypeMapping ||
                         binaryExpression.Left.TypeMapping is NpgsqlCidrTypeMapping
                    => ">>",

                    PostgresExpressionType.ContainedBy
                    when binaryExpression.Left.TypeMapping is NpgsqlInetTypeMapping ||
                         binaryExpression.Left.TypeMapping is NpgsqlCidrTypeMapping
                    => "<<",

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

                    PostgresExpressionType.LTreeMatches
                        when binaryExpression.Right.TypeMapping.StoreType == "lquery" ||
                             binaryExpression.Right.TypeMapping is NpgsqlArrayTypeMapping arrayMapping &&
                             arrayMapping.ElementMapping.StoreType == "lquery"
                        => "~",
                    PostgresExpressionType.LTreeMatches
                        when binaryExpression.Right.TypeMapping.StoreType == "ltxtquery"
                        => "@",
                    PostgresExpressionType.LTreeMatchesAny      => "?",
                    PostgresExpressionType.LTreeFirstAncestor   => "?@>",
                    PostgresExpressionType.LTreeFirstDescendent => "?<@",
                    PostgresExpressionType.LTreeFirstMatches
                        when binaryExpression.Right.TypeMapping.StoreType == "lquery" => "?~",
                    PostgresExpressionType.LTreeFirstMatches
                        when binaryExpression.Right.TypeMapping.StoreType == "ltxtquery" => "?@",

                    PostgresExpressionType.PostgisDistanceKnn => "<->",

                    _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {binaryExpression.OperatorType}")
                })
                .Append(" ");

            requiresBrackets = RequiresBrackets(binaryExpression.Right);

            if (requiresBrackets)
                Sql.Append("(");

            Visit(binaryExpression.Right);

            if (requiresBrackets)
                Sql.Append(")");

            return binaryExpression;
        }

        protected virtual Expression VisitArrayIndex(SqlBinaryExpression expression)
        {
            Visit(expression.Left);
            Sql.Append("[");
            Visit(expression.Right);
            Sql.Append("]");
            return expression;
        }

        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        {
            Debug.Assert(sqlUnaryExpression.TypeMapping is not null);
            Debug.Assert(sqlUnaryExpression.Operand.TypeMapping is not null);

            switch (sqlUnaryExpression.OperatorType)
            {
            case ExpressionType.Convert:
            {
                // PostgreSQL supports the standard CAST(x AS y), but also a lighter x::y which we use
                // where there's no precedence issues
                switch (sqlUnaryExpression.Operand)
                {
                case SqlConstantExpression:
                case SqlParameterExpression:
                case SqlUnaryExpression { OperatorType: ExpressionType.Convert }:
                case ColumnExpression:
                case SqlFunctionExpression:
                case ScalarSubqueryExpression:
                    var storeType = sqlUnaryExpression.TypeMapping.StoreType switch
                    {
                        "integer" => "INT",
                        "timestamp with time zone" => "timestamptz",
                        "timestamp without time zone" => "timestamp",
                        var s => s
                    };

                    Visit(sqlUnaryExpression.Operand);
                    Sql.Append("::");
                    Sql.Append(storeType);
                    return sqlUnaryExpression;
                }

                break;
            }

            // Bitwise complement on networking types
            case ExpressionType.Not when
                sqlUnaryExpression.Operand.TypeMapping.ClrType == typeof(IPAddress) ||
                sqlUnaryExpression.Operand.TypeMapping.ClrType == typeof((IPAddress, int)) ||
                sqlUnaryExpression.Operand.TypeMapping.ClrType == typeof(PhysicalAddress):
                Sql.Append("~");
                Visit(sqlUnaryExpression.Operand);
                return sqlUnaryExpression;

            // Not operation on full-text queries
            case ExpressionType.Not when sqlUnaryExpression.Operand.TypeMapping.ClrType == typeof(NpgsqlTsQuery):
                Sql.Append("!!");
                Visit(sqlUnaryExpression.Operand);
                return sqlUnaryExpression;

            // EF uses unary Equal and NotEqual to represent is-null checking.
            // These need to be surrounded in parentheses in various cases (e.g. where TRUE = x IS NOT NULL
            case ExpressionType.Equal:
                Sql.Append("(");
                Visit(sqlUnaryExpression.Operand);
                Sql.Append(" IS NULL)");
                return sqlUnaryExpression;

            case ExpressionType.NotEqual:
                Sql.Append("(");
                Visit(sqlUnaryExpression.Operand);
                Sql.Append(" IS NOT NULL)");
                return sqlUnaryExpression;
            }

            return base.VisitSqlUnary(sqlUnaryExpression);
        }

        protected override void GenerateSetOperationOperand(SetOperationBase setOperation, SelectExpression operand)
        {
            // PostgreSQL allows ORDER BY and LIMIT in set operation operands, but requires parentheses
            if (operand.Orderings.Count > 0 || operand.Limit != null)
            {
                Sql.AppendLine("(");
                using (Sql.Indent())
                {
                    Visit(operand);
                }
                Sql.AppendLine().Append(")");
                return;
            }

            base.GenerateSetOperationOperand(setOperation, operand);
        }

        protected override Expression VisitCollate(CollateExpression collateExpresion)
        {
            Check.NotNull(collateExpresion, nameof(collateExpresion));

            Visit(collateExpresion.Operand);

            Sql
                .Append(" COLLATE ")
                .Append(_sqlGenerationHelper.DelimitIdentifier(collateExpresion.Collation));

            return collateExpresion;
        }

        public virtual Expression VisitArrayAll(PostgresAllExpression expression)
        {
            Visit(expression.Item);

            Sql
                .Append(" ")
                .Append(expression.OperatorType switch
                {
                    PostgresAllOperatorType.Like => "LIKE",
                    PostgresAllOperatorType.ILike => "ILIKE",
                    _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {expression.OperatorType}")
                })
                .Append(" ALL (");

            Visit(expression.Array);

            Sql.Append(")");

            return expression;
        }

        public virtual Expression VisitArrayAny(PostgresAnyExpression expression)
        {
            Visit(expression.Item);

            Sql
                .Append(" ")
                .Append(expression.OperatorType switch
                {
                    PostgresAnyOperatorType.Equal => "=",
                    PostgresAnyOperatorType.Like => "LIKE",
                    PostgresAnyOperatorType.ILike => "ILIKE",
                    _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {expression.OperatorType}")
                })
                .Append(" ANY (");

            Visit(expression.Array);

            Sql.Append(")");

            return expression;
        }

        /// <summary>
        /// Produces SQL array index expression (e.g. arr[1]).
        /// </summary>
        public virtual Expression VisitArrayIndex(PostgresArrayIndexExpression expression)
        {
            Visit(expression.Array);
            Sql.Append("[");
            Visit(expression.Index);
            Sql.Append("]");
            return expression;
        }

        /// <summary>
        /// Visits the children of a <see cref="PostgresRegexMatchExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        /// <remarks>
        /// See: http://www.postgresql.org/docs/current/static/functions-matching.html
        /// </remarks>
        public virtual Expression VisitRegexMatch(PostgresRegexMatchExpression expression)
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
                Sql.Append("i");

            if (options.HasFlag(RegexOptions.Multiline))
                Sql.Append("n");
            else if (!options.HasFlag(RegexOptions.Singleline))
                // In .NET's default mode, . doesn't match newlines but PostgreSQL it does.
                Sql.Append("p");

            if (options.HasFlag(RegexOptions.IgnorePatternWhitespace))
                Sql.Append("x");

            Sql.Append(")' || ");
            Visit(expression.Pattern);
            Sql.Append(")");

            return expression;
        }

        /// <summary>
        /// Visits the children of an <see cref="PostgresILikeExpression"/>.
        /// </summary>
        /// <param name="likeExpression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        public virtual Expression VisitILike(PostgresILikeExpression likeExpression)
        {
            Visit(likeExpression.Match);
            Sql.Append(" ILIKE ");
            Visit(likeExpression.Pattern);

            if (likeExpression.EscapeChar != null)
            {
                Sql.Append(" ESCAPE ");
                Visit(likeExpression.EscapeChar);
            }

            return likeExpression;
        }

        /// <summary>
        /// Visits the children of an <see cref="PostgresJsonTraversalExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        public virtual Expression VisitJsonPathTraversal(PostgresJsonTraversalExpression expression)
        {
            Visit(expression.Expression);

            if (expression.Path.Count == 1)
            {
                Sql.Append(expression.ReturnsText ? "->>" : "->");
                Visit(expression.Path[0]);
                return expression;
            }

            // Multiple path components
            Sql.Append(expression.ReturnsText ? "#>>" : "#>");

            // Use simplified array literal syntax if all path components are constants for cleaner SQL
            if (expression.Path.All(p => p is SqlConstantExpression))
            {
                Sql
                    .Append("'{")
                    .Append(string.Join(",", expression.Path.Select(p => ((SqlConstantExpression)p).Value)))
                    .Append("}'");
            }
            else
            {
                Sql.Append("ARRAY[");
                for (var i = 0; i < expression.Path.Count; i++)
                {
                    Visit(expression.Path[i]);
                    if (i < expression.Path.Count - 1)
                        Sql.Append(",");
                }
                Sql.Append("]::TEXT[]");
            }

            return expression;
        }

        /// <summary>
        /// Visits the children of a <see cref="PostgresUnknownBinaryExpression"/>.
        /// </summary>
        /// <param name="unknownBinaryExpression">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        public virtual Expression VisitUnknownBinary(PostgresUnknownBinaryExpression unknownBinaryExpression)
        {
            Check.NotNull(unknownBinaryExpression, nameof(unknownBinaryExpression));

            var requiresBrackets = RequiresBrackets(unknownBinaryExpression.Left);

            if (requiresBrackets)
                Sql.Append("(");

            Visit(unknownBinaryExpression.Left);

            if (requiresBrackets)
                Sql.Append(")");

            Sql
                .Append(" ")
                .Append(unknownBinaryExpression.Operator)
                .Append(" ");

            requiresBrackets = RequiresBrackets(unknownBinaryExpression.Right);

            if (requiresBrackets)
                Sql.Append("(");

            Visit(unknownBinaryExpression.Right);

            if (requiresBrackets)
                Sql.Append(")");

            return unknownBinaryExpression;
        }


        /// <inheritdoc />
//        protected override Expression VisitDefault(DefaultExpression e)
//        {
//            // LOWER(range) and UPPER(range) return null on empty or infinite bounds.
//            // When this happens, we need to ensure the database null is coalesced
//            // back to the CLR default bound value (e.g. NpgsqlRange<int>.LowerBound is `int` not `int?`).
//            var instance = e.Type.IsNullableType() ? null : Activator.CreateInstance(e.Type);
//
//            Sql.Append(TypeMappingSource.FindMapping(e.Type).GenerateSqlLiteral(instance));
//
//            return e;
//        }

        /// <summary>
        /// Visits the children of a <see cref="PostgresFunctionExpression"/>.
        /// </summary>
        /// <param name="e">The expression.</param>
        /// <returns>
        /// An <see cref="Expression"/>.
        /// </returns>
        public virtual Expression VisitPgFunction(PostgresFunctionExpression e)
        {
            Check.NotNull(e, nameof(e));

            if (e.IsBuiltIn)
            {
                Sql.Append(e.Name);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.Schema))
                {
                    Sql
                        .Append(_sqlGenerationHelper.DelimitIdentifier(e.Schema))
                        .Append(".");
                }

                // TODO: Quote user-defined function names with upper-case (also for regular SqlFunctionExpression)

                Sql.Append(_sqlGenerationHelper.DelimitIdentifier(e.Name));
            }

            Sql.Append("(");

            for (var i = 0; i < e.Arguments.Count; i++)
            {
                if (i < e.ArgumentNames.Count && e.ArgumentNames[i] is string argumentName)
                {
                    Sql
                        .Append(argumentName)
                        .Append(" => ");
                }

                Visit(e.Arguments[i]);

                if (i < e.Arguments.Count - 1)
                    Sql.Append(i < e.ArgumentSeparators.Count && e.ArgumentSeparators[i] != null
                        ? $" {e.ArgumentSeparators[i]} "
                        : ", ");
            }

            Sql.Append(")");

            return e;
        }

        private static bool RequiresBrackets(SqlExpression expression)
            => expression is SqlBinaryExpression || expression is LikeExpression || expression is PostgresBinaryExpression;
    }
}
