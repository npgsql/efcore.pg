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
using System.Diagnostics.CodeAnalysis;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public static class NpgsqlFullTextSearchLinqExtensions
    {
        /// <summary>
        /// AND tsquerys together. Generates the "&amp;&amp;" operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery And(this NpgsqlTsQuery query1, NpgsqlTsQuery query2) =>
            throw new NotSupportedException();

        /// <summary>
        /// OR tsquerys together. Generates the "||" operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery Or(this NpgsqlTsQuery query1, NpgsqlTsQuery query2) =>
            throw new NotSupportedException();

        /// <summary>
        /// Negate a tsquery. Generates the "!!" operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery ToNegative(this NpgsqlTsQuery query) =>
            throw new NotSupportedException();

        /// <summary>
        /// Returns whether <paramref name="query1" /> contains <paramref name="query2" />.
        /// Generates the "@&gt;" operator.
        /// http://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static bool Contains(this NpgsqlTsQuery query1, NpgsqlTsQuery query2) =>
            throw new NotSupportedException();

        /// <summary>
        /// Returns whether <paramref name="query1" /> is contained within <paramref name="query2" />.
        /// Generates the "&lt;@" operator.
        /// http://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static bool IsContainedIn(this NpgsqlTsQuery query1, NpgsqlTsQuery query2) =>
            throw new NotSupportedException();

        /// <summary>
        /// Returns the number of lexemes plus operators in <paramref name="query" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static int GetNodeCount(this NpgsqlTsQuery query) => throw new NotSupportedException();

        /// <summary>
        /// Get the indexable part of <paramref name="query" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static string GetQueryTree(this NpgsqlTsQuery query) => throw new NotSupportedException();

        /// <summary>
        /// Returns a string suitable for display containing a query match.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
        /// </summary>
        public static string GetResultHeadline(this NpgsqlTsQuery query, string document) =>
            throw new NotSupportedException();

        /// <summary>
        /// Returns a string suitable for display containing a query match.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
        /// </summary>
        public static string GetResultHeadline(this NpgsqlTsQuery query, string document, string options) =>
            throw new NotSupportedException();

        /// <summary>
        /// Returns a string suitable for display containing a query match using the text
        /// search configuration specified by <paramref name="config" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
        /// </summary>
        public static string GetResultHeadline(
            this NpgsqlTsQuery query,
            string config,
            string document,
            string options) => throw new NotSupportedException();

        /// <summary>
        /// Searchs <paramref name="query" /> for occurrences of <paramref name="target" />, and replaces
        /// each occurrence with a <paramref name="substitute" />. All parameters are of type tsquery.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery Rewrite(this NpgsqlTsQuery query, NpgsqlTsQuery target, NpgsqlTsQuery substitute) =>
            throw new NotSupportedException();

        /// <summary>
        /// Returns a tsquery that searches for a match to <paramref name="query1" /> followed by a match
        /// to <paramref name="query2" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery ToPhrase(this NpgsqlTsQuery query1, NpgsqlTsQuery query2) =>
            throw new NotSupportedException();

        /// <summary>
        /// Returns a tsquery that searches for a match to <paramref name="query1" /> followed by a match
        /// to <paramref name="query2" /> at a distance of <paramref name="distance" /> lexemes using
        /// the &lt;N&gt; tsquery operator
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static NpgsqlTsQuery ToPhrase(this NpgsqlTsQuery query1, NpgsqlTsQuery query2, int distance) =>
            throw new NotSupportedException();

        /// <summary>
        /// This method generates the "@@" match operator. The <paramref name="query"/> parameter is
        /// assumed to be a plain search query and will be converted to a tsquery using plainto_tsquery.
        /// http://www.postgresql.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING
        /// </summary>
        public static bool Matches(this NpgsqlTsVector vector, string query) => throw new NotSupportedException();

        /// <summary>
        /// This method generates the "@@" match operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING
        /// </summary>
        public static bool Matches(this NpgsqlTsVector vector, NpgsqlTsQuery query) =>
            throw new NotSupportedException();

        /// <summary>
        /// Assign weight to each element of <paramref name="vector" /> and return a new
        /// weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static NpgsqlTsVector SetWeight(this NpgsqlTsVector vector, NpgsqlTsVector.Lexeme.Weight weight) =>
            throw new NotSupportedException();

        /// <summary>
        /// Assign weight to elements of <paramref name="vector" /> that are in <paramref name="lexemes" /> and
        /// return a new weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static NpgsqlTsVector SetWeight(
            this NpgsqlTsVector vector,
            NpgsqlTsVector.Lexeme.Weight weight,
            string[] lexemes) =>
            throw new NotSupportedException();

        /// <summary>
        /// Assign weight to each element of <paramref name="vector" /> and return a new
        /// weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static NpgsqlTsVector SetWeight(this NpgsqlTsVector vector, char weight) =>
            throw new NotSupportedException();

        /// <summary>
        /// Assign weight to elements of <paramref name="vector" /> that are in <paramref name="lexemes" /> and
        /// return a new weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static NpgsqlTsVector SetWeight(this NpgsqlTsVector vector, char weight, string[] lexemes) =>
            throw new NotSupportedException();

        /// <summary>
        /// Returns the number of lexemes in <paramref name="vector" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static int GetLength(this NpgsqlTsVector vector) => throw new NotSupportedException();

        /// <summary>
        /// Removes weights and positions from <paramref name="vector" /> and returns
        /// a new stripped tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static NpgsqlTsVector ToStripped(this NpgsqlTsVector vector) => throw new NotSupportedException();

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank(this NpgsqlTsVector vector, NpgsqlTsQuery query) => throw new NotSupportedException();

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> while normalizing
        /// the result according to the behaviors specified by <paramref name="normalization" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank(
            this NpgsqlTsVector vector,
            NpgsqlTsQuery query,
            NpgsqlTsRankingNormalization normalization) => throw new NotSupportedException();

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> with custom
        /// weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank(this NpgsqlTsVector vector, float[] weights, NpgsqlTsQuery query) =>
            throw new NotSupportedException();

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> while normalizing
        /// the result according to the behaviors specified by <paramref name="normalization" />
        /// and using custom weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank(
            this NpgsqlTsVector vector,
            float[] weights,
            NpgsqlTsQuery query,
            NpgsqlTsRankingNormalization normalization) => throw new NotSupportedException();

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
        /// density method.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(this NpgsqlTsVector vector, NpgsqlTsQuery query) =>
            throw new NotSupportedException();

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
        /// density method while normalizing the result according to the behaviors specified by
        /// <paramref name="normalization" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(
            this NpgsqlTsVector vector,
            NpgsqlTsQuery query,
            NpgsqlTsRankingNormalization normalization) => throw new NotSupportedException();

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
        /// density method with custom weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(this NpgsqlTsVector vector, float[] weights, NpgsqlTsQuery query) =>
            throw new NotSupportedException();

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover density
        /// method while normalizing the result according to the behaviors specified by <paramref name="normalization" />
        /// and using custom weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(
            this NpgsqlTsVector vector,
            float[] weights,
            NpgsqlTsQuery query,
            NpgsqlTsRankingNormalization normalization) => throw new NotSupportedException();
    }
}
