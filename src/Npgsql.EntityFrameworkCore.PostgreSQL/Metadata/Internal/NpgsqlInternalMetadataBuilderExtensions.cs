// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class NpgsqlInternalMetadataBuilderExtensions
    {
        public static RelationalModelBuilderAnnotations Npgsql(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalModelBuilderAnnotations(builder, configurationSource, NpgsqlFullAnnotationNames.Instance);

        public static RelationalPropertyBuilderAnnotations Npgsql(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalPropertyBuilderAnnotations(builder, configurationSource, NpgsqlFullAnnotationNames.Instance);

        public static RelationalEntityTypeBuilderAnnotations Npgsql(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeBuilderAnnotations(builder, configurationSource, NpgsqlFullAnnotationNames.Instance);

        public static RelationalKeyBuilderAnnotations Npgsql(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalKeyBuilderAnnotations(builder, configurationSource, NpgsqlFullAnnotationNames.Instance);

        public static RelationalIndexBuilderAnnotations Npgsql(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalIndexBuilderAnnotations(builder, configurationSource, NpgsqlFullAnnotationNames.Instance);

        public static RelationalForeignKeyBuilderAnnotations Npgsql(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyBuilderAnnotations(builder, configurationSource, NpgsqlFullAnnotationNames.Instance);
    }
}
