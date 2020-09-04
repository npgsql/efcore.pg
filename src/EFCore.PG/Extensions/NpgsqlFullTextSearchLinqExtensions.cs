using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public static class NpgsqlFullTextSearchLinqExtensions
    {
        /// <summary>
        /// AND tsquerys together. Generates the "&amp;&amp;" operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery And([NotNull] this NpgsqlTsQuery query1, [NotNull] NpgsqlTsQuery query2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(And)));

        /// <summary>
        /// OR tsquerys together. Generates the "||" operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery Or([NotNull] this NpgsqlTsQuery query1, [NotNull] NpgsqlTsQuery query2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(Or)));

        /// <summary>
        /// Negate a tsquery. Generates the "!!" operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery ToNegative([NotNull] this NpgsqlTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(ToNegative)));

        /// <summary>
        /// Returns whether <paramref name="query1" /> contains <paramref name="query2" />.
        /// Generates the "@&gt;" operator.
        /// http://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static bool Contains([NotNull] this NpgsqlTsQuery query1, [NotNull] NpgsqlTsQuery query2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(Contains)));

        /// <summary>
        /// Returns whether <paramref name="query1" /> is contained within <paramref name="query2" />.
        /// Generates the "&lt;@" operator.
        /// http://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static bool IsContainedIn([NotNull] this NpgsqlTsQuery query1, [NotNull] NpgsqlTsQuery query2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(IsContainedIn)));

        /// <summary>
        /// Returns the number of lexemes plus operators in <paramref name="query" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static int GetNodeCount([NotNull] this NpgsqlTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(GetNodeCount)));

        /// <summary>
        /// Get the indexable part of <paramref name="query" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static string GetQueryTree([NotNull] this NpgsqlTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(GetQueryTree)));

        /// <summary>
        /// Returns a string suitable for display containing a query match.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
        /// </summary>
        public static string GetResultHeadline([NotNull] this NpgsqlTsQuery query, [NotNull] string document)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(GetResultHeadline)));

        /// <summary>
        /// Returns a string suitable for display containing a query match.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
        /// </summary>
        public static string GetResultHeadline(
            [NotNull] this NpgsqlTsQuery query,
            [NotNull] string document,
            [NotNull] string options)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(GetResultHeadline)));

        /// <summary>
        /// Returns a string suitable for display containing a query match using the text
        /// search configuration specified by <paramref name="config" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
        /// </summary>
        public static string GetResultHeadline(
            [NotNull] this NpgsqlTsQuery query,
            [NotNull] string config,
            [NotNull] string document,
            [NotNull] string options)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(GetResultHeadline)));

        /// <summary>
        /// Searches <paramref name="query" /> for occurrences of <paramref name="target" />, and replaces
        /// each occurrence with a <paramref name="substitute" />. All parameters are of type tsquery.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery Rewrite(
            [NotNull] this NpgsqlTsQuery query,
            [NotNull] NpgsqlTsQuery target,
            [NotNull] NpgsqlTsQuery substitute)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(Rewrite)));

        /// <summary>
        /// Returns a tsquery that searches for a match to <paramref name="query1" /> followed by a match
        /// to <paramref name="query2" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery ToPhrase(
            [NotNull] this NpgsqlTsQuery query1,
            [NotNull] NpgsqlTsQuery query2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(ToPhrase)));

        /// <summary>
        /// Returns a tsquery that searches for a match to <paramref name="query1" /> followed by a match
        /// to <paramref name="query2" /> at a distance of <paramref name="distance" /> lexemes using
        /// the &lt;N&gt; tsquery operator
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery ToPhrase(
            [NotNull] this NpgsqlTsQuery query1,
            [NotNull] NpgsqlTsQuery query2,
            int distance)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsQuery) + "." + nameof(ToPhrase)));

        /// <summary>
        /// This method generates the "@@" match operator. The <paramref name="query"/> parameter is
        /// assumed to be a plain search query and will be converted to a tsquery using plainto_tsquery.
        /// http://www.postgresql.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING
        /// </summary>
        public static bool Matches([NotNull] this NpgsqlTsVector vector, [NotNull] string query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(Matches)));

        /// <summary>
        /// This method generates the "@@" match operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING
        /// </summary>
        public static bool Matches([NotNull] this NpgsqlTsVector vector, [NotNull] NpgsqlTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(Matches)));

        /// <summary>
        /// Returns a vector which combines the lexemes and positional information of <paramref name="vector1" />
        /// and <paramref name="vector2"/> using the || tsvector operator. Positions and weight labels are retained
        /// during the concatenation.
        /// https://www.postgresql.org/docs/10/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static NpgsqlTsVector Concat([NotNull] this NpgsqlTsVector vector1, [NotNull] NpgsqlTsVector vector2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(Concat)));

        /// <summary>
        /// Assign weight to each element of <paramref name="vector" /> and return a new
        /// weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static NpgsqlTsVector SetWeight(
            [NotNull] this NpgsqlTsVector vector,
            NpgsqlTsVector.Lexeme.Weight weight)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(SetWeight)));

        /// <summary>
        /// Assign weight to elements of <paramref name="vector" /> that are in <paramref name="lexemes" /> and
        /// return a new weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static NpgsqlTsVector SetWeight(
            [NotNull] this NpgsqlTsVector vector,
            NpgsqlTsVector.Lexeme.Weight weight,
            [NotNull] string[] lexemes)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(SetWeight)));

        /// <summary>
        /// Assign weight to each element of <paramref name="vector" /> and return a new
        /// weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static NpgsqlTsVector SetWeight([NotNull] this NpgsqlTsVector vector, char weight)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(SetWeight)));

        /// <summary>
        /// Assign weight to elements of <paramref name="vector" /> that are in <paramref name="lexemes" /> and
        /// return a new weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static NpgsqlTsVector SetWeight(
            [NotNull] this NpgsqlTsVector vector,
            char weight,
            [NotNull] string[] lexemes)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(SetWeight)));

        /// <summary>
        /// Return a new vector with <paramref name="lexeme" /> removed from <paramref name="vector" />
        /// https://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static NpgsqlTsVector Delete([NotNull] this NpgsqlTsVector vector, [NotNull]  string lexeme)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(Delete)));

        /// <summary>
        /// Return a new vector with <paramref name="lexemes" /> removed from <paramref name="vector" />
        /// https://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static NpgsqlTsVector Delete([NotNull] this NpgsqlTsVector vector, [NotNull] string[] lexemes)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(Delete)));

        /// <summary>
        /// Returns a new vector with only lexemes having weights specified in <paramref name="weights" />.
        /// https://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static NpgsqlTsVector Filter([NotNull] this NpgsqlTsVector vector, [NotNull] char[] weights)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(Filter)));

        /// <summary>
        /// Returns the number of lexemes in <paramref name="vector" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static int GetLength([NotNull] this NpgsqlTsVector vector)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(GetLength)));

        /// <summary>
        /// Removes weights and positions from <paramref name="vector" /> and returns
        /// a new stripped tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static NpgsqlTsVector ToStripped([NotNull] this NpgsqlTsVector vector)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(ToStripped)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank([NotNull] this NpgsqlTsVector vector, [NotNull] NpgsqlTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(Rank)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> while normalizing
        /// the result according to the behaviors specified by <paramref name="normalization" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank(
            [NotNull] this NpgsqlTsVector vector,
            [NotNull] NpgsqlTsQuery query,
            NpgsqlTsRankingNormalization normalization)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(Rank)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> with custom
        /// weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank(
            [NotNull] this NpgsqlTsVector vector,
            [NotNull] float[] weights,
            [NotNull] NpgsqlTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(Rank)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> while normalizing
        /// the result according to the behaviors specified by <paramref name="normalization" />
        /// and using custom weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank(
            [NotNull] this NpgsqlTsVector vector,
            [NotNull] float[] weights,
            [NotNull] NpgsqlTsQuery query,
            NpgsqlTsRankingNormalization normalization)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(Rank)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
        /// density method.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(
            [NotNull] this NpgsqlTsVector vector,
            [NotNull] NpgsqlTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(RankCoverDensity)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
        /// density method while normalizing the result according to the behaviors specified by
        /// <paramref name="normalization" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(
            [NotNull] this NpgsqlTsVector vector,
            [NotNull] NpgsqlTsQuery query,
            NpgsqlTsRankingNormalization normalization)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(RankCoverDensity)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
        /// density method with custom weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(
            [NotNull] this NpgsqlTsVector vector,
            [NotNull] float[] weights,
            [NotNull] NpgsqlTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(RankCoverDensity)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover density
        /// method while normalizing the result according to the behaviors specified by <paramref name="normalization" />
        /// and using custom weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(
            [NotNull] this NpgsqlTsVector vector,
            [NotNull] float[] weights,
            [NotNull] NpgsqlTsQuery query,
            NpgsqlTsRankingNormalization normalization)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NpgsqlTsVector) + "." + nameof(RankCoverDensity)));
    }
}
