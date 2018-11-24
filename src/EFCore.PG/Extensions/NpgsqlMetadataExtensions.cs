using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlMetadataExtensions
    {
        public static INpgsqlEntityTypeAnnotations Npgsql([NotNull] this IEntityType entityType)
            => new NpgsqlEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        public static NpgsqlEntityTypeAnnotations Npgsql([NotNull] this IMutableEntityType entityType)
            => (NpgsqlEntityTypeAnnotations)Npgsql((IEntityType)entityType);

        public static IRelationalForeignKeyAnnotations Npgsql([NotNull] this IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)));

        public static RelationalForeignKeyAnnotations Npgsql([NotNull] this IMutableForeignKey foreignKey)
            => (RelationalForeignKeyAnnotations)Npgsql((IForeignKey)foreignKey);

        public static INpgsqlIndexAnnotations Npgsql([NotNull] this IIndex index)
            => new NpgsqlIndexAnnotations(Check.NotNull(index, nameof(index)));

        public static NpgsqlIndexAnnotations Npgsql([NotNull] this IMutableIndex index)
            => (NpgsqlIndexAnnotations)Npgsql((IIndex)index);

        public static IRelationalKeyAnnotations Npgsql([NotNull] this IKey key)
            => new RelationalKeyAnnotations(Check.NotNull(key, nameof(key)));

        public static RelationalKeyAnnotations Npgsql([NotNull] this IMutableKey key)
            => (RelationalKeyAnnotations)Npgsql((IKey)key);

        public static INpgsqlModelAnnotations Npgsql([NotNull] this IModel model)
            => new NpgsqlModelAnnotations(Check.NotNull(model, nameof(model)));

        public static NpgsqlModelAnnotations Npgsql([NotNull] this IMutableModel model)
            => (NpgsqlModelAnnotations)Npgsql((IModel)model);

        public static INpgsqlPropertyAnnotations Npgsql([NotNull] this IProperty property)
            => new NpgsqlPropertyAnnotations(Check.NotNull(property, nameof(property)));

        public static NpgsqlPropertyAnnotations Npgsql([NotNull] this IMutableProperty property)
            => (NpgsqlPropertyAnnotations)Npgsql((IProperty)property);

        public static NpgsqlAlterDatabaseOperationAnnotations Npgsql([NotNull] this AlterDatabaseOperation operation)
            => new NpgsqlAlterDatabaseOperationAnnotations(Check.NotNull(operation, nameof(operation)));

        public static NpgsqlDatabaseModelAnnotations Npgsql([NotNull] this DatabaseModel model)
            => new NpgsqlDatabaseModelAnnotations(Check.NotNull(model, nameof(model)));
    }
}
