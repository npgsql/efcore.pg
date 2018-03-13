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

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public enum NpgsqlValueGenerationStrategy
    {
        /// <summary>
        /// <para>
        /// A sequence-based hi-lo pattern where blocks of IDs are allocated from the server and
        /// used client-side for generating keys.
        /// </para>
        /// <para>
        /// This is an advanced pattern--only use this strategy if you are certain it is what you need.
        /// </para>
        /// </summary>
        SequenceHiLo,

        /// <summary>
        /// <para>
        /// Selects the serial column strategy, which is a regular column backed by an auto-created index.
        /// </para>
        /// <para>
        /// If you are creating a new project on PostgreSQL 10 or above, consider using <see cref="IdentityByDefaultColumn"/> instead.
        /// </para>
        /// </summary>
        SerialColumn,

        /// <summary>
        /// <para>Selects the always-identity column strategy (a value cannot be provided).</para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        IdentityAlwaysColumn,

        /// <summary>
        /// <para>Selects the by-default-identity column strategy (a value can be provided to override the identity mechanism).</para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        IdentityByDefaultColumn,
    }
}
