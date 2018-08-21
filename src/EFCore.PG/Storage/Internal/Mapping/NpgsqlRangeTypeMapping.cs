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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlRangeTypeMapping : NpgsqlTypeMapping
    {
        public RelationalTypeMapping SubtypeMapping { get; }

        public NpgsqlRangeTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [NotNull] RelationalTypeMapping subtypeMapping)
            : base(storeType, clrType, GenerateNpgsqlDbType(subtypeMapping))
        => SubtypeMapping = subtypeMapping;

        protected NpgsqlRangeTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) { }

        [NotNull]
        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlRangeTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        [NotNull]
        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlRangeTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

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
            var p = new NpgsqlParameter();
            p.DbType = subtypeMapping.DbType.Value;
            return NpgsqlDbType.Range | p.NpgsqlDbType;
        }
    }
}
