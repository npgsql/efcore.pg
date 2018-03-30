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
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlRangeTypeMapping<T> : NpgsqlTypeMapping
    {
        public RelationalTypeMapping SubtypeMapping { get; }

        readonly string EmptyLiteral;

        internal NpgsqlRangeTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            RelationalTypeMapping subtypeMapping,
            NpgsqlDbType subtypeNpgsqlDbType)
            : base(storeType, clrType, NpgsqlDbType.Range | subtypeNpgsqlDbType)
        {
            SubtypeMapping = subtypeMapping;
            EmptyLiteral = $"'empty'::{storeType}";
        }

        protected NpgsqlRangeTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlRangeTypeMapping<T>(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlRangeTypeMapping<T>(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var range = (NpgsqlRange<T>)value;
            if (range.IsEmpty)
                return EmptyLiteral;

            var sb = new StringBuilder();
            sb.Append('\'');
            sb.Append(range.LowerBoundIsInclusive ? '[' : '(');
            if (!range.LowerBoundInfinite)
                sb.Append(SubtypeMapping.GenerateSqlLiteral(range.LowerBound));
            sb.Append(',');
            if (!range.UpperBoundInfinite)
                sb.Append(SubtypeMapping.GenerateSqlLiteral(range.UpperBound));
            sb.Append(range.UpperBoundIsInclusive ? ']' : ')');
            sb.Append('\'');
            sb.Append("::");
            sb.Append(StoreType);
            return sb.ToString();
        }
    }
}
