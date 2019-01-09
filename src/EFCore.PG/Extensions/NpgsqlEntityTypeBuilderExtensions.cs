using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        #region Unlogged Table

        /// <summary>
        /// Configures the entity to use an unlogged table when targeting Npgsql.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <param name="unlogged">True to configure the entity to use an unlogged table; otherwise, false.</param>
        /// <returns>
        /// The same builder instance so that multiple calls can be chained.
        /// </returns>
        public static EntityTypeBuilder ForNpgsqlHasUnloggedTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            bool unlogged = true)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Metadata.Npgsql().UnloggedTable = unlogged;

            return entityTypeBuilder;
        }

        /// <summary>
        /// Configures the entity to use an unlogged table when targeting Npgsql.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <param name="unlogged">True to configure the entity to use an unlogged table; otherwise, false.</param>
        /// <returns>
        /// The same builder instance so that multiple calls can be chained.
        /// </returns>
        public static EntityTypeBuilder<TEntity> ForNpgsqlHasUnloggedTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            bool unlogged = true)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ForNpgsqlHasUnloggedTable((EntityTypeBuilder)entityTypeBuilder, unlogged);

        #endregion

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

        #region Generic Index

        /// <summary>
        /// Configures an index on the specified properties. If there is an existing index on the given
        /// set of properties, then the existing index will be returned for configuration.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="indexExpression">
        ///   <para>
        ///     A lambda expression representing the property(s) to be included in the index
        ///     (<c>blog => blog.Url</c>).
        ///   </para>
        ///   <para>
        ///     If the index is made up of multiple properties then specify an anonymous type including the
        ///     properties (<c>post => new { post.Title, post.BlogId }</c>).
        ///   </para>
        /// </param>
        /// <returns> An object that can be used to configure the index. </returns>
        public static IndexBuilder<TEntity> ForNpgsqlHasIndex<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] Expression<Func<TEntity, object>> indexExpression)
            where TEntity : class
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(indexExpression, nameof(indexExpression));

            var builder = ((IInfrastructure<InternalEntityTypeBuilder>)entityTypeBuilder).GetInfrastructure();

            return new IndexBuilder<TEntity>(
                builder.HasIndex(indexExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
        }

        #endregion
    }
}
