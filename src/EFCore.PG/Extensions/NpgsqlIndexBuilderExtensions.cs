using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Npgsql specific extension methods for <see cref="IndexBuilder" />.
    /// </summary>
    public static class NpgsqlIndexBuilderExtensions
    {
        /// <summary>
        /// The PostgreSQL index method to be used. Null selects the default (currently btree).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="method"> The name of the index. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForNpgsqlHasMethod(
            [NotNull] this IndexBuilder indexBuilder,
            [CanBeNull] string method)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(method, nameof(method));

            indexBuilder.Metadata.SetNpgsqlMethod(method);

            return indexBuilder;
        }

        /// <summary>
        /// The PostgreSQL index operators to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="operators"> The operators to use for each column. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForNpgsqlHasOperators(
            [NotNull] this IndexBuilder indexBuilder,
            [CanBeNull, ItemNotNull] params string[] operators)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(operators, nameof(operators));

            indexBuilder.Metadata.SetNpgsqlOperators(operators);

            return indexBuilder;
        }

        /// <summary>
        /// The PostgreSQL index collation to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="values"> The sort options to use for each column. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForNpgsqlHasCollation(
            [NotNull] this IndexBuilder indexBuilder,
            [CanBeNull, ItemNotNull] params string[] values)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(values, nameof(values));

            indexBuilder.Metadata.SetNpgsqlCollation(values);

            return indexBuilder;
        }

        /// <summary>
        /// The PostgreSQL index sort ordering to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="values"> The sort order to use for each column. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForNpgsqlHasSortOrder(
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
        /// The PostgreSQL index NULL sort ordering to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="values"> The sort order to use for each column. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForNpgsqlHasNullSortOrder(
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
        /// Adds an INCLUDE clause to the index definition with the specified property names.
        /// This clause specifies a list of columns which will be included as a non-key part in the index.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="propertyNames"> An array of property names to be used in INCLUDE clause. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForNpgsqlInclude(
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
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="includeExpression">
        ///     <para>
        ///         A lambda expression representing the property(s) to be included in the INCLUDE clause
        ///         (<c>blog => blog.Url</c>).
        ///     </para>
        ///     <para>
        ///         If multiple properties are to be included then specify an anonymous type including the
        ///         properties (<c>post => new { post.Title, post.BlogId }</c>).
        ///     </para>
        /// </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder<TEntity> ForNpgsqlInclude<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder,
            [NotNull] Expression<Func<TEntity, object>> includeExpression)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NotNull(includeExpression, nameof(includeExpression));

            indexBuilder.ForNpgsqlInclude(includeExpression.GetPropertyAccessList().Select(x => x.Name).ToArray());

            return indexBuilder;
        }
    }
}
