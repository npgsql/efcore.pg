using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

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
        public RelationalTypeMapping ElementMapping { get; }

        protected NpgsqlArrayTypeMapping(RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping)
            : base(parameters)
            => ElementMapping = elementMapping;

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
    }
}
