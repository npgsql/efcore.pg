using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal
{
    public static class NpgsqlInternalMetadataBuilderExtensions
    {
        public static NpgsqlModelBuilderAnnotations Npgsql(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new NpgsqlModelBuilderAnnotations(builder, configurationSource);

        public static NpgsqlPropertyBuilderAnnotations Npgsql(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new NpgsqlPropertyBuilderAnnotations(builder, configurationSource);

        public static RelationalEntityTypeBuilderAnnotations Npgsql(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeBuilderAnnotations(builder, configurationSource);

        public static RelationalKeyBuilderAnnotations Npgsql(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalKeyBuilderAnnotations(builder, configurationSource);

        public static RelationalIndexBuilderAnnotations Npgsql(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalIndexBuilderAnnotations(builder, configurationSource);

        public static RelationalForeignKeyBuilderAnnotations Npgsql(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyBuilderAnnotations(builder, configurationSource);
    }
}
