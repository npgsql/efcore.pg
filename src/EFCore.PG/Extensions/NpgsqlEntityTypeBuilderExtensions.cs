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

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlEntityTypeBuilderExtensions
    {
        #region xmin

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

        #endregion xmin

        #region Storage parameters

        /// <summary>
        /// Sets a PostgreSQL storage parameter on the table created for this entity.
        /// </summary>
        /// <remarks>
        /// See https://www.postgresql.org/docs/current/static/sql-createtable.html#SQL-CREATETABLE-STORAGE-PARAMETERS
        /// </remarks>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="parameterName"> The name of the storage parameter. </param>
        /// <param name="parameterValue"> The value of the storage parameter. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ForNpgsqlSetStorageParameter(
            [NotNull] this EntityTypeBuilder entityTypeBuilder, string parameterName, object parameterValue)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Metadata.Npgsql().SetStorageParameter(parameterName, parameterValue);

            return entityTypeBuilder;
        }

        /// <summary>
        /// Sets a PostgreSQL storage parameter on the table created for this entity.
        /// </summary>
        /// <remarks>
        /// See https://www.postgresql.org/docs/current/static/sql-createtable.html#SQL-CREATETABLE-STORAGE-PARAMETERS
        /// </remarks>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="parameterName"> The name of the storage parameter. </param>
        /// <param name="parameterValue"> The value of the storage parameter. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ForNpgsqlSetStorageParameter<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder, string parameterName, object parameterValue)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ForNpgsqlSetStorageParameter((EntityTypeBuilder)entityTypeBuilder, parameterName, parameterValue);

        #endregion Storage parameters

        #region Comment

        /// <summary>
        ///     Configures the comment set on the table when targeting Npgsql.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="comment"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ForNpgsqlHasComment(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string comment)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(comment, nameof(comment));

            entityTypeBuilder.Metadata.Npgsql().Comment = comment;

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the comment set on the table when targeting Npgsql.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="comment"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ForNpgsqlHasComment<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string comment)
            where TEntity : class
        => (EntityTypeBuilder<TEntity>)ForNpgsqlHasComment((EntityTypeBuilder)entityTypeBuilder, comment);

        #endregion Comment

        #region CockroachDB Interleave-in-parent

        public static EntityTypeBuilder ForCockroachDbInterleaveInParent(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] Type parentTableType,
            [NotNull] List<string> interleavePrefix)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(parentTableType, nameof(parentTableType));
            Check.NotNull(interleavePrefix, nameof(interleavePrefix));

            var parentEntity = entityTypeBuilder.Metadata.Model.FindEntityType(parentTableType);
            if (parentEntity == null)
                throw new Exception("Entity not found in model: " + parentTableType);

            var interleaveInParent = entityTypeBuilder.Metadata.Npgsql().CockroachDbInterleaveInParent;
            interleaveInParent.ParentTableSchema = parentEntity.Relational().Schema;
            interleaveInParent.ParentTableName = parentEntity.Relational().TableName;
            interleaveInParent.InterleavePrefix = interleavePrefix;

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ForCockroachDbInterleaveInParent<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] Type parentTableType,
            [NotNull] List<string> interleavePrefix)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ForCockroachDbInterleaveInParent((EntityTypeBuilder)entityTypeBuilder, parentTableType, interleavePrefix);

        #endregion CockroachDB Interleave-in-parent
    }
}
