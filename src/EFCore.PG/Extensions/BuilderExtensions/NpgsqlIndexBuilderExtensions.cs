using System.Collections;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Npgsql specific extension methods for <see cref="IndexBuilder" />.
/// </summary>
public static class NpgsqlIndexBuilderExtensions
{
    #region Method

    /// <summary>
    ///     The PostgreSQL index method to be used. Null selects the default (currently btree).
    /// </summary>
    /// <remarks>
    ///     http://www.postgresql.org/docs/current/static/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="method">The name of the index.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder HasMethod(
        this IndexBuilder indexBuilder,
        string? method)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));
        Check.NullButNotEmpty(method, nameof(method));

        indexBuilder.Metadata.SetMethod(method);

        return indexBuilder;
    }

    /// <summary>
    ///     The PostgreSQL index method to be used. Null selects the default (currently btree).
    /// </summary>
    /// <remarks>
    ///     http://www.postgresql.org/docs/current/static/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="method">The name of the index.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> HasMethod<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        string? method)
        => (IndexBuilder<TEntity>)HasMethod((IndexBuilder)indexBuilder, method);

    /// <summary>
    ///     The PostgreSQL index method to be used. Null selects the default (currently btree).
    /// </summary>
    /// <remarks>
    ///     http://www.postgresql.org/docs/current/static/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="method">The name of the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IConventionIndexBuilder? HasMethod(
        this IConventionIndexBuilder indexBuilder,
        string? method,
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
    ///     The PostgreSQL index method to be used. Null selects the default (currently btree).
    /// </summary>
    /// <remarks>
    ///     http://www.postgresql.org/docs/current/static/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="method">The name of the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><c>true</c> if the index can be configured with the method</returns>
    public static bool CanSetMethod(
        this IConventionIndexBuilder indexBuilder,
        string? method,
        bool fromDataAnnotation = false)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));

        return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.IndexMethod, method, fromDataAnnotation);
    }

    #endregion Method

    #region Operators

    /// <summary>
    ///     The PostgreSQL index operators to be used.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-opclass.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="operators">The operators to use for each column.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder HasOperators(
        this IndexBuilder indexBuilder,
        params string[]? operators)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));
        Check.NullButNotEmpty(operators, nameof(operators));

        indexBuilder.Metadata.SetOperators(operators);

        return indexBuilder;
    }

    /// <summary>
    ///     The PostgreSQL index operators to be used.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-opclass.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="operators">The operators to use for each column.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> HasOperators<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        params string[]? operators)
        => (IndexBuilder<TEntity>)HasOperators((IndexBuilder)indexBuilder, operators);

    /// <summary>
    ///     The PostgreSQL index operators to be used.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-opclass.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="operators">The operators to use for each column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IConventionIndexBuilder? HasOperators(
        this IConventionIndexBuilder indexBuilder,
        IReadOnlyList<string>? operators,
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
    ///     Returns a value indicating whether the PostgreSQL index operators can be set.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-opclass.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="operators">The operators to use for each column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><c>true</c> if the index can be configured with the method.</returns>
    public static bool CanSetOperators(
        this IConventionIndexBuilder indexBuilder,
        IReadOnlyList<string>? operators,
        bool fromDataAnnotation)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));

        return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.IndexOperators, operators, fromDataAnnotation);
    }

    #endregion Operators

    #region IsTsVectorExpressionIndex

    /// <summary>
    ///     Configures this index to be a full-text tsvector expression index.
    /// </summary>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="config">
    ///     <para>
    ///         The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    ///         generated tsvector property.
    ///     </para>
    ///     <para>
    ///         See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
    ///     </para>
    /// </param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder IsTsVectorExpressionIndex(
        this IndexBuilder indexBuilder,
        string config)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));
        Check.NotNull(config, nameof(config));

        indexBuilder.Metadata.SetTsVectorConfig(config);
        return indexBuilder;
    }

    /// <summary>
    ///     Configures this index to be a full-text tsvector expression index.
    /// </summary>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="config">
    ///     <para>
    ///         The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    ///         generated tsvector property.
    ///     </para>
    ///     <para>
    ///         See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
    ///     </para>
    /// </param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> IsTsVectorExpressionIndex<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        string config)
        => (IndexBuilder<TEntity>)IsTsVectorExpressionIndex((IndexBuilder)indexBuilder, config);

    /// <summary>
    ///     Configures this index to be a full-text tsvector expression index.
    /// </summary>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="config">
    ///     <para>
    ///         The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    ///         generated tsvector property.
    ///     </para>
    ///     <para>
    ///         See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
    ///     </para>
    /// </param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <c>null</c> otherwise.
    /// </returns>
    public static IConventionIndexBuilder? IsTsVectorExpressionIndex(
        this IConventionIndexBuilder indexBuilder,
        string? config)
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
    ///     Returns a value indicating whether the index can be configured as a full-text tsvector expression index.
    /// </summary>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="config">
    ///     <para>
    ///         The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    ///         generated tsvector property.
    ///     </para>
    ///     <para>
    ///         See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
    ///     </para>
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><c>true</c> if the index can be configured as a full-text tsvector expression index.</returns>
    public static bool CanSetIsTsVectorExpressionIndex(
        this IConventionIndexBuilder indexBuilder,
        string? config,
        bool fromDataAnnotation = false)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));

        return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.TsVectorConfig, config, fromDataAnnotation);
    }

    #endregion IsTsVectorExpressionIndex

    #region Collation

    /// <summary>
    ///     The PostgreSQL index collation to be used.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-collations.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="values">The sort options to use for each column.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder UseCollation(
        this IndexBuilder indexBuilder,
        params string[]? values)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));
        Check.NullButNotEmpty(values, nameof(values));

        indexBuilder.Metadata.SetCollation(values);

        return indexBuilder;
    }

    /// <summary>
    ///     The PostgreSQL index collation to be used.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-collations.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="values">The sort options to use for each column.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> UseCollation<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        params string[]? values)
        => (IndexBuilder<TEntity>)UseCollation((IndexBuilder)indexBuilder, values);

    /// <summary>
    ///     The PostgreSQL index collation to be used.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-collations.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="values">The sort options to use for each column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IConventionIndexBuilder? UseCollation(
        this IConventionIndexBuilder indexBuilder,
        IReadOnlyList<string>? values,
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
    ///     Returns a value indicating whether the PostgreSQL index collation can be set.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-collations.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="values">The sort options to use for each column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static bool CanSetCollation(
        this IConventionIndexBuilder indexBuilder,
        IReadOnlyList<string>? values,
        bool fromDataAnnotation)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));

        return indexBuilder.CanSetAnnotation(RelationalAnnotationNames.Collation, values, fromDataAnnotation);
    }

    #endregion Collation

    #region Null sort order

    /// <summary>
    ///     The PostgreSQL index NULL sort ordering to be used.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="values">The sort order to use for each column.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder HasNullSortOrder(
        this IndexBuilder indexBuilder,
        params NullSortOrder[]? values)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));
        Check.NullButNotEmpty(values, nameof(values));

        if (!SortOrderHelper.IsDefaultNullSortOrder(values, indexBuilder.Metadata.IsDescending))
        {
            indexBuilder.Metadata.SetNullSortOrder(values);
        }

        return indexBuilder;
    }

    /// <summary>
    ///     The PostgreSQL index NULL sort ordering to be used.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="values">The sort order to use for each column.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> HasNullSortOrder<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        params NullSortOrder[]? values)
        => (IndexBuilder<TEntity>)HasNullSortOrder((IndexBuilder)indexBuilder, values);

    /// <summary>
    ///     The PostgreSQL index NULL sort ordering to be used.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="values">The sort order to use for each column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IConventionIndexBuilder? HasNullSortOrder(
        this IConventionIndexBuilder indexBuilder,
        IReadOnlyList<NullSortOrder>? values,
        bool fromDataAnnotation)
    {
        if (indexBuilder.CanSetNullSortOrder(values, fromDataAnnotation))
        {
            if (!SortOrderHelper.IsDefaultNullSortOrder(values, indexBuilder.Metadata.IsDescending))
            {
                indexBuilder.Metadata.SetNullSortOrder(values, fromDataAnnotation);
            }

            return indexBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the PostgreSQL index null sort ordering can be set.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="values">The sort order to use for each column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static bool CanSetNullSortOrder(
        this IConventionIndexBuilder indexBuilder,
        IReadOnlyList<NullSortOrder>? values,
        bool fromDataAnnotation)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));

        return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.IndexNullSortOrder, values, fromDataAnnotation);
    }

    #endregion Null sort order

    #region Include

    /// <summary>
    ///     Adds an INCLUDE clause to the index definition with the specified property names.
    ///     This clause specifies a list of columns which will be included as a non-key part in the index.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="propertyNames">An array of property names to be used in INCLUDE clause.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder IncludeProperties(
        this IndexBuilder indexBuilder,
        params string[] propertyNames)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));
        Check.NullButNotEmpty(propertyNames, nameof(propertyNames));

        indexBuilder.Metadata.SetIncludeProperties(propertyNames);

        return indexBuilder;
    }

    /// <summary>
    ///     Adds an INCLUDE clause to the index definition with the specified property names.
    ///     This clause specifies a list of columns which will be included as a non-key part in the index.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="propertyNames">An array of property names to be used in INCLUDE clause.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> IncludeProperties<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        params string[] propertyNames)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));
        Check.NullButNotEmpty(propertyNames, nameof(propertyNames));

        indexBuilder.Metadata.SetIncludeProperties(propertyNames);

        return indexBuilder;
    }

    /// <summary>
    ///     Adds an INCLUDE clause to the index definition with property names from the specified expression.
    ///     This clause specifies a list of columns which will be included as a non-key part in the index.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
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
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> IncludeProperties<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        Expression<Func<TEntity, object>> includeExpression)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));
        Check.NotNull(includeExpression, nameof(includeExpression));

        indexBuilder.IncludeProperties(includeExpression.GetPropertyAccessList().Select(x => x.Name).ToArray());

        return indexBuilder;
    }

    /// <summary>
    ///     Adds an INCLUDE clause to the index definition with the specified property names.
    ///     This clause specifies a list of columns which will be included as a non-key part in the index.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="propertyNames">An array of property names to be used in INCLUDE clause.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IConventionIndexBuilder? IncludeProperties(
        this IConventionIndexBuilder indexBuilder,
        IReadOnlyList<string> propertyNames,
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
    ///     Returns a value indicating whether the given include properties can be set.
    /// </summary>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="propertyNames">An array of property names to be used in 'include' clause.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns> <c>true</c> if the given include properties can be set. </returns>
    public static bool CanSetIncludeProperties(
        this IConventionIndexBuilder indexBuilder,
        IReadOnlyList<string>? propertyNames,
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
    ///     When this option is used, PostgreSQL will build the index without taking any locks that prevent concurrent inserts,
    ///     updates, or deletes on the table; whereas a standard index build locks out writes (but not reads) on the table until it's done.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-CONCURRENTLY
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="createdConcurrently">A value indicating whether the index is created with the "concurrently" option.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder IsCreatedConcurrently(this IndexBuilder indexBuilder, bool createdConcurrently = true)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));

        indexBuilder.Metadata.SetIsCreatedConcurrently(createdConcurrently);

        return indexBuilder;
    }

    /// <summary>
    ///     When this option is used, PostgreSQL will build the index without taking any locks that prevent concurrent inserts,
    ///     updates, or deletes on the table; whereas a standard index build locks out writes (but not reads) on the table until it's done.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-CONCURRENTLY
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="createdConcurrently">A value indicating whether the index is created with the "concurrently" option.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> IsCreatedConcurrently<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        bool createdConcurrently = true)
        => (IndexBuilder<TEntity>)IsCreatedConcurrently((IndexBuilder)indexBuilder, createdConcurrently);

    /// <summary>
    ///     When this option is used, PostgreSQL will build the index without taking any locks that prevent concurrent inserts,
    ///     updates, or deletes on the table; whereas a standard index build locks out writes (but not reads) on the table until it's done.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-CONCURRENTLY
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="createdConcurrently">A value indicating whether the index is created with the "concurrently" option.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IConventionIndexBuilder? IsCreatedConcurrently(
        this IConventionIndexBuilder indexBuilder,
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
    ///     Returns a value indicating whether concurrent creation for the index can be set.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-CONCURRENTLY
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="createdConcurrently">A value indicating whether the index is created with the "concurrently" option.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static bool CanSetIsCreatedConcurrently(
        this IConventionIndexBuilder indexBuilder,
        bool? createdConcurrently,
        bool fromDataAnnotation = false)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));

        return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.CreatedConcurrently, createdConcurrently, fromDataAnnotation);
    }

    #endregion Created concurrently

    #region NULLS distinct

    /// <summary>
    ///     Specifies whether for a unique index, null values should be considered distinct (not equal).
    ///     The default is that they are distinct, so that a unique index could contain multiple null values in a column.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="nullsDistinct">Whether nulls should be considered distinct.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder AreNullsDistinct(
        this IndexBuilder indexBuilder,
        bool nullsDistinct = true)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));

        indexBuilder.Metadata.SetAreNullsDistinct(nullsDistinct);

        return indexBuilder;
    }

    /// <summary>
    ///     Specifies whether for a unique index, null values should be considered distinct (not equal).
    ///     The default is that they are distinct, so that a unique index could contain multiple null values in a column.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="nullsDistinct">Whether nulls should be considered distinct.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> AreNullsDistinct<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        bool nullsDistinct = true)
        => (IndexBuilder<TEntity>)AreNullsDistinct((IndexBuilder)indexBuilder, nullsDistinct);

    /// <summary>
    ///     Specifies whether for a unique index, null values should be considered distinct (not equal).
    ///     The default is that they are distinct, so that a unique index could contain multiple null values in a column.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="nullsDistinct">Whether nulls should be considered distinct.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IConventionIndexBuilder? AreNullsDistinct(
        this IConventionIndexBuilder indexBuilder,
        bool nullsDistinct = true,
        bool fromDataAnnotation = false)
    {
        if (indexBuilder.CanSetAreNullsDistinct(nullsDistinct, fromDataAnnotation))
        {
            indexBuilder.Metadata.SetAreNullsDistinct(nullsDistinct, fromDataAnnotation);

            return indexBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Specifies whether for a unique index, null values should be considered distinct (not equal).
    ///     The default is that they are distinct, so that a unique index could contain multiple null values in a column.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/sql-createindex.html
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="nullsDistinct">Whether nulls should be considered distinct.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><c>true</c> if the index can be configured with the method</returns>
    public static bool CanSetAreNullsDistinct(
        this IConventionIndexBuilder indexBuilder,
        bool nullsDistinct = true,
        bool fromDataAnnotation = false)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));

        return indexBuilder.CanSetAnnotation(NpgsqlAnnotationNames.NullsDistinct, nullsDistinct, fromDataAnnotation);
    }

    #endregion NULLS distinct

    #region Storage parameters

    /// <summary>
    ///     Sets a PostgreSQL storage parameter on the index.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-STORAGE-PARAMETERS
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="parameterName">The name of the storage parameter.</param>
    /// <param name="parameterValue">The value of the storage parameter.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static IndexBuilder HasStorageParameter(
        this IndexBuilder indexBuilder,
        string parameterName,
        object? parameterValue)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));

        indexBuilder.Metadata.SetStorageParameter(parameterName, parameterValue);

        return indexBuilder;
    }

    /// <summary>
    ///     Sets a PostgreSQL storage parameter on the index.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-STORAGE-PARAMETERS
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="parameterName">The name of the storage parameter.</param>
    /// <param name="parameterValue">The value of the storage parameter.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static IndexBuilder<TEntity> HasStorageParameter<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        string parameterName,
        object? parameterValue)
        where TEntity : class
        => (IndexBuilder<TEntity>)HasStorageParameter((IndexBuilder)indexBuilder, parameterName, parameterValue);

    /// <summary>
    ///     Sets a PostgreSQL storage parameter on the index.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-STORAGE-PARAMETERS
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="parameterName">The name of the storage parameter.</param>
    /// <param name="parameterValue">The value of the storage parameter.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><c>true</c> if the index can be configured with the method</returns>
    public static IConventionIndexBuilder? HasStorageParameter(
        this IConventionIndexBuilder indexBuilder,
        string parameterName,
        object? parameterValue,
        bool fromDataAnnotation = false)
    {
        if (indexBuilder.CanSetStorageParameter(parameterName, parameterValue, fromDataAnnotation))
        {
            indexBuilder.Metadata.SetStorageParameter(parameterName, parameterValue);

            return indexBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the PostgreSQL storage parameter is set on the table created for this entity.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/static/sql-createtable.html#SQL-CREATETABLE-STORAGE-PARAMETERS
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="parameterName">The name of the storage parameter.</param>
    /// <param name="parameterValue">The value of the storage parameter.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><c>true</c> if the index can be configured as with the storage parameter.</returns>
    public static bool CanSetStorageParameter(
        this IConventionIndexBuilder indexBuilder,
        string parameterName,
        object? parameterValue,
        bool fromDataAnnotation = false)
    {
        Check.NotNull(indexBuilder, nameof(indexBuilder));

        return indexBuilder.CanSetAnnotation(
            NpgsqlAnnotationNames.StorageParameterPrefix + parameterName, parameterValue, fromDataAnnotation);
    }

    #endregion Storage parameters
}
