using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
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

            entityTypeBuilder.Metadata.SetNpgsqlStorageParameter(parameterName, parameterValue);

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

        /// <summary>
        /// Sets a PostgreSQL storage parameter on the table created for this entity.
        /// </summary>
        /// <remarks>
        /// See https://www.postgresql.org/docs/current/static/sql-createtable.html#SQL-CREATETABLE-STORAGE-PARAMETERS
        /// </remarks>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="parameterName"> The name of the storage parameter. </param>
        /// <param name="parameterValue"> The value of the storage parameter. </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IConventionEntityTypeBuilder ForNpgsqlSetStorageParameter(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, string parameterName, object parameterValue, bool fromDataAnnotation = false)
        {
            if (entityTypeBuilder.ForNpgsqlCanSetStorageParameter(parameterName, parameterValue, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetNpgsqlStorageParameter(parameterName, parameterValue);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the PostgreSQL storage parameter on the table created for this entity.
        /// </summary>
        /// <remarks>
        /// See https://www.postgresql.org/docs/current/static/sql-createtable.html#SQL-CREATETABLE-STORAGE-PARAMETERS
        /// </remarks>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="parameterName"> The name of the storage parameter. </param>
        /// <param name="parameterValue"> The value of the storage parameter. </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the mapped table can be configured as with the storage parameter.</returns>
        public static bool ForNpgsqlCanSetStorageParameter(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, string parameterName, object parameterValue, bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(NpgsqlAnnotationNames.StorageParameterPrefix + parameterName, parameterValue, fromDataAnnotation);
        }

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

            entityTypeBuilder.Metadata.SetNpgsqlComment(comment);

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

        // ReSharper disable once CommentTypo
        /// <summary>
        /// Configures the entity to use an unlogged table when targeting Npgsql.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <param name="isUnlogged">True to configure the entity to use an unlogged table; otherwise, false.</param>
        /// <returns>
        /// The same builder instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-UNLOGGED
        /// </remarks>
        public static EntityTypeBuilder ForNpgsqlIsUnlogged(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            bool isUnlogged = true)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Metadata.SetNpgsqlIsUnlogged(isUnlogged);

            return entityTypeBuilder;
        }

        // ReSharper disable once CommentTypo
        /// <summary>
        /// Configures the mapped table to use an unlogged table when targeting Npgsql.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <param name="isUnlogged">True to configure the entity to use an unlogged table; otherwise, false.</param>
        /// <returns>
        /// The same builder instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-UNLOGGED
        /// </remarks>
        public static EntityTypeBuilder<TEntity> ForNpgsqlIsUnlogged<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            bool isUnlogged = true)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ForNpgsqlIsUnlogged((EntityTypeBuilder)entityTypeBuilder, isUnlogged);

        /// <summary>
        /// Configures the mapped table to use an unlogged table when targeting Npgsql.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <param name="isUnlogged">True to configure the entity to use an unlogged table; otherwise, false.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        /// The same builder instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-UNLOGGED
        /// </remarks>
        public static IConventionEntityTypeBuilder ForNpgsqlIsUnlogged(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            bool isUnlogged = true,
            bool fromDataAnnotation = false)
        {
            if (entityTypeBuilder.ForNpgsqlCanSetIsUnlogged(isUnlogged, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetNpgsqlIsUnlogged(isUnlogged, fromDataAnnotation);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the mapped table can be configured to use an unlogged table when targeting Npgsql.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <param name="isUnlogged">True to configure the entity to use an unlogged table; otherwise, false.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        /// The same builder instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-UNLOGGED
        /// </remarks>
        public static bool ForNpgsqlCanSetIsUnlogged(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            bool isUnlogged = true,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(NpgsqlAnnotationNames.UnloggedTable, isUnlogged, fromDataAnnotation);
        }

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
                throw new ArgumentException("Entity not found in model for type: " + parentTableType, nameof(parentTableType));

            var interleaveInParent = entityTypeBuilder.Metadata.GetNpgsqlCockroachDbInterleaveInParent();
            interleaveInParent.ParentTableSchema = parentEntity.GetSchema();
            interleaveInParent.ParentTableName = parentEntity.GetTableName();
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
