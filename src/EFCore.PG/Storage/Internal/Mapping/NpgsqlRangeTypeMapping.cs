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
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The type mapping for the PostgreSQL range types.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/rangetypes.html
    /// </remarks>
    public class NpgsqlRangeTypeMapping : NpgsqlTypeMapping
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        /// <summary>
        /// The relational type mapping used to initialize the bound mapping.
        /// </summary>
        public RelationalTypeMapping SubtypeMapping { get; }

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlRangeTypeMapping"/> class.
        /// </summary>
        /// <param name="storeType">The database type to map</param>
        /// <param name="clrType">The CLR type to map.</param>
        /// <param name="subtypeMapping">The type mapping for the range subtype.</param>
        public NpgsqlRangeTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [NotNull] RelationalTypeMapping subtypeMapping)
            : base(storeType, clrType, GenerateNpgsqlDbType(subtypeMapping))
            => SubtypeMapping = subtypeMapping;

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlRangeTypeMapping"/> class.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        /// <param name="npgsqlDbType">The database type of the range subtype.</param>
        protected NpgsqlRangeTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlRangeTypeMapping(parameters, NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var sb = new StringBuilder();
            sb.Append('\'');
            sb.Append(value);
            sb.Append("'::");
            sb.Append(StoreType);
            return sb.ToString();
        }

        static NpgsqlDbType GenerateNpgsqlDbType([NotNull] RelationalTypeMapping subtypeMapping)
        {
            if (subtypeMapping is NpgsqlTypeMapping npgsqlTypeMapping)
                return NpgsqlDbType.Range | npgsqlTypeMapping.NpgsqlDbType;

            // We're using a built-in, non-Npgsql mapping such as IntTypeMapping.
            // Infer the NpgsqlDbType from the DbType (somewhat hacky but why not).
            Debug.Assert(subtypeMapping.DbType.HasValue);
            var p = new NpgsqlParameter { DbType = subtypeMapping.DbType.Value };
            return NpgsqlDbType.Range | p.NpgsqlDbType;
        }
    }
}
