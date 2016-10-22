#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

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

        public static EntityTypeBuilder ForNpgsqlUseXminAsConcurrencyToken(
            [NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Property<uint>("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ForNpgsqlUseXminAsConcurrencyToken<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ForNpgsqlUseXminAsConcurrencyToken((EntityTypeBuilder)entityTypeBuilder);
    }
}
