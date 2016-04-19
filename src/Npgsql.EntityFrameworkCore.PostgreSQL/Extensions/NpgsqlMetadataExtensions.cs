using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlMetadataExtensions
    {
        public static IRelationalEntityTypeAnnotations Npgsql([NotNull] this IEntityType entityType)
            => new RelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)), NpgsqlFullAnnotationNames.Instance);

        public static RelationalEntityTypeAnnotations Npgsql([NotNull] this IMutableEntityType entityType)
            => (RelationalEntityTypeAnnotations)Npgsql((IEntityType)entityType);

        public static IRelationalForeignKeyAnnotations Npgsql([NotNull] this IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), NpgsqlFullAnnotationNames.Instance);

        public static RelationalForeignKeyAnnotations Npgsql([NotNull] this IMutableForeignKey foreignKey)
            => (RelationalForeignKeyAnnotations)Npgsql((IForeignKey)foreignKey);

        public static INpgsqlIndexAnnotations Npgsql([NotNull] this IIndex index)
            => new NpgsqlIndexAnnotations(Check.NotNull(index, nameof(index)));

        public static RelationalIndexAnnotations Npgsql([NotNull] this IMutableIndex index)
            => (NpgsqlIndexAnnotations)Npgsql((IIndex)index);

        public static IRelationalKeyAnnotations Npgsql([NotNull] this IKey key)
            => new RelationalKeyAnnotations(Check.NotNull(key, nameof(key)), NpgsqlFullAnnotationNames.Instance);

        public static RelationalKeyAnnotations Npgsql([NotNull] this IMutableKey key)
            => (RelationalKeyAnnotations)Npgsql((IKey)key);

        public static IRelationalModelAnnotations Npgsql([NotNull] this IModel model)
            => new RelationalModelAnnotations(Check.NotNull(model, nameof(model)), NpgsqlFullAnnotationNames.Instance);

        public static RelationalModelAnnotations Npgsql([NotNull] this IMutableModel model)
            => (RelationalModelAnnotations)Npgsql((IModel)model);

        public static IRelationalPropertyAnnotations Npgsql([NotNull] this IProperty property)
            => new RelationalPropertyAnnotations(Check.NotNull(property, nameof(property)), NpgsqlFullAnnotationNames.Instance);

        public static RelationalPropertyAnnotations Npgsql([NotNull] this IMutableProperty property)
            => (RelationalPropertyAnnotations)Npgsql((IProperty)property);
    }
}
