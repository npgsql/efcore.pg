using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Npgsql specific extension methods for <see cref="IndexBuilder" />.
    /// </summary>
    public static class NpgsqlIndexBuilderExtensions
    {
        #region Method

        /// <summary>
        /// The PostgreSQL index method to be used. Null selects the default (currently btree).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="method">The name of the index.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder HasMethod(
            [NotNull] this IndexBuilder indexBuilder,
            [CanBeNull] string method)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(method, nameof(method));

            indexBuilder.Metadata.SetMethod(method);

            return indexBuilder;
        }

        /// <summary>
        /// The PostgreSQL index method to be used. Null selects the default (currently btree).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="method">The name of the index.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder<TEntity> HasMethod<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder,
            [CanBeNull] string method)
            => (IndexBuilder<TEntity>)HasMethod((IndexBuilder)indexBuilder, method);

        /// <summary>
        /// The PostgreSQL index method to be used. Null selects the default (currently btree).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="method">The name of the index.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IConventionIndexBuilder HasMethod(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] string method,
            bool fromDataAnnotation = false)
        {
            if (indexBuilder.CanSetMethod(method, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetMethod(method, fromDataAnnotation);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        /// The PostgreSQL index method to be used. Null selects the default (currently btree).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="method">The name of the index.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the index can be configured with the method</returns>
        public static bool CanSetMethod(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] string method,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.IndexMethod, method, fromDataAnnotation);
        }

        #endregion Method

        #region Operators

        /// <summary>
        /// The PostgreSQL index operators to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="operators">The operators to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder HasOperators(
            [NotNull] this IndexBuilder indexBuilder,
            [CanBeNull] params string[] operators)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(operators, nameof(operators));

            indexBuilder.Metadata.SetOperators(operators);

            return indexBuilder;
        }

        /// <summary>
        /// The PostgreSQL index operators to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="operators">The operators to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder<TEntity> HasOperators<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder,
            [CanBeNull] params string[] operators)
            => (IndexBuilder<TEntity>)HasOperators((IndexBuilder)indexBuilder, operators);

        /// <summary>
        /// The PostgreSQL index operators to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="operators">The operators to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IConventionIndexBuilder HasOperators(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<string> operators,
            bool fromDataAnnotation)
        {
            if (indexBuilder.CanSetOperators(operators, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetOperators(operators, fromDataAnnotation);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the PostgreSQL index operators can be set.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="operators">The operators to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the index can be configured with the method.</returns>
        public static bool CanSetOperators(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<string> operators,
            bool fromDataAnnotation)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.IndexOperators, operators, fromDataAnnotation);
        }

        #endregion Operators

        #region IsTsVectorExpressionIndex

        /// <summary>
        /// Configures this index to be a full-text tsvector expression index.
        /// </summary>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="config">
        /// <para>
        /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
        /// generated tsvector property.
        /// </para>
        /// <para>
        /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
        /// </para>
        /// </param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder IsTsVectorExpressionIndex(
            [NotNull] this IndexBuilder indexBuilder,
            [NotNull] string config)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NotNull(config, nameof(config));

            indexBuilder.Metadata.SetTsVectorConfig(config);
            return indexBuilder;
        }

        /// <summary>
        /// Configures this index to be a full-text tsvector expression index.
        /// </summary>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="config">
        /// <para>
        /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
        /// generated tsvector property.
        /// </para>
        /// <para>
        /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
        /// </para>
        /// </param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder<TEntity> IsTsVectorExpressionIndex<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder,
            [NotNull] string config)
            => (IndexBuilder<TEntity>)IsTsVectorExpressionIndex((IndexBuilder)indexBuilder, config);

        /// <summary>
        /// Configures this index to be a full-text tsvector expression index.
        /// </summary>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="config">
        /// <para>
        /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
        /// generated tsvector property.
        /// </para>
        /// <para>
        /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
        /// </para>
        /// </param>
        /// <returns>
        /// The same builder instance if the configuration was applied,
        /// <c>null</c> otherwise.
        /// </returns>
        public static IConventionIndexBuilder IsTsVectorExpressionIndex(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] string config)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            if (indexBuilder.CanSetIsTsVectorExpressionIndex(config))
            {
                indexBuilder.Metadata.SetTsVectorConfig(config);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the index can be configured as a full-text tsvector expression index.
        /// </summary>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="config">
        /// <para>
        /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
        /// generated tsvector property.
        /// </para>
        /// <para>
        /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
        /// </para>
        /// </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the index can be configured as a full-text tsvector expression index.</returns>
        public static bool CanSetIsTsVectorExpressionIndex(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] string config,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.TsVectorConfig, config, fromDataAnnotation);
        }

        #endregion IsTsVectorExpressionIndex

        #region Collation

        /// <summary>
        /// The PostgreSQL index collation to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort options to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder UseCollation(
            [NotNull] this IndexBuilder indexBuilder,
            [CanBeNull] params string[] values)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(values, nameof(values));

            indexBuilder.Metadata.SetCollation(values);

            return indexBuilder;
        }

        /// <summary>
        /// The PostgreSQL index collation to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort options to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder<TEntity> UseCollation<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder,
            [CanBeNull] params string[] values)
            => (IndexBuilder<TEntity>)UseCollation((IndexBuilder)indexBuilder, values);

        /// <summary>
        /// The PostgreSQL index collation to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort options to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IConventionIndexBuilder UseCollation(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<string> values,
            bool fromDataAnnotation)
        {
            if (indexBuilder.CanSetCollation(values, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetCollation(values, fromDataAnnotation);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the PostgreSQL index collation can be set.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort options to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static bool CanSetCollation(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<string> values,
            bool fromDataAnnotation)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(RelationalAnnotationNames.Collation, values, fromDataAnnotation);
        }

        #endregion Collation

        #region Sort order

        /// <summary>
        /// The PostgreSQL index sort ordering to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder HasSortOrder(
            [NotNull] this IndexBuilder indexBuilder,
            [CanBeNull] params SortOrder[] values)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(values, nameof(values));

            if (!SortOrderHelper.IsDefaultSortOrder(values))
                indexBuilder.Metadata.SetSortOrder(values);

            return indexBuilder;
        }

        /// <summary>
        /// The PostgreSQL index sort ordering to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder<TEntity> HasSortOrder<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder,
            [CanBeNull] params SortOrder[] values)
            => (IndexBuilder<TEntity>)HasSortOrder((IndexBuilder)indexBuilder, values);

        /// <summary>
        /// The PostgreSQL index sort ordering to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IConventionIndexBuilder HasSortOrder(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<SortOrder> values,
            bool fromDataAnnotation)
        {
            if (indexBuilder.CanSetSortOrder(values, fromDataAnnotation))
            {
                Check.NotNull(indexBuilder, nameof(indexBuilder));
                Check.NullButNotEmpty(values, nameof(values));

                if (!SortOrderHelper.IsDefaultSortOrder(values))
                    indexBuilder.Metadata.SetSortOrder(values, fromDataAnnotation);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the PostgreSQL index sort ordering can be set.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static bool CanSetSortOrder(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<SortOrder> values,
            bool fromDataAnnotation)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.IndexSortOrder, values, fromDataAnnotation);
        }

        #endregion Sort order

        #region Null sort order

        /// <summary>
        /// The PostgreSQL index NULL sort ordering to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder HasNullSortOrder(
            [NotNull] this IndexBuilder indexBuilder,
            [CanBeNull] params NullSortOrder[] values)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(values, nameof(values));

            var sortOrders = indexBuilder.Metadata.GetSortOrder();

            if (!SortOrderHelper.IsDefaultNullSortOrder(values, sortOrders))
                indexBuilder.Metadata.SetNullSortOrder(values);

            return indexBuilder;
        }

        /// <summary>
        /// The PostgreSQL index NULL sort ordering to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder<TEntity> HasNullSortOrder<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder,
            [CanBeNull] params NullSortOrder[] values)
            => (IndexBuilder<TEntity>)HasNullSortOrder((IndexBuilder)indexBuilder, values);

        /// <summary>
        /// The PostgreSQL index NULL sort ordering to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IConventionIndexBuilder HasNullSortOrder(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<NullSortOrder> values,
            bool fromDataAnnotation)
        {
            if (indexBuilder.CanSetNullSortOrder(values, fromDataAnnotation))
            {
                var sortOrders = indexBuilder.Metadata.GetSortOrder();

                if (!SortOrderHelper.IsDefaultNullSortOrder(values, sortOrders))
                    indexBuilder.Metadata.SetNullSortOrder(values, fromDataAnnotation);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the PostgreSQL index null sort ordering can be set.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static bool CanSetNullSortOrder(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<NullSortOrder> values,
            bool fromDataAnnotation)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.IndexNullSortOrder, values, fromDataAnnotation);
        }

        #endregion Null sort order

        #region Include

        /// <summary>
        /// Adds an INCLUDE clause to the index definition with the specified property names.
        /// This clause specifies a list of columns which will be included as a non-key part in the index.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="propertyNames">An array of property names to be used in INCLUDE clause.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder IncludeProperties(
            [NotNull] this IndexBuilder indexBuilder,
            [NotNull, ItemNotNull] params string[] propertyNames)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(propertyNames, nameof(propertyNames));

            indexBuilder.Metadata.SetIncludeProperties(propertyNames);

            return indexBuilder;
        }

        /// <summary>
        /// Adds an INCLUDE clause to the index definition with property names from the specified expression.
        /// This clause specifies a list of columns which will be included as a non-key part in the index.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="includeExpression">
        /// <para>
        /// A lambda expression representing the property(s) to be included in the INCLUDE clause
        /// (<c>blog => blog.Url</c>).
        /// </para>
        /// <para>
        /// If multiple properties are to be included then specify an anonymous type including the
        /// properties (<c>post => new { post.Title, post.BlogId }</c>).
        /// </para>
        /// </param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder<TEntity> IncludeProperties<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder,
            [NotNull] Expression<Func<TEntity, object>> includeExpression)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NotNull(includeExpression, nameof(includeExpression));

            indexBuilder.IncludeProperties(includeExpression.GetPropertyAccessList().Select(x => x.Name).ToArray());

            return indexBuilder;
        }

        /// <summary>
        /// Adds an INCLUDE clause to the index definition with the specified property names.
        /// This clause specifies a list of columns which will be included as a non-key part in the index.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="propertyNames">An array of property names to be used in INCLUDE clause.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IConventionIndexBuilder IncludeProperties(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [NotNull] IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation = false)
        {
            if (indexBuilder.CanSetIncludeProperties(propertyNames, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetIncludeProperties(propertyNames, fromDataAnnotation);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the given include properties can be set.
        /// </summary>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="propertyNames">An array of property names to be used in 'include' clause.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns> <c>true</c> if the given include properties can be set. </returns>
        public static bool CanSetIncludeProperties(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                   .Overrides(indexBuilder.Metadata.GetIncludePropertiesConfigurationSource())
                   || StructuralComparisons.StructuralEqualityComparer.Equals(
                       propertyNames, indexBuilder.Metadata.GetIncludeProperties());
        }

        #endregion Include

        #region Created concurrently

        /// <summary>
        /// When this option is used, PostgreSQL will build the index without taking any locks that prevent concurrent inserts,
        /// updates, or deletes on the table; whereas a standard index build locks out writes (but not reads) on the table until it's done.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-CONCURRENTLY
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="createdConcurrently">A value indicating whether the index is created with the "concurrently" option.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder IsCreatedConcurrently([NotNull] this IndexBuilder indexBuilder, bool createdConcurrently = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.SetIsCreatedConcurrently(createdConcurrently);

            return indexBuilder;
        }

        /// <summary>
        /// When this option is used, PostgreSQL will build the index without taking any locks that prevent concurrent inserts,
        /// updates, or deletes on the table; whereas a standard index build locks out writes (but not reads) on the table until it's done.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-CONCURRENTLY
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="createdConcurrently">A value indicating whether the index is created with the "concurrently" option.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IndexBuilder<TEntity> IsCreatedConcurrently<TEntity>([NotNull] this IndexBuilder<TEntity> indexBuilder, bool createdConcurrently = true)
            => (IndexBuilder<TEntity>)IsCreatedConcurrently((IndexBuilder)indexBuilder, createdConcurrently);

        /// <summary>
        /// When this option is used, PostgreSQL will build the index without taking any locks that prevent concurrent inserts,
        /// updates, or deletes on the table; whereas a standard index build locks out writes (but not reads) on the table until it's done.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-CONCURRENTLY
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="createdConcurrently">A value indicating whether the index is created with the "concurrently" option.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static IConventionIndexBuilder IsCreatedConcurrently(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            bool? createdConcurrently,
            bool fromDataAnnotation = false)
        {
            if (indexBuilder.CanSetIsCreatedConcurrently(createdConcurrently, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetIsCreatedConcurrently(createdConcurrently);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether concurrent creation for the index can be set.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-CONCURRENTLY
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="createdConcurrently">A value indicating whether the index is created with the "concurrently" option.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        public static bool CanSetIsCreatedConcurrently(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            bool? createdConcurrently,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.CreatedConcurrently, createdConcurrently, fromDataAnnotation);
        }

        #endregion Created concurrently

        #region Obsolete

        /// <summary>
        /// The PostgreSQL index collation to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort options to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        [Obsolete("Use UseCollation")]
        public static IndexBuilder HasCollation(
            [NotNull] this IndexBuilder indexBuilder,
            [CanBeNull] params string[] values)
            => UseCollation(indexBuilder, values);

        /// <summary>
        /// The PostgreSQL index collation to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort options to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        [Obsolete("Use UseCollation")]
        public static IndexBuilder<TEntity> HasCollation<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder,
            [CanBeNull] params string[] values)
            => UseCollation(indexBuilder, values);

        /// <summary>
        /// The PostgreSQL index collation to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort options to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        [Obsolete("Use UseCollation")]
        public static IConventionIndexBuilder HasCollation(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<string> values,
            bool fromDataAnnotation)
            => UseCollation(indexBuilder, values, fromDataAnnotation);

        /// <summary>
        /// Returns a value indicating whether the PostgreSQL index collation can be set.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort options to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        [Obsolete("Use CanSetHasCollation")]
        public static bool CanSetHasCollation(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<string> values,
            bool fromDataAnnotation)
            => CanSetCollation(indexBuilder, values, fromDataAnnotation);

        /// <summary>
        /// The PostgreSQL index method to be used. Null selects the default (currently btree).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="method">The name of the index.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the index can be configured with the method</returns>
        [Obsolete("Use CanSetMethod")]
        public static bool CanSetHasMethod(
            [NotNull] this IConventionIndexBuilder indexBuilder, [CanBeNull] string method,
            bool fromDataAnnotation = false)
            => CanSetMethod(indexBuilder, method, fromDataAnnotation);

        /// <summary>
        /// Returns a value indicating whether the PostgreSQL index operators can be set.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="operators">The operators to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the index can be configured with the method.</returns>
        [Obsolete("Use CanSetOperators")]
        public static bool CanSetHasOperators(
            [NotNull] this IConventionIndexBuilder indexBuilder, [CanBeNull] IReadOnlyList<string> operators,
            bool fromDataAnnotation)
            => CanSetOperators(indexBuilder, operators, fromDataAnnotation);

        /// <summary>
        /// Returns a value indicating whether the index can be configured as a full-text tsvector expression index.
        /// </summary>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="config">
        /// <para>
        /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
        /// generated tsvector property.
        /// </para>
        /// <para>
        /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
        /// </para>
        /// </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the index can be configured as a full-text tsvector expression index.</returns>
        [Obsolete("Use CanSetIsTsVectorExpressionIndex")]
        public static bool CanSetToTsVector(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] string config,
            bool fromDataAnnotation = false)
            => CanSetIsTsVectorExpressionIndex(indexBuilder, config, fromDataAnnotation);

        /// <summary>
        /// Returns a value indicating whether the PostgreSQL index sort ordering can be set.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        [Obsolete("Use CanSetSortOrder")]
        public static bool CanSetHasSortOrder(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<SortOrder> values,
            bool fromDataAnnotation)
            => CanSetSortOrder(indexBuilder, values, fromDataAnnotation);

        /// <summary>
        /// Returns a value indicating whether the PostgreSQL index null sort ordering can be set.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the index.</returns>
        [Obsolete("Use CanSetNullSortOrder")]
        public static bool CanSetHasNullSortOrder(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<NullSortOrder> values,
            bool fromDataAnnotation)
            => CanSetNullSortOrder(indexBuilder, values, fromDataAnnotation);

        /// <summary>
        /// Returns a value indicating whether the given include properties can be set.
        /// </summary>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="propertyNames">An array of property names to be used in 'include' clause.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns> <c>true</c> if the given include properties can be set. </returns>
        [Obsolete("Use CanSetIncludeProperties")]
        public static bool CanSetInclude(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation = false)
            => CanSetIncludeProperties(indexBuilder, propertyNames, fromDataAnnotation);

        #endregion Obsolete
    }
}
