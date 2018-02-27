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
    public static class NpgsqlFullTextSearchDbFunctionsExtensions
    {
        /// <summary>
        /// Reduce <paramref name="document" /> to tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-DOCUMENTS
        /// </summary>
        public static NpgsqlTsVector ToTsVector(this DbFunctions _, string document) =>
            throw new NotSupportedException();

        /// <summary>
        /// Reduce <paramref name="document" /> to tsvector using the text search configuration specified
        /// by <paramref name="config" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-DOCUMENTS
        /// </summary>
        public static NpgsqlTsVector ToTsVector(this DbFunctions _, string config, string document) =>
            throw new NotSupportedException();

        /// <summary>
        /// Produce tsquery from <paramref name="query" /> ignoring punctuation.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </summary>
        public static NpgsqlTsQuery PlainToTsQuery(this DbFunctions _, string query) =>
            throw new NotSupportedException();

        /// <summary>
        /// Produce tsquery from <paramref name="query" /> ignoring punctuation and using the text search
        /// configuration specified by <paramref name="config" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </summary>
        public static NpgsqlTsQuery PlainToTsQuery(this DbFunctions _, string config, string query) =>
            throw new NotSupportedException();

        /// <summary>
        /// Produce tsquery that searches for a phrase from <paramref name="query" /> ignoring punctuation.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </summary>
        public static NpgsqlTsQuery PhraseToTsQuery(this DbFunctions _, string query) =>
            throw new NotSupportedException();

        /// <summary>
        /// Produce tsquery that searches for a phrase from <paramref name="query" /> ignoring punctuation
        /// and using the text search configuration specified by <paramref name="config" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </summary>
        public static NpgsqlTsQuery PhraseToTsQuery(this DbFunctions _, string config, string query) =>
            throw new NotSupportedException();

        /// <summary>
        /// Normalize words in <paramref name="query" /> and convert to tsquery. If your input
        /// contains punctuation that should not be treated as text search operators, use
        /// <see cref="PlainToTsQuery(DbFunctions, string)" /> instead.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </summary>
        public static NpgsqlTsQuery ToTsQuery(this DbFunctions _, string query) => throw new NotSupportedException();

        /// <summary>
        /// Normalize words in <paramref name="query" /> and convert to tsquery using the text search
        /// configuration specified by <paramref name="config" />. If your input contains punctuation
        /// that should not be treated as text search operators, use
        /// <see cref="PlainToTsQuery(DbFunctions, string, string)" /> instead.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </summary>
        public static NpgsqlTsQuery ToTsQuery(this DbFunctions _, string config, string query) =>
            throw new NotSupportedException();
    }
}
