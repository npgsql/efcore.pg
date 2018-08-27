using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <inheritdoc />
    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/datatype-character.html
    /// </remarks>
    public class NpgsqlCharacterTypeMapping : NpgsqlStringTypeMapping
    {
        /// <summary>
        /// Static <see cref="ValueComparer{T}"/> for fixed-width character types.
        /// </summary>
        [NotNull] static readonly ValueComparer<string> CharacterValueComparer =
            new ValueComparer<string>(
                (x, y) => x != null && y != null && x.AsSpan().TrimEnd() == y.AsSpan().TrimEnd(),
                x => x != null ? x.TrimEnd().GetHashCode() : 0);

        public override ValueComparer Comparer => CharacterValueComparer;

        public override ValueComparer KeyComparer => CharacterValueComparer;

        public NpgsqlCharacterTypeMapping([NotNull] string storeType, bool unicode = false, int? size = null)
            : base(storeType, unicode, size) {}

        protected NpgsqlCharacterTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlCharacterTypeMapping(parameters);

        protected override void ConfigureParameter(DbParameter parameter)
        {
            var value = parameter.Value as string;

            if (value != null)
                parameter.Value = value.TrimEnd();

            // See #357
            if (Size.HasValue)
                parameter.Size = parameter.Value == DBNull.Value || value == null || value.Length <= Size.Value ? Size.Value : -1;
        }
    }
}
