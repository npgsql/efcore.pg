using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translations for PGroonga full-text search methods.
    /// </summary>
    public class NpgsqlPGroongaMethodTranslator : IMethodCallTranslator
    {
        static readonly IReadOnlyDictionary<string, string> SqlNameByMethodName = new Dictionary<string, string>
        {
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaCommand)] = "pgroonga_command",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaCommandEscapeValue)] = "pgroonga_command_escape_value",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaEscape)] = "pgroonga_escape",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaFlush)] = "pgroonga_flush",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaHighlightHtml)] = "pgroonga_highlight_html",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaIsWritable)] = "pgroonga_is_writable",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaMatchPositionsByte)] = "pgroonga_match_positions_byte",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaMatchPositionsCharacter)] = "pgroonga_match_positions_character",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaNormalize)] = "pgroonga_normalize",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaQueryEscape)] = "pgroonga_query_escape",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaQueryExpand)] = "pgroonga_query_expand",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaQueryExtractKeywords)] = "pgroonga_query_extract_keywords",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaSetWritable)] = "pgroonga_set_writable",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaScore)] = "pgroonga_score",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaSnippetHtml)] = "pgroonga_snippet_html",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaTableName)] = "pgroonga_table_name",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaWalApply)] = "pgroonga_wal_apply",
            [nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaWalTruncate)] = "pgroonga_wal_truncate"
        };

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression e)
        {
            if (e.Method.DeclaringType != typeof(NpgsqlPGroongaDbFunctionsExtensions) &&
                e.Method.DeclaringType != typeof(NpgsqlPGroongaLinqExtensions))
                return null;

            if (SqlNameByMethodName.TryGetValue(e.Method.Name, out var sqlFunctionName))
            {
                if (sqlFunctionName == "pgroonga_score")
                {
                    var column = e.Arguments[1] as ColumnExpression;
                    return new SqlFunctionExpression(sqlFunctionName, e.Method.ReturnType, new[]
                    {
                        new ColumnExpression("tableoid", column.Property, column.Table),
                        new ColumnExpression("ctid", column.Property, column.Table)
                    });
                }

                return new SqlFunctionExpression(sqlFunctionName, e.Method.ReturnType, e.Arguments.Skip(1));
            }

            return TryTranslateOperator(e);
        }

        [CanBeNull]
        static Expression TryTranslateOperator([NotNull] MethodCallExpression e)
        {
            switch (e.Method.Name)
            {
            case nameof(NpgsqlPGroongaLinqExtensions.Match):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&@", typeof(bool));

            case nameof(NpgsqlPGroongaLinqExtensions.Query):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&@~", typeof(bool));

            case nameof(NpgsqlPGroongaLinqExtensions.SimilarSearch):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&@*", typeof(bool));

            case nameof(NpgsqlPGroongaLinqExtensions.ScriptQuery):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&`", typeof(bool));

            case nameof(NpgsqlPGroongaLinqExtensions.MatchIn):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&@|", typeof(bool));

            case nameof(NpgsqlPGroongaLinqExtensions.QueryIn):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&@~|", typeof(bool));

            case nameof(NpgsqlPGroongaLinqExtensions.PrefixSearch):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&^", typeof(bool));

            case nameof(NpgsqlPGroongaLinqExtensions.PrefixRkSearch):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&^~", typeof(bool));

            case nameof(NpgsqlPGroongaLinqExtensions.PrefixSearchIn):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&^|", typeof(bool));

            case nameof(NpgsqlPGroongaLinqExtensions.PrefixRkSearchIn):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&^~|", typeof(bool));

            case nameof(NpgsqlPGroongaLinqExtensions.RegexpMatch):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&~", typeof(bool));

            default:
                return null;
            }
        }
    }
}
