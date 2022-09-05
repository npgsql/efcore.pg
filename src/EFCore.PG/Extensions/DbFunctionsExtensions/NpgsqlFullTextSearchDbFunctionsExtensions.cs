using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
///     The methods on this class are accessed via <see cref="EF.Functions" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>.
/// </remarks>
[SuppressMessage("ReSharper", "UnusedParameter.Global")]
public static class NpgsqlFullTextSearchDbFunctionsExtensions
{
    /// <summary>
    /// Convert <paramref name="lexemes" /> to a tsvector.
    /// </summary>
    /// <remarks>
    /// https://www.postgresql.org/docs/current/static/functions-textsearch.html
    /// </remarks>
    public static NpgsqlTsVector ArrayToTsVector(this DbFunctions _, string[] lexemes)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ArrayToTsVector)));

    /// <summary>
    /// Reduce <paramref name="document" /> to tsvector.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-DOCUMENTS
    /// </remarks>
    public static NpgsqlTsVector ToTsVector(this DbFunctions _, string document)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToTsVector)));

    /// <summary>
    /// Reduce <paramref name="document" /> to tsvector using the text search configuration specified
    /// by <paramref name="config" />.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-DOCUMENTS
    /// </remarks>
    public static NpgsqlTsVector ToTsVector(this DbFunctions _, [NotParameterized] string config, string document)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToTsVector)));

    /// <summary>
    /// Produce tsquery from <paramref name="query" /> ignoring punctuation.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
    /// </remarks>
    public static NpgsqlTsQuery PlainToTsQuery(this DbFunctions _, string query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(PlainToTsQuery)));

    /// <summary>
    /// Produce tsquery from <paramref name="query" /> ignoring punctuation and using the text search
    /// configuration specified by <paramref name="config" />.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
    /// </remarks>
    public static NpgsqlTsQuery PlainToTsQuery(this DbFunctions _, [NotParameterized] string config, string query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(PlainToTsQuery)));

    /// <summary>
    /// Produce tsquery that searches for a phrase from <paramref name="query" /> ignoring punctuation.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
    /// </remarks>
    public static NpgsqlTsQuery PhraseToTsQuery(this DbFunctions _, string query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(PhraseToTsQuery)));

    /// <summary>
    /// Produce tsquery that searches for a phrase from <paramref name="query" /> ignoring punctuation
    /// and using the text search configuration specified by <paramref name="config" />.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
    /// </remarks>
    public static NpgsqlTsQuery PhraseToTsQuery(this DbFunctions _, [NotParameterized] string config, string query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(PhraseToTsQuery)));

    /// <summary>
    /// Normalize words in <paramref name="query" /> and convert to tsquery. If your input
    /// contains punctuation that should not be treated as text search operators, use
    /// <see cref="PlainToTsQuery(DbFunctions, string)" /> instead.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
    /// </remarks>
    public static NpgsqlTsQuery ToTsQuery(this DbFunctions _, string query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToTsQuery)));

    /// <summary>
    /// Normalize words in <paramref name="query" /> and convert to tsquery using the text search
    /// configuration specified by <paramref name="config" />. If your input contains punctuation
    /// that should not be treated as text search operators, use
    /// <see cref="PlainToTsQuery(DbFunctions, string, string)" /> instead.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
    /// </remarks>
    public static NpgsqlTsQuery ToTsQuery(this DbFunctions _, [NotParameterized] string config, string query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToTsQuery)));

    /// <summary>
    /// Convert <paramref name="query" /> tsquery using the simplified websearch syntax.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
    /// </remarks>
    public static NpgsqlTsQuery WebSearchToTsQuery(this DbFunctions _, string query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(WebSearchToTsQuery)));

    /// <summary>
    /// Convert <paramref name="query" /> tsquery using the simplified websearch syntax and the text
    /// search configuration specified by <paramref name="config" />.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
    /// </remarks>
    public static NpgsqlTsQuery WebSearchToTsQuery(this DbFunctions _, [NotParameterized] string config, string query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(WebSearchToTsQuery)));

    /// <summary>
    /// Returns a new string that removes diacritics from characters in the given <paramref name="text" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="regDictionary">A specific text search dictionary.</param>
    /// <param name="text">The text to remove the diacritics.</param>
    /// <remarks>
    /// <para>The method call is translated to <c>unaccent(regdictionary, text)</c>.</para>
    ///
    /// See https://www.postgresql.org/docs/current/unaccent.html.
    /// </remarks>
    /// <returns>A string without diacritics.</returns>
    public static string Unaccent(this DbFunctions _, string regDictionary, string text)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Unaccent)));

    /// <summary>
    /// Returns a new string that removes diacritics from characters in the given <paramref name="text" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="text">The text to remove the diacritics.</param>
    /// <remarks>
    /// <para>The method call is translated to <c>unaccent(text)</c>.</para>
    ///
    /// See https://www.postgresql.org/docs/current/unaccent.html.
    /// </remarks>
    /// <returns>A string without diacritics.</returns>
    public static string Unaccent(this DbFunctions _, string text)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Unaccent)));
}
