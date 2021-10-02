using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The type mapping for PostgreSQL range types.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/rangetypes.html
    /// </remarks>
    public class NpgsqlRangeTypeMapping : NpgsqlTypeMapping
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        /// The relational type mapping of the range's subtype.
        /// </summary>
        public virtual RelationalTypeMapping SubtypeMapping { get; }

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlRangeTypeMapping"/> class.
        /// </summary>
        /// <param name="storeType">The database type to map</param>
        /// <param name="clrType">The CLR type to map.</param>
        /// <param name="subtypeMapping">The type mapping for the range subtype.</param>
        /// <param name="sqlGenerationHelper">The SQL generation helper to delimit the store name.</param>
        public NpgsqlRangeTypeMapping(
            string storeType,
            Type clrType,
            RelationalTypeMapping subtypeMapping,
            ISqlGenerationHelper sqlGenerationHelper)
            : this(storeType, storeTypeSchema: null, clrType, subtypeMapping, sqlGenerationHelper) {}

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlRangeTypeMapping"/> class.
        /// </summary>
        /// <param name="storeType">The database type to map</param>
        /// <param name="storeTypeSchema">The schema of the type.</param>
        /// <param name="clrType">The CLR type to map.</param>
        /// <param name="subtypeMapping">The type mapping for the range subtype.</param>
        /// <param name="sqlGenerationHelper">The SQL generation helper to delimit the store name.</param>
        public NpgsqlRangeTypeMapping(
            string storeType,
            string? storeTypeSchema,
            Type clrType,
            RelationalTypeMapping subtypeMapping,
            ISqlGenerationHelper sqlGenerationHelper)
            : base(sqlGenerationHelper.DelimitIdentifier(storeType, storeTypeSchema), clrType, GenerateNpgsqlDbType(subtypeMapping))
        {
            SubtypeMapping = subtypeMapping;
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        protected NpgsqlRangeTypeMapping(
            RelationalTypeMappingParameters parameters,
            NpgsqlDbType npgsqlDbType,
            RelationalTypeMapping subtypeMapping,
            ISqlGenerationHelper sqlGenerationHelper)
            : base(parameters, npgsqlDbType)
        {
            SubtypeMapping = subtypeMapping;
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlRangeTypeMapping(parameters, NpgsqlDbType, SubtypeMapping, _sqlGenerationHelper);

        protected override string GenerateNonNullSqlLiteral(object value)
            => new StringBuilder()
                .Append('\'')
                .Append(value)
                .Append("'::")
                .Append(StoreType)
                .ToString();

        private static NpgsqlDbType GenerateNpgsqlDbType(RelationalTypeMapping subtypeMapping)
        {
            NpgsqlDbType subtypeNpgsqlDbType;
            if (subtypeMapping is INpgsqlTypeMapping npgsqlTypeMapping)
            {
                subtypeNpgsqlDbType = npgsqlTypeMapping.NpgsqlDbType;
            }
            else
            {
                // We're using a built-in, non-Npgsql mapping such as IntTypeMapping.
                // Infer the NpgsqlDbType from the DbType (somewhat hacky but why not).
                Debug.Assert(subtypeMapping.DbType.HasValue);
                var p = new NpgsqlParameter { DbType = subtypeMapping.DbType.Value };
                subtypeNpgsqlDbType = p.NpgsqlDbType;
            }

            return NpgsqlDbType.Range | subtypeNpgsqlDbType;
        }

        public override Expression GenerateCodeLiteral(object value)
            => GenerateCodeLiteral(value, ClrType, SubtypeMapping.ClrType);

        internal static Expression GenerateCodeLiteral(object value, Type rangeType, Type subtypeType)
        {
            Debug.Assert(rangeType == typeof(NpgsqlRange<>).MakeGenericType(subtypeType));

            var lower = rangeType.GetProperty(nameof(NpgsqlRange<int>.LowerBound))!.GetValue(value);
            var upper = rangeType.GetProperty(nameof(NpgsqlRange<int>.UpperBound))!.GetValue(value);
            var lowerInfinite = (bool)rangeType.GetProperty(nameof(NpgsqlRange<int>.LowerBoundInfinite))!.GetValue(value)!;
            var upperInfinite = (bool)rangeType.GetProperty(nameof(NpgsqlRange<int>.UpperBoundInfinite))!.GetValue(value)!;
            var lowerInclusive = (bool)rangeType.GetProperty(nameof(NpgsqlRange<int>.LowerBoundIsInclusive))!.GetValue(value)!;
            var upperInclusive = (bool)rangeType.GetProperty(nameof(NpgsqlRange<int>.UpperBoundIsInclusive))!.GetValue(value)!;

            return lowerInfinite || upperInfinite
                ? Expression.New(
                    rangeType.GetConstructor(new[] { subtypeType, typeof(bool), typeof(bool), subtypeType, typeof(bool), typeof(bool) })!,
                    Expression.Constant(lower),
                    Expression.Constant(lowerInclusive),
                    Expression.Constant(lowerInfinite),
                    Expression.Constant(upper),
                    Expression.Constant(upperInclusive),
                    Expression.Constant(upperInfinite))
                : lowerInclusive && upperInclusive
                    ? Expression.New(
                        rangeType.GetConstructor(new[] { subtypeType, subtypeType })!,
                        Expression.Constant(lower),
                        Expression.Constant(upper))
                    : Expression.New(
                        rangeType.GetConstructor(new[] { subtypeType, typeof(bool), subtypeType, typeof(bool) })!,
                        Expression.Constant(lower),
                        Expression.Constant(lowerInclusive),
                        Expression.Constant(upper),
                        Expression.Constant(upperInclusive));
        }
    }
}
