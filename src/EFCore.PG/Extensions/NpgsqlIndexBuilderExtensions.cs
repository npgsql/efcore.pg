using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
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

            indexBuilder.Metadata.SetNpgsqlMethod(method);

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
            if (indexBuilder.ForNpgsqlCanSetHasMethod(method, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetNpgsqlMethod(method, fromDataAnnotation);

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
        public static bool ForNpgsqlCanSetHasMethod(
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
            [CanBeNull, ItemNotNull] params string[] operators)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(operators, nameof(operators));

            indexBuilder.Metadata.SetNpgsqlOperators(operators);

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
            [CanBeNull, ItemNotNull] params string[] operators)
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
            [CanBeNull, ItemNotNull] IReadOnlyList<string> operators,
            bool fromDataAnnotation)
        {
            if (indexBuilder.ForNpgsqlCanSetHasOperators(operators, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetNpgsqlOperators(operators, fromDataAnnotation);

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
        public static bool ForNpgsqlCanSetHasOperators(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull, ItemNotNull] IReadOnlyList<string> operators,
            bool fromDataAnnotation)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.IndexOperators, operators, fromDataAnnotation);
        }

        #endregion Operators

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
        public static IndexBuilder HasCollation(
            [NotNull] this IndexBuilder indexBuilder,
            [CanBeNull, ItemNotNull] params string[] values)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(values, nameof(values));

            indexBuilder.Metadata.SetNpgsqlCollation(values);

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
        public static IndexBuilder<TEntity> HasCollation<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder,
            [CanBeNull, ItemNotNull] params string[] values)
            => (IndexBuilder<TEntity>)HasCollation((IndexBuilder)indexBuilder, values);

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
        public static IConventionIndexBuilder HasCollation(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull, ItemNotNull] IReadOnlyList<string> values,
            bool fromDataAnnotation)
        {
            if (indexBuilder.ForNpgsqlCanSetHasCollation(values, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetNpgsqlCollation(values, fromDataAnnotation);

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
        public static bool ForNpgsqlCanSetHasCollation(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull, ItemNotNull] IReadOnlyList<string> values,
            bool fromDataAnnotation)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.IndexCollation, values, fromDataAnnotation);
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
                indexBuilder.Metadata.SetNpgsqlSortOrder(values);

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
            if (indexBuilder.ForNpgsqlCanSetHasSortOrder(values, fromDataAnnotation))
            {
                Check.NotNull(indexBuilder, nameof(indexBuilder));
                Check.NullButNotEmpty(values, nameof(values));

                if (!SortOrderHelper.IsDefaultSortOrder(values))
                    indexBuilder.Metadata.SetNpgsqlSortOrder(values, fromDataAnnotation);

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
        public static bool ForNpgsqlCanSetHasSortOrder(
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

            var sortOrders = indexBuilder.Metadata.GetNpgsqlSortOrder();

            if (!SortOrderHelper.IsDefaultNullSortOrder(values, sortOrders))
                indexBuilder.Metadata.SetNpgsqlNullSortOrder(values);

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
            IReadOnlyList<NullSortOrder> values,
            bool fromDataAnnotation)
        {
            if (indexBuilder.ForNpgsqlCanSetHasNullSortOrder(values, fromDataAnnotation))
            {
                var sortOrders = indexBuilder.Metadata.GetNpgsqlSortOrder();

                if (!SortOrderHelper.IsDefaultNullSortOrder(values, sortOrders))
                    indexBuilder.Metadata.SetNpgsqlNullSortOrder(values, fromDataAnnotation);

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
        public static bool ForNpgsqlCanSetHasNullSortOrder(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            IReadOnlyList<NullSortOrder> values,
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

            indexBuilder.Metadata.SetNpgsqlIncludeProperties(propertyNames);

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
            if (indexBuilder.ForNpgsqlCanSetInclude(propertyNames, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetNpgsqlIncludeProperties(propertyNames, fromDataAnnotation);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given include properties can be set.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="propertyNames"> An array of property names to be used in 'include' clause. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given include properties can be set. </returns>
        public static bool ForNpgsqlCanSetInclude(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                   .Overrides(indexBuilder.Metadata.GetNpgsqlIncludePropertiesConfigurationSource())
                   || StructuralComparisons.StructuralEqualityComparer.Equals(
                       propertyNames, indexBuilder.Metadata.GetNpgsqlIncludeProperties());
        }

        #endregion Include

        #region Obsolete

        /// <summary>
        /// The PostgreSQL index method to be used. Null selects the default (currently btree).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="method">The name of the index.</param>
        /// <returns>A builder to further configure the index.</returns>
        [Obsolete("Use HasMethod")]
        public static IndexBuilder ForNpgsqlHasMethod([NotNull] this IndexBuilder indexBuilder, [CanBeNull] string method)
            => indexBuilder.HasMethod(method);

        /// <summary>
        /// The PostgreSQL index operators to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="operators">The operators to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        [Obsolete("Use HasOperators")]
        public static IndexBuilder ForNpgsqlHasOperators([NotNull] this IndexBuilder indexBuilder, [CanBeNull, ItemNotNull] params string[] operators)
            => indexBuilder.HasOperators(operators);

        /// <summary>
        /// The PostgreSQL index collation to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort options to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        [Obsolete("Use HasCollation")]
        public static IndexBuilder ForNpgsqlHasCollation([NotNull] this IndexBuilder indexBuilder, [CanBeNull, ItemNotNull] params string[] values)
            => indexBuilder.HasCollation(values);

        /// <summary>
        /// The PostgreSQL index sort ordering to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        [Obsolete("Use HasSortOrder")]
        public static IndexBuilder ForNpgsqlHasSortOrder([NotNull] this IndexBuilder indexBuilder, [CanBeNull] params SortOrder[] values)
            => indexBuilder.HasSortOrder(values);

        /// <summary>
        /// The PostgreSQL index NULL sort ordering to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder">The builder for the index being configured.</param>
        /// <param name="values">The sort order to use for each column.</param>
        /// <returns>A builder to further configure the index.</returns>
        [Obsolete("Use HasNullSortOrder")]
        public static IndexBuilder ForNpgsqlHasNullSortOrder([NotNull] this IndexBuilder indexBuilder, [CanBeNull] params NullSortOrder[] values)
            => indexBuilder.HasNullSortOrder(values);

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
        [Obsolete("Use IncludeProperties")]
        public static IndexBuilder ForNpgsqlInclude([NotNull] this IndexBuilder indexBuilder, [NotNull, ItemNotNull] params string[] propertyNames)
            => indexBuilder.IncludeProperties(propertyNames);

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
        [Obsolete("Use IncludeProperties")]
        public static IndexBuilder<TEntity> ForNpgsqlInclude<TEntity>([NotNull] this IndexBuilder<TEntity> indexBuilder, [NotNull] Expression<Func<TEntity, object>> includeExpression)
            => indexBuilder.IncludeProperties(includeExpression);

        #endregion Obsolete
    }
}
