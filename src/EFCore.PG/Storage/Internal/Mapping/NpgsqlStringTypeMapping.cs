using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    // TODO: See #357. We should be able to simply use StringTypeMapping but DbParameter.Size isn't managed properly.
    public class NpgsqlStringTypeMapping : StringTypeMapping
    {
        /// <summary>
        /// Static <see cref="ValueComparer{T}"/> for fixed-width character types.
        /// </summary>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/static/datatype-character.html
        /// </remarks>
        [NotNull] static readonly ValueComparer<string> CharacterValueComparer =
            new ValueComparer<string>(
                (x, y) => x != null && y != null && x.AsSpan().TrimEnd() == y.AsSpan().TrimEnd(),
                x => x != null ? x.TrimEnd().GetHashCode() : 0);

        /// <inheritdoc />
        public override ValueComparer Comparer { get; }

        /// <inheritdoc />
        public override ValueComparer KeyComparer { get; }

        public NpgsqlStringTypeMapping(
            [NotNull] string storeType,
            DbType dbType = System.Data.DbType.String,
            bool unicode = false,
            int? size = null)
            : base(storeType, dbType, unicode, size)
        {
            if (storeType == "character" && dbType == System.Data.DbType.StringFixedLength)
            {
                Comparer = CharacterValueComparer;
                KeyComparer = CharacterValueComparer;
            }
        }

        /// <inheritdoc />
        protected NpgsqlStringTypeMapping(
            RelationalTypeMappingParameters parameters,
            ValueComparer comparer,
            ValueComparer keyComparer)
            : base(parameters)
        {
            Comparer = comparer;
            KeyComparer = keyComparer;
        }

        /// <inheritdoc />
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlStringTypeMapping(parameters, Comparer, KeyComparer);

        /// <inheritdoc />
        protected override void ConfigureParameter(DbParameter parameter)
        {
            if (parameter.DbType == System.Data.DbType.StringFixedLength)
                parameter.Value = (parameter.Value as string)?.TrimEnd() ?? string.Empty;

            // See #357
            if (Size.HasValue)
            {
                var value = parameter.Value;
                var length = (value as string)?.Length;
                var size = Size.Value;

                parameter.Size = value == null || value == DBNull.Value || length != null && length <= size
                    ? size
                    : -1;
            }
        }
    }
}
