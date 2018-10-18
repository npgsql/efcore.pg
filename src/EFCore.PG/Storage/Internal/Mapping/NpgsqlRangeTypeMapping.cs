using System;
using System.Diagnostics;
using System.Linq.Expressions;
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

        public override Expression GenerateCodeLiteral(object value)
        {
            var subtypeType = SubtypeMapping.ClrType;
            var rangeType = typeof(NpgsqlRange<>).MakeGenericType(subtypeType);

            var lower = rangeType.GetProperty(nameof(NpgsqlRange<int>.LowerBound)).GetValue(value);
            var upper = rangeType.GetProperty(nameof(NpgsqlRange<int>.UpperBound)).GetValue(value);
            var lowerInfinite = (bool)rangeType.GetProperty(nameof(NpgsqlRange<int>.LowerBoundInfinite)).GetValue(value);
            var upperInfinite = (bool)rangeType.GetProperty(nameof(NpgsqlRange<int>.UpperBoundInfinite)).GetValue(value);
            var lowerInclusive = (bool)rangeType.GetProperty(nameof(NpgsqlRange<int>.LowerBoundIsInclusive)).GetValue(value);
            var upperInclusive = (bool)rangeType.GetProperty(nameof(NpgsqlRange<int>.UpperBoundIsInclusive)).GetValue(value);

            return lowerInfinite || upperInfinite
               ? Expression.New(
                    rangeType.GetConstructor(new[] { subtypeType, typeof(bool), typeof(bool), subtypeType, typeof(bool), typeof(bool) }),
                    Expression.Constant(lower),
                    Expression.Constant(lowerInclusive),
                    Expression.Constant(lowerInfinite),
                    Expression.Constant(upper),
                    Expression.Constant(upperInclusive),
                    Expression.Constant(upperInfinite))
               : Expression.New(
                    rangeType.GetConstructor(new[] { subtypeType, typeof(bool), subtypeType, typeof(bool) }),
                    Expression.Constant(lower),
                    Expression.Constant(lowerInclusive),
                    Expression.Constant(upper),
                    Expression.Constant(upperInclusive));
        }
    }
}
