using System;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlPGroongaDbFunctionsExtensions
    {
        /// <summary>
        /// Executes a Groonga command
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="command">A Groonga command</param>
        /// <returns>The result as text type value</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-command.html</remarks>
        public static string PgroongaCommand(this DbFunctions _, string command) => throw new NotSupportedException();

        /// <summary>
        /// Executes a Groonga command
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="command">A Groonga command</param>
        /// <param name="arguments">Arguments for the command</param>
        /// <returns>The result as text type value</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-command.html</remarks>
        public static string PgroongaCommand(this DbFunctions _, string command, string[] arguments) => throw new NotSupportedException();

        /// <summary>
        /// Escapes special characters in "value" part of Groonga command format
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="value">"value" part of Groonga command format</param>
        /// <returns>The escaped result as text type value</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-command-escape-value.html</remarks>
        public static string PgroongaCommandEscapeValue(this DbFunctions _, string value) => throw new NotSupportedException();

        /// <summary>
        /// Converts the given value to a literal for script syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="value">A literal to be used in script syntax</param>
        /// <returns>Script syntax safely literal</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-escape.html</remarks>
        public static string PgroongaEscape(this DbFunctions _, string value) => throw new NotSupportedException();

        /// <summary>
        /// Converts the given value to a literal for script syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="value">A literal to be used in script syntax</param>
        /// <returns>Script syntax safely literal</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-escape.html</remarks>
        public static string PgroongaEscape(this DbFunctions _, bool value) => throw new NotSupportedException();

        /// <summary>
        /// Converts the given value to a literal for script syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="value">A literal to be used in script syntax</param>
        /// <returns>Script syntax safely literal</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-escape.html</remarks>
        public static string PgroongaEscape(this DbFunctions _, short value) => throw new NotSupportedException();

        /// <summary>
        /// Converts the given value to a literal for script syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="value">A literal to be used in script syntax</param>
        /// <returns>Script syntax safely literal</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-escape.html</remarks>
        public static string PgroongaEscape(this DbFunctions _, int value) => throw new NotSupportedException();

        /// <summary>
        /// Converts the given value to a literal for script syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="value">A literal to be used in script syntax</param>
        /// <returns>Script syntax safely literal</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-escape.html</remarks>
        public static string PgroongaEscape(this DbFunctions _, long value) => throw new NotSupportedException();

        /// <summary>
        /// Converts the given value to a literal for script syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="value">A literal to be used in script syntax</param>
        /// <returns>Script syntax safely literal</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-escape.html</remarks>
        public static string PgroongaEscape(this DbFunctions _, float value) => throw new NotSupportedException();

        /// <summary>
        /// Converts the given value to a literal for script syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="value">A literal to be used in script syntax</param>
        /// <returns>Script syntax safely literal</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-escape.html</remarks>
        public static string PgroongaEscape(this DbFunctions _, double value) => throw new NotSupportedException();

        /// <summary>
        /// Converts the given value to a literal for script syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="value">A literal to be used in script syntax</param>
        /// <returns>Script syntax safely literal</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-escape.html</remarks>
        public static string PgroongaEscape(this DbFunctions _, DateTime value) => throw new NotSupportedException();

        /// <summary>
        /// Converts the given value to a literal for script syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="value">A literal to be used in script syntax</param>
        /// <returns>Script syntax safely literal</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-escape.html</remarks>
        public static string PgroongaEscape(this DbFunctions _, DateTimeOffset value) => throw new NotSupportedException();

        /// <summary>
        /// Converts the given value to a literal for script syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="value">A literal to be used in script syntax</param>
        /// <param name="specialCharacters">All characters to be escaped</param>
        /// <returns>Script syntax safely literal</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-escape.html</remarks>
        public static string PgroongaEscape(this DbFunctions _, string value, string[] specialCharacters) => throw new NotSupportedException();

        /// <summary>
        /// Ensure writing changes only in memory into disk
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="pgroongaIndexName">An index name</param>
        /// <returns>Whether the changes has been flushed or not</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-flush.html</remarks>
        public static bool PgroongaFlush(this DbFunctions _, string pgroongaIndexName) => throw new NotSupportedException();

        /// <summary>
        /// Surrounds the specified keywords in the specified text by <span class="keyword"> and </span>
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="target">A text to be highlighted</param>
        /// <param name="keywords">Keywords to be highlighted</param>
        /// <returns>Highlighted text</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-highlight-html.html</remarks>
        public static string PgroongaHighlightHtml(this DbFunctions _, string target, string[] keywords) => throw new NotSupportedException();

        /// <summary>
        /// Surrounds the specified keywords in the specified text by <span class="keyword"> and </span>
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="target">A text to be highlighted</param>
        /// <param name="keywords">Keywords to be highlighted</param>
        /// <param name="pgroongaIndexName">An index name</param>
        /// <returns>Highlighted text</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-highlight-html.html</remarks>
        public static string PgroongaHighlightHtml(this DbFunctions _, string target, string[] keywords, string pgroongaIndexName) => throw new NotSupportedException();

        /// <summary>
        /// Determine whether you can change PGroonga data or not
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <returns>Whether you can change PGroonga data or not</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-is-writable.html</remarks>
        public static bool PgroongaIsWritable(this DbFunctions _) => throw new NotSupportedException();

        /// <summary>
        /// Get positions of the specified keywords in the specified text
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="target">A text to be searched</param>
        /// <param name="keywords">Keywords to be found</param>
        /// <returns>An array of positions consists of offset and length</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-match-positions-byte.html</remarks>
        public static int[,] PgroongaMatchPositionsByte(this DbFunctions _, string target, string[] keywords) => throw new NotSupportedException();

        /// <summary>
        /// Get positions of the specified keywords in the specified text
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="target">A text to be searched</param>
        /// <param name="keywords">Keywords to be found</param>
        /// <returns>An array of positions consists of offset and length</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-match-positions-character.html</remarks>
        public static int[,] PgroongaMatchPositionsCharacter(this DbFunctions _, string target, string[] keywords) => throw new NotSupportedException();

        /// <summary>
        /// Converts a text into a normalized form using Groonga's normalizer modules
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="target">A text to be highlighted</param>
        /// <returns>Normalized text</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-normalize.html</remarks>
        public static string PgroongaNormalize(this DbFunctions _, string target) => throw new NotSupportedException();

        /// <summary>
        /// Converts a text into a normalized form using Groonga's normalizer modules
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="target">A text to be highlighted</param>
        /// <param name="normalizerName">The normalizer module you want to use</param>
        /// <returns>Normalized text</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-normalize.html</remarks>
        public static string PgroongaNormalize(this DbFunctions _, string target, string normalizerName) => throw new NotSupportedException();

        /// <summary>
        /// Converts the given value to a literal for query syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="query">A text in query syntax</param>
        /// <returns>Query syntax safely literal</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-query-escape.html</remarks>
        public static string PgroongaQueryEscape(this DbFunctions _, string query) => throw new NotSupportedException();

        /// <summary>
        /// Expands registered synonyms in query in query syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="tableName">An existing table name that stores synonyms</param>
        /// <param name="termColumnName">A column name that stores term to be expanded</param>
        /// <param name="synonymsColumnName">A column name that stores synonyms of the term column</param>
        /// <param name="query">A text in query syntax</param>
        /// <returns>The query with all registered synonyms expanded</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-query-expand.html</remarks>
        public static string PgroongaQueryExpand(this DbFunctions _, string tableName, string termColumnName, string synonymsColumnName, string query) => throw new NotSupportedException();

        /// <summary>
        /// Extract keywords from text that uses query syntax
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="query">A text in query syntax</param>
        /// <returns>An array of keywords</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-query-extract-keywords.html</remarks>
        public static string[] PgroongaQueryExtractKeywords(this DbFunctions _, string query) => throw new NotSupportedException();

        /// <summary>
        /// Set whether you can change PGroonga data or not
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="writable">True for writable or False for read-only</param>
        /// <returns>Whether you can change PGroonga data or not before changing the current state</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-set-writable.html</remarks>
        public static bool PgroongaSetWritable(this DbFunctions _, bool writable) => throw new NotSupportedException();

        /// <summary>
        /// Get precision as a number
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="columnName">Table alias name</param>
        /// <returns>Precision as double precision type value</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-score.html</remarks>
        public static double PgroongaScore(this DbFunctions _, string columnName) => throw new NotSupportedException();

        /// <summary>
        /// Get KWIC (keyword in context) in the specified text
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="target">A text to be searched</param>
        /// <param name="keywords">Keywords to be extracted</param>
        /// <returns>An array of keywords in context with keywords highlighted</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-snippet-html.html</remarks>
        public static string[] PgroongaSnippetHtml(this DbFunctions _, string target, string[] keywords) => throw new NotSupportedException();

        /// <summary>
        /// Converts PGroonga index name to Groonga table name
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="pgroongaIndexName">An index name</param>
        /// <returns>Groonga table name for pgroonga_index_name as text type value</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-table-name.html</remarks>
        public static string PgroongaTableName(this DbFunctions _, string pgroongaIndexName) => throw new NotSupportedException();

        /// <summary>
        /// Apply pending WAL
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <returns>The number of applied operations</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-wal-apply.html</remarks>
        public static long PgroongaWalApply(this DbFunctions _) => throw new NotSupportedException();

        /// <summary>
        /// Apply pending WAL
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="pgroongaIndexName">An index name</param>
        /// <returns>The number of applied operations</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-wal-apply.html</remarks>
        public static long PgroongaWalApply(this DbFunctions _, string pgroongaIndexName) => throw new NotSupportedException();

        /// <summary>
        /// Truncate pending WAL
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <returns>The number of truncated PostgreSQL data blocks</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-wal-truncate.html</remarks>
        public static long PgroongaWalTruncate(this DbFunctions _) => throw new NotSupportedException();

        /// <summary>
        /// Truncate pending WAL
        /// </summary>
        /// <param name="_"><see cref="DbFunctions"/></param>
        /// <param name="pgroongaIndexName">An index name</param>
        /// <returns>The number of truncated PostgreSQL data blocks</returns>
        /// <remarks>https://pgroonga.github.io/reference/functions/pgroonga-wal-truncate.html</remarks>
        public static long PgroongaWalTruncate(this DbFunctions _, string pgroongaIndexName) => throw new NotSupportedException();
    }
}
