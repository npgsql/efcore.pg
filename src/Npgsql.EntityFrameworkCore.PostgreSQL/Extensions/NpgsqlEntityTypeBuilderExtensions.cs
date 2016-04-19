// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlEntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder ForNpgsqlToTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            entityTypeBuilder.Metadata.Npgsql().TableName = name;

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ForNpgsqlToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ForNpgsqlToTable((EntityTypeBuilder)entityTypeBuilder, name);

        public static EntityTypeBuilder ForNpgsqlToTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var relationalEntityTypeAnnotations = entityTypeBuilder.Metadata.Npgsql();
            relationalEntityTypeAnnotations.TableName = name;
            relationalEntityTypeAnnotations.Schema = schema;

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ForNpgsqlToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ForNpgsqlToTable((EntityTypeBuilder)entityTypeBuilder, name, schema);
    }
}
