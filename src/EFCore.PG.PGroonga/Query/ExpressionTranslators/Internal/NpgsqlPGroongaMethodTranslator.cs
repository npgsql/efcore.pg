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
        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression e)
        {
            if (e.Method.DeclaringType != typeof(NpgsqlPGroongaDbFunctionsExtensions))
                return null;

            switch (e.Method.Name)
            {
            case nameof(NpgsqlPGroongaDbFunctionsExtensions.Match):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&@", typeof(bool));

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.Query):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&@~", typeof(bool));

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.SimilarSearch):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&@*", typeof(bool));

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.ScriptQuery):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&`", typeof(bool));

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.MatchIn):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&@|", typeof(bool));

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.QueryIn):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&@~|", typeof(bool));

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PrefixSearch):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&^", typeof(bool));

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PrefixRkSearch):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&^~", typeof(bool));

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PrefixSearchIn):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&^|", typeof(bool));

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PrefixRkSearchIn):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&^~|", typeof(bool));

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.RegexpMatch):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&~", typeof(bool));

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaCommand):
                return new SqlFunctionExpression("pgroonga_command", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaCommandEscapeValue):
                return new SqlFunctionExpression("pgroonga_command_escape_value", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaEscape):
                return new SqlFunctionExpression("pgroonga_escape", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaFlush):
                return new SqlFunctionExpression("pgroonga_flush", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaHighlightHtml):
                return new SqlFunctionExpression("pgroonga_highlight_html", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaIsWritable):
                return new SqlFunctionExpression("pgroonga_is_writable", e.Method.ReturnType);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaMatchPositionsByte):
                return new SqlFunctionExpression("pgroonga_match_positions_byte", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaMatchPositionsCharacter):
                return new SqlFunctionExpression("pgroonga_match_positions_character", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaNormalize):
                return new SqlFunctionExpression("pgroonga_normalize", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaQueryEscape):
                return new SqlFunctionExpression("pgroonga_query_escape", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaQueryExpand):
                return new SqlFunctionExpression("pgroonga_query_expand", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaQueryExtractKeywords):
                return new SqlFunctionExpression("pgroonga_query_extract_keywords", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaSetWritable):
                return new SqlFunctionExpression("pgroonga_set_writable", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaScore):
                return new SqlFunctionExpression("pgroonga_score", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaSnippetHtml):
                return new SqlFunctionExpression("pgroonga_snippet_html", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaTableName):
                return new SqlFunctionExpression("pgroonga_table_name", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaWalApply):
                return new SqlFunctionExpression("pgroonga_wal_apply", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlPGroongaDbFunctionsExtensions.PgroongaWalTruncate):
                return new SqlFunctionExpression("pgroonga_wal_truncate", e.Method.ReturnType, e.Arguments);

            default:
                return null;
            }
        }
    }
}
