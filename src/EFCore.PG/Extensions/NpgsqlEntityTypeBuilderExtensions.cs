using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

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

        #region Generated tsvector column

        // Note: actual configuration for generated TsVector properties is on the property

        /// <summary>
        /// Configures a property on this entity to be a full-text search tsvector column over other given properties.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity being configured.</param>
        /// <param name="tsVectorPropertyExpression">
        /// A lambda expression representing the property to be configured as a tsvector column
        /// (<c>blog => blog.Url</c>).
        /// </param>
        /// <param name="config">
        /// <para>
        /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
        /// generated tsvector property.
        /// </para>
        /// <para>
        /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
        /// </para>
        /// </param>
        /// <param name="includeExpression">
        /// <para>
        /// A lambda expression representing the property(s) to be included in the tsvector column
        /// (<c>blog => blog.Url</c>).
        /// </para>
        /// <para>
        /// If multiple properties are to be included then specify an anonymous type including the
        /// properties (<c>post => new { post.Title, post.BlogId }</c>).
        /// </para>
        /// </param>
        /// <returns>A builder to further configure the property.</returns>
        public static EntityTypeBuilder<TEntity> HasGeneratedTsVectorColumn<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] Expression<Func<TEntity, NpgsqlTsVector>> tsVectorPropertyExpression,
            [NotNull] string config,
            [NotNull] Expression<Func<TEntity, object>> includeExpression)
            where TEntity : class
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(tsVectorPropertyExpression, nameof(tsVectorPropertyExpression));
            Check.NotNull(config, nameof(config));
            Check.NotNull(includeExpression, nameof(includeExpression));

            entityTypeBuilder.Property(tsVectorPropertyExpression).IsGeneratedTsVectorColumn(
                config,
                includeExpression.GetPropertyAccessList().Select(MemberInfoExtensions.GetSimpleMemberName).ToArray());

            return entityTypeBuilder;
        }

        #endregion Generated tsvector column

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
        public static EntityTypeBuilder HasStorageParameter(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] string parameterName,
            [CanBeNull] object parameterValue)
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
        public static EntityTypeBuilder<TEntity> HasStorageParameter<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] string parameterName,
            [CanBeNull] object parameterValue)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)HasStorageParameter((EntityTypeBuilder)entityTypeBuilder, parameterName, parameterValue);

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
        public static IConventionEntityTypeBuilder HasStorageParameter(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] string parameterName,
            [CanBeNull] object parameterValue,
            bool fromDataAnnotation = false)
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
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] string parameterName,
            [CanBeNull] object parameterValue, bool fromDataAnnotation = false)
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
        /// <param name="unlogged">True to configure the entity to use an unlogged table; otherwise, false.</param>
        /// <returns>
        /// The same builder instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-UNLOGGED
        /// </remarks>
        public static EntityTypeBuilder IsUnlogged(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            bool unlogged = true)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Metadata.SetIsUnlogged(unlogged);

            return entityTypeBuilder;
        }

        /// <summary>
        /// Configures the mapped table to use an unlogged table when targeting Npgsql.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <param name="unlogged">True to configure the entity to use an unlogged table; otherwise, false.</param>
        /// <returns>
        /// The same builder instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-UNLOGGED
        /// </remarks>
        public static EntityTypeBuilder<TEntity> IsUnlogged<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            bool unlogged = true)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)IsUnlogged((EntityTypeBuilder)entityTypeBuilder, unlogged);

        /// <summary>
        /// Configures the mapped table to use an unlogged table when targeting Npgsql.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <param name="unlogged">True to configure the entity to use an unlogged table; otherwise, false.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        /// The same builder instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-UNLOGGED
        /// </remarks>
        public static IConventionEntityTypeBuilder IsUnlogged(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            bool unlogged = true,
            bool fromDataAnnotation = false)
        {
            if (entityTypeBuilder.CanSetIsUnlogged(unlogged, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetIsUnlogged(unlogged, fromDataAnnotation);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the mapped table can be configured to use an unlogged table when targeting Npgsql.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <param name="unlogged">True to configure the entity to use an unlogged table; otherwise, false.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        /// The same builder instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-UNLOGGED
        /// </remarks>
        public static bool CanSetIsUnlogged(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            bool unlogged = true,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(NpgsqlAnnotationNames.UnloggedTable, unlogged, fromDataAnnotation);
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

        #region Obsolete

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
        [Obsolete("Use HasStorageParameter")]
        public static EntityTypeBuilder SetStorageParameter(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] string parameterName,
            [CanBeNull] object parameterValue)
            => HasStorageParameter(entityTypeBuilder, parameterName, parameterValue);

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
        [Obsolete("Use HasStorageParameter")]
        public static EntityTypeBuilder<TEntity> SetStorageParameter<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] string parameterName,
            [CanBeNull] object parameterValue)
            where TEntity : class
            => HasStorageParameter(entityTypeBuilder, parameterName, parameterValue);

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
        [Obsolete("Use HasStorageParameter")]
        public static IConventionEntityTypeBuilder SetStorageParameter(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] string parameterName,
            [CanBeNull] object parameterValue,
            bool fromDataAnnotation = false)
            => HasStorageParameter(entityTypeBuilder, parameterName, parameterValue, fromDataAnnotation);

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
        [Obsolete("Use CanSetStorageParameter")]
        public static bool CanSetSetStorageParameter(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] string parameterName,
            [CanBeNull] object parameterValue,
            bool fromDataAnnotation = false)
            => CanSetStorageParameter(entityTypeBuilder, parameterName, parameterValue, fromDataAnnotation);

        #endregion Obsolete
    }
}
