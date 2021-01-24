using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

#nullable enable

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

        protected NpgsqlArrayTypeMapping(
            RelationalTypeMappingParameters parameters, [NotNull] RelationalTypeMapping elementMapping)
            : base(parameters)
        {
            ElementMapping = elementMapping;

            // If the element mapping has an NpgsqlDbType or DbType, set our own NpgsqlDbType as an array of that.
            // Otherwise let the ADO.NET layer infer the PostgreSQL type. We can't always let it infer, otherwise
            // when given a byte[] it will infer byte (but we want smallint[])
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Array |
                           (elementMapping is NpgsqlTypeMapping elementNpgsqlTypeMapping
                               ? elementNpgsqlTypeMapping.NpgsqlDbType
                               : elementMapping.DbType.HasValue
                                   ? new NpgsqlParameter { DbType = elementMapping.DbType.Value }.NpgsqlDbType
                                   : default(NpgsqlDbType?));
        }

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
