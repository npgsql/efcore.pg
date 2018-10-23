using System;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
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
            var p = new NpgsqlParameter();
            p.DbType = subtypeMapping.DbType.Value;
            return NpgsqlDbType.Range | p.NpgsqlDbType;
        }
    }
}
