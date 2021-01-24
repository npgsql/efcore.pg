using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

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
        [NotNull] readonly ISqlGenerationHelper _sqlGenerationHelper;

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        /// The relational type mapping used to initialize the bound mapping.
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
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [NotNull] RelationalTypeMapping subtypeMapping,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : this(storeType, null, clrType, subtypeMapping, sqlGenerationHelper) {}

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlRangeTypeMapping"/> class.
        /// </summary>
        /// <param name="storeType">The database type to map</param>
        /// <param name="storeTypeSchema">The schema of the type.</param>
        /// <param name="clrType">The CLR type to map.</param>
        /// <param name="subtypeMapping">The type mapping for the range subtype.</param>
        /// <param name="sqlGenerationHelper">The SQL generation helper to delimit the store name.</param>
        public NpgsqlRangeTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] string storeTypeSchema,
            [NotNull] Type clrType,
            [NotNull] RelationalTypeMapping subtypeMapping,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : base(sqlGenerationHelper.DelimitIdentifier(storeType, storeTypeSchema), clrType, GenerateNpgsqlDbType(subtypeMapping))
        {
            SubtypeMapping = subtypeMapping;
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        protected NpgsqlRangeTypeMapping(
            RelationalTypeMappingParameters parameters,
            NpgsqlDbType npgsqlDbType,
            [NotNull] RelationalTypeMapping subtypeMapping,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : base(parameters, npgsqlDbType)
        {
            SubtypeMapping = subtypeMapping;
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        [NotNull]
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlRangeTypeMapping(parameters, NpgsqlDbType, SubtypeMapping, _sqlGenerationHelper);

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
