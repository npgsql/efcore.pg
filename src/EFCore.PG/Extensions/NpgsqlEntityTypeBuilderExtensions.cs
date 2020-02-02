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

        /// <summary>
        /// Configures using the auto-updating system column <c>xmin</c> as the optimistic concurrency token.
        /// </summary>
        /// <remarks>
        /// See http://www.npgsql.org/efcore/miscellaneous.html#optimistic-concurrency-and-concurrency-tokens
        /// </remarks>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static EntityTypeBuilder UseXminAsConcurrencyToken(
            [NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Property<uint>("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            return entityTypeBuilder;
        }

        /// <summary>
        /// Configures using the auto-updating system column <c>xmin</c> as the optimistic concurrency token.
        /// </summary>
        /// <remarks>
        /// See http://www.npgsql.org/efcore/miscellaneous.html#optimistic-concurrency-and-concurrency-tokens
        /// </remarks>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static EntityTypeBuilder<TEntity> UseXminAsConcurrencyToken<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)UseXminAsConcurrencyToken((EntityTypeBuilder)entityTypeBuilder);

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
        public static EntityTypeBuilder SetStorageParameter(
            [NotNull] this EntityTypeBuilder entityTypeBuilder, string parameterName, object parameterValue)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Metadata.SetStorageParameter(parameterName, parameterValue);

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
        public static EntityTypeBuilder<TEntity> SetStorageParameter<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder, string parameterName, object parameterValue)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)SetStorageParameter((EntityTypeBuilder)entityTypeBuilder, parameterName, parameterValue);

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
        public static IConventionEntityTypeBuilder SetStorageParameter(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, string parameterName, object parameterValue, bool fromDataAnnotation = false)
        {
            if (entityTypeBuilder.CanSetStorageParameter(parameterName, parameterValue, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetStorageParameter(parameterName, parameterValue);

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
        public static bool CanSetStorageParameter(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, string parameterName, object parameterValue, bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(NpgsqlAnnotationNames.StorageParameterPrefix + parameterName, parameterValue, fromDataAnnotation);
        }

        #endregion Storage parameters

        #region Unlogged Table

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
        public static EntityTypeBuilder IsUnlogged(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            bool isUnlogged = true)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Metadata.SetIsUnlogged(isUnlogged);

            return entityTypeBuilder;
        }

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
        public static EntityTypeBuilder<TEntity> IsUnlogged<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            bool isUnlogged = true)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)IsUnlogged((EntityTypeBuilder)entityTypeBuilder, isUnlogged);

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
        public static IConventionEntityTypeBuilder IsUnlogged(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            bool isUnlogged = true,
            bool fromDataAnnotation = false)
        {
            if (entityTypeBuilder.CanSetIsUnlogged(isUnlogged, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetIsUnlogged(isUnlogged, fromDataAnnotation);

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
        public static bool CanSetIsUnlogged(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            bool isUnlogged = true,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(NpgsqlAnnotationNames.UnloggedTable, isUnlogged, fromDataAnnotation);
        }

        #endregion

        #region CockroachDB Interleave-in-parent

        public static EntityTypeBuilder UseCockroachDbInterleaveInParent(
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

            var interleaveInParent = entityTypeBuilder.Metadata.GetCockroachDbInterleaveInParent();
            interleaveInParent.ParentTableSchema = parentEntity.GetSchema();
            interleaveInParent.ParentTableName = parentEntity.GetTableName();
            interleaveInParent.InterleavePrefix = interleavePrefix;

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> UseCockroachDbInterleaveInParent<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] Type parentTableType,
            [NotNull] List<string> interleavePrefix)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)UseCockroachDbInterleaveInParent((EntityTypeBuilder)entityTypeBuilder, parentTableType, interleavePrefix);

        #endregion CockroachDB Interleave-in-parent
    }
}
