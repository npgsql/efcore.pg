using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The type mapping for the PostgreSQL 'character' data type.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/datatype-character.html
    /// </remarks>
    /// <inheritdoc />
    public class NpgsqlCharacterTypeMapping : StringTypeMapping
    {
        /// <summary>
        /// Static <see cref="ValueComparer{T}"/> for fixed-width character types.
        /// </summary>
        /// <remarks>
        /// Comparisons of 'character' data as defined in the SQL standard
        /// differ dramatically from CLR string comparisons. This value comparer
        /// adjusts for this by only comparing strings after truncating trailing
        /// whitespace.
        /// </remarks>
        [NotNull] static readonly ValueComparer<string> CharacterValueComparer =
            new ValueComparer<string>(
                (x, y) => x != null && y != null && x.AsSpan().TrimEnd() == y.AsSpan().TrimEnd(),
                x => x != null ? x.TrimEnd().GetHashCode() : 0);

        public override ValueComparer Comparer => CharacterValueComparer;

        public override ValueComparer KeyComparer => CharacterValueComparer;

        public NpgsqlCharacterTypeMapping([NotNull] string storeType, int? size = null)
            : this(new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(string)),
                storeType,
                size == null ? StoreTypePostfix.None : StoreTypePostfix.Size,
                System.Data.DbType.StringFixedLength,
                false,
                size,
                true)) {}

        protected NpgsqlCharacterTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlCharacterTypeMapping(new RelationalTypeMappingParameters(
                parameters.CoreParameters,
                parameters.StoreType,
                parameters.Size == null ? StoreTypePostfix.None : StoreTypePostfix.Size,
                parameters.DbType,
                parameters.Unicode,
                parameters.Size,
                parameters.FixedLength,
                parameters.Precision,
                parameters.Scale));

        protected override void ConfigureParameter(DbParameter parameter)
        {
            if (parameter.Value is string value)
                parameter.Value = value.TrimEnd();

            base.ConfigureParameter(parameter);
        }
    }
}
