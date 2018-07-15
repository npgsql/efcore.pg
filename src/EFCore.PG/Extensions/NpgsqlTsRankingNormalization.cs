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

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Specifies whether and how a document's length should impact its rank.
    /// This is used with the ranking functions in <see cref="NpgsqlFullTextSearchLinqExtensions" />.
    ///
    /// See http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
    /// for more information about the behaviors that are controlled by this value.
    /// </summary>
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum NpgsqlTsRankingNormalization
    {
        /// <summary>
        /// Ignores the document length.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Divides the rank by 1 + the logarithm of the document length.
        /// </summary>
        DivideBy1PlusLogLength = 1,

        /// <summary>
        /// Divides the rank by the document length.
        /// </summary>
        DivideByLength = 2,

        /// <summary>
        /// Divides the rank by the mean harmonic distance between extents (this is implemented only by ts_rank_cd).
        /// </summary>
        DivideByMeanHarmonicDistanceBetweenExtents = 4,

        /// <summary>
        /// Divides the rank by the number of unique words in document.
        /// </summary>
        DivideByUniqueWordCount = 8,

        /// <summary>
        /// Divides the rank by 1 + the logarithm of the number of unique words in document.
        /// </summary>
        DividesBy1PlusLogUniqueWordCount = 16,

        /// <summary>
        /// Divides the rank by itself + 1.
        /// </summary>
        DivideByItselfPlusOne = 32
    }
}
