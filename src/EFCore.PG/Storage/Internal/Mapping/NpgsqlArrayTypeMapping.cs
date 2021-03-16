using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// Abstract base class for PostgreSQL array mappings (i.e. CLR array and <see cref="List{T}"/>.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/arrays.html
    /// </remarks>
    public abstract class NpgsqlArrayTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        /// The relational type mapping used to initialize the array mapping.
        /// </summary>
        [NotNull]
        public virtual RelationalTypeMapping ElementMapping { get; }

        /// <summary>
        /// The database type used by Npgsql.
        /// </summary>
        public virtual NpgsqlDbType? NpgsqlDbType { get; }

        /// <summary>
        /// Whether the array's element is nullable. This is required since <see cref="Type"/> and <see cref="ElementMapping"/> do not
        /// contain nullable reference type information.
        /// </summary>
        public virtual bool IsElementNullable { get; }

        protected NpgsqlArrayTypeMapping(
            RelationalTypeMappingParameters parameters, [NotNull] RelationalTypeMapping elementMapping, bool isElementNullable)
            : base(parameters)
        {
            ElementMapping = elementMapping;
            IsElementNullable = isElementNullable;

            // If the element mapping has an NpgsqlDbType or DbType, set our own NpgsqlDbType as an array of that.
            // Otherwise let the ADO.NET layer infer the PostgreSQL type. We can't always let it infer, otherwise
            // when given a byte[] it will infer byte (but we want smallint[])
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Array |
                           (elementMapping is INpgsqlTypeMapping elementNpgsqlTypeMapping
                               ? elementNpgsqlTypeMapping.NpgsqlDbType
                               : elementMapping.DbType.HasValue
                                   ? new NpgsqlParameter { DbType = elementMapping.DbType.Value }.NpgsqlDbType
                                   : default(NpgsqlDbType?));
        }

        /// <summary>
        /// Returns a copy of this type mapping with <see cref="IsElementNullable"/> set to <see langword="false"/>.
        /// </summary>
        public abstract NpgsqlArrayTypeMapping MakeNonNullable();

        public override CoreTypeMapping Clone(ValueConverter? converter)
        {
            // When the mapping is cloned to apply a value converter, we need to also apply that value converter to the element, otherwise
            // we end up with an array mapping over a converter-less element mapping. This is important in some inference situations.
            // If the array converter was properly set up, it's a INpgsqlArrayConverter with a reference to its element's converter.
            // Just clone the element's mapping with that (same with the null converter case).
            if (converter is INpgsqlArrayConverter or null)
            {
                return Clone(
                    Parameters.WithComposedConverter(converter),
                    (RelationalTypeMapping)ElementMapping.Clone(converter is INpgsqlArrayConverter arrayConverter
                        ? arrayConverter.ElementConverter
                        : null));
            }

            throw new NotSupportedException(
                $"Value converters for array or List properties must be configured via {nameof(NpgsqlPropertyBuilderExtensions.HasPostgresArrayConversion)}.");
        }

        protected abstract RelationalTypeMapping Clone(
            RelationalTypeMappingParameters parameters,
            [NotNull] RelationalTypeMapping elementMapping);

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => Clone(parameters, ElementMapping);

        // The array-to-array mapping needs to know how to generate an SQL literal for a List<>, and
        // the list-to-array mapping needs to know how to generate an SQL literal for an array.
        // This is because in cases such as ctx.SomeListColumn.SequenceEquals(new[] { 1, 2, 3}), the list mapping
        // from the left side gets applied to the right side.
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var type = value.GetType();

            if (!type.IsArray && !type.IsGenericList())
                throw new ArgumentException("Parameter must be an array or List<>", nameof(value));
            if (value is Array array && array.Rank != 1)
                throw new NotSupportedException("Multidimensional array literals aren't supported");

            var list = (IList)value;

            var sb = new StringBuilder();
            sb.Append("ARRAY[");
            for (var i = 0; i < list.Count; i++)
            {
                sb.Append(ElementMapping.GenerateSqlLiteral(list[i]));
                if (i < list.Count - 1)
                    sb.Append(",");
            }

            sb.Append("]::");
            sb.Append(ElementMapping.StoreType);
            sb.Append("[]");
            return sb.ToString();
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            var npgsqlParameter = parameter as NpgsqlParameter;
            if (npgsqlParameter == null)
                throw new ArgumentException($"Npgsql-specific type mapping {GetType()} being used with non-Npgsql parameter type {parameter.GetType().Name}");

            base.ConfigureParameter(parameter);

            if (NpgsqlDbType.HasValue)
                npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Value;
        }

        private protected static bool CalculateElementNullability(Type elementType, bool? isElementNullable)
        {
            if (elementType.IsValueType)
            {
                // If elementType is a value type, we can infer its nullability from the type - make sure isElementNullable wasn't given
                // externally.
                if (isElementNullable is not null)
                {
                    throw new ArgumentException($"{nameof(isElementNullable)} must be null for arrays over value types");
                }

                return elementType.IsNullableType();
            }

            return isElementNullable ?? true;
        }

        protected class NullableEqualityComparer<T> : IEqualityComparer<T?>
            where T : struct
        {
            readonly IEqualityComparer<T> _underlyingComparer;

            public NullableEqualityComparer(IEqualityComparer<T> underlyingComparer)
                => _underlyingComparer = underlyingComparer;

            public bool Equals(T? x, T? y)
                => x is null
                    ? y is null
                    : y.HasValue && _underlyingComparer.Equals(x.Value, y.Value);

            public int GetHashCode(T? obj)
                => obj is null ? 0 : _underlyingComparer.GetHashCode(obj.Value);
        }
    }
}
