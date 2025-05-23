using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="IIndex" /> for Npgsql-specific metadata.
/// </summary>
public static class NpgsqlIndexExtensions
{
    #region Method

    /// <summary>
    ///     Returns the index method to be used, or <c>null</c> if it hasn't been specified.
    ///     <c>null</c> selects the default (currently <c>btree</c>).
    /// </summary>
    /// <remarks>
    ///     http://www.postgresql.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static string? GetMethod(this IReadOnlyIndex index)
        => (string?)index[NpgsqlAnnotationNames.IndexMethod];

    /// <summary>
    ///     Sets the index method to be used, or <c>null</c> if it hasn't been specified.
    ///     <c>null</c> selects the default (currently <c>btree</c>).
    /// </summary>
    /// <remarks>
    ///     http://www.postgresql.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static void SetMethod(this IMutableIndex index, string? method)
        => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexMethod, method);

    /// <summary>
    ///     Sets the index method to be used, or <c>null</c> if it hasn't been specified.
    ///     <c>null</c> selects the default (currently <c>btree</c>).
    /// </summary>
    /// <remarks>
    ///     http://www.postgresql.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static string? SetMethod(
        this IConventionIndex index,
        string? method,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(method, nameof(method));

        index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexMethod, method, fromDataAnnotation);

        return method;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the index method.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the index method.</returns>
    public static ConfigurationSource? GetMethodConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(NpgsqlAnnotationNames.IndexMethod)?.GetConfigurationSource();

    #endregion Method

    #region Operators

    /// <summary>
    ///     Returns the column operators to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-opclass.html
    /// </remarks>
    public static IReadOnlyList<string>? GetOperators(this IReadOnlyIndex index)
        => (IReadOnlyList<string>?)index[NpgsqlAnnotationNames.IndexOperators];

    /// <summary>
    ///     Sets the column operators to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-opclass.html
    /// </remarks>
    public static void SetOperators(this IMutableIndex index, IReadOnlyList<string>? operators)
        => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexOperators, operators);

    /// <summary>
    ///     Sets the column operators to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-opclass.html
    /// </remarks>
    public static IReadOnlyList<string>? SetOperators(
        this IConventionIndex index,
        IReadOnlyList<string>? operators,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(operators, nameof(operators));

        index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexOperators, operators, fromDataAnnotation);

        return operators;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the index operators.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the index operators.</returns>
    public static ConfigurationSource? GetOperatorsConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(NpgsqlAnnotationNames.IndexOperators)?.GetConfigurationSource();

    #endregion Operators

    #region Collation

    /// <summary>
    ///     Returns the column collations to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-collations.html
    /// </remarks>
#pragma warning disable 618
    public static IReadOnlyList<string>? GetCollation(this IReadOnlyIndex index)
        => (IReadOnlyList<string>?)(
            index[RelationalAnnotationNames.Collation] ?? index[NpgsqlAnnotationNames.IndexCollation]);
#pragma warning restore 618

    /// <summary>
    ///     Sets the column collations to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-collations.html
    /// </remarks>
    public static void SetCollation(this IMutableIndex index, IReadOnlyList<string>? collations)
        => index.SetOrRemoveAnnotation(RelationalAnnotationNames.Collation, collations);

    /// <summary>
    ///     Sets the column collations to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-collations.html
    /// </remarks>
    public static IReadOnlyList<string>? SetCollation(
        this IConventionIndex index,
        IReadOnlyList<string>? collations,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(collations, nameof(collations));

        index.SetOrRemoveAnnotation(RelationalAnnotationNames.Collation, collations, fromDataAnnotation);

        return collations;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the index collations.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the index collations.</returns>
    public static ConfigurationSource? GetCollationConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(RelationalAnnotationNames.Collation)?.GetConfigurationSource();

    #endregion Collation

    #region Null sort order

    /// <summary>
    ///     Returns the column NULL sort orders to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    public static IReadOnlyList<NullSortOrder>? GetNullSortOrder(this IReadOnlyIndex index)
        => (IReadOnlyList<NullSortOrder>?)index[NpgsqlAnnotationNames.IndexNullSortOrder];

    /// <summary>
    ///     Sets the column NULL sort orders to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    public static void SetNullSortOrder(this IMutableIndex index, IReadOnlyList<NullSortOrder>? nullSortOrder)
        => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexNullSortOrder, nullSortOrder);

    /// <summary>
    ///     Sets the column NULL sort orders to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    public static IReadOnlyList<NullSortOrder>? SetNullSortOrder(
        this IConventionIndex index,
        IReadOnlyList<NullSortOrder>? nullSortOrder,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(nullSortOrder, nameof(nullSortOrder));

        index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexNullSortOrder, nullSortOrder, fromDataAnnotation);

        return nullSortOrder;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the index null sort orders.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the index null sort orders.</returns>
    public static ConfigurationSource? GetNullSortOrderConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(NpgsqlAnnotationNames.IndexNullSortOrder)?.GetConfigurationSource();

    #endregion

    #region Included properties

    /// <summary>
    ///     Returns included property names, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The included property names, or <c>null</c> if they have not been specified.</returns>
    public static IReadOnlyList<string>? GetIncludeProperties(this IReadOnlyIndex index)
        => (IReadOnlyList<string>?)index[NpgsqlAnnotationNames.IndexInclude];

    /// <summary>
    ///     Sets included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="properties">The value to set.</param>
    public static void SetIncludeProperties(this IMutableIndex index, IReadOnlyList<string>? properties)
        => index.SetOrRemoveAnnotation(
            NpgsqlAnnotationNames.IndexInclude,
            properties);

    /// <summary>
    ///     Sets included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <param name="properties">The value to set.</param>
    public static IReadOnlyList<string>? SetIncludeProperties(
        this IConventionIndex index,
        IReadOnlyList<string>? properties,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(properties, nameof(properties));

        index.SetOrRemoveAnnotation(
            NpgsqlAnnotationNames.IndexInclude,
            properties,
            fromDataAnnotation);

        return properties;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the included property names.</returns>
    public static ConfigurationSource? GetIncludePropertiesConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(NpgsqlAnnotationNames.IndexInclude)?.GetConfigurationSource();

    #endregion Included properties

    #region Created concurrently

    /// <summary>
    ///     Returns a value indicating whether the index is created concurrently.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns><c>true</c> if the index is created concurrently.</returns>
    public static bool? IsCreatedConcurrently(this IReadOnlyIndex index)
        => (bool?)index[NpgsqlAnnotationNames.CreatedConcurrently];

    /// <summary>
    ///     Sets a value indicating whether the index is created concurrently.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="createdConcurrently">The value to set.</param>
    public static void SetIsCreatedConcurrently(this IMutableIndex index, bool? createdConcurrently)
        => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.CreatedConcurrently, createdConcurrently);

    /// <summary>
    ///     Sets a value indicating whether the index is created concurrently.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="createdConcurrently">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static bool? SetIsCreatedConcurrently(
        this IConventionIndex index,
        bool? createdConcurrently,
        bool fromDataAnnotation = false)
    {
        index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.CreatedConcurrently, createdConcurrently, fromDataAnnotation);

        return createdConcurrently;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for whether the index is created concurrently.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for whether the index is created concurrently.</returns>
    public static ConfigurationSource? GetIsCreatedConcurrentlyConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(NpgsqlAnnotationNames.CreatedConcurrently)?.GetConfigurationSource();

    #endregion Created concurrently

    #region NULLS distinct

    /// <summary>
    ///     Returns whether for a unique index, null values should be considered distinct (not equal).
    ///     The default is that they are distinct, so that a unique index could contain multiple null values in a column.
    /// </summary>
    /// <remarks>
    ///     http://www.postgresql.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static bool? GetAreNullsDistinct(this IReadOnlyIndex index)
        => (bool?)index[NpgsqlAnnotationNames.NullsDistinct];

    /// <summary>
    ///     Sets whether for a unique index, null values should be considered distinct (not equal).
    ///     The default is that they are distinct, so that a unique index could contain multiple null values in a column.
    /// </summary>
    /// <remarks>
    ///     http://www.postgresql.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static void SetAreNullsDistinct(this IMutableIndex index, bool? nullsDistinct)
        => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.NullsDistinct, nullsDistinct);

    /// <summary>
    ///     Sets whether for a unique index, null values should be considered distinct (not equal).
    ///     The default is that they are distinct, so that a unique index could contain multiple null values in a column.
    /// </summary>
    /// <remarks>
    ///     http://www.postgresql.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static bool? SetAreNullsDistinct(this IConventionIndex index, bool? nullsDistinct, bool fromDataAnnotation = false)
    {
        index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.NullsDistinct, nullsDistinct, fromDataAnnotation);

        return nullsDistinct;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for whether nulls are considered distinct.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" />.</returns>
    public static ConfigurationSource? GetAreNullsDistinctConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(NpgsqlAnnotationNames.NullsDistinct)?.GetConfigurationSource();

    #endregion NULLS distinct

    #region ToTsVector

    /// <summary>
    ///     Returns the text search configuration for this tsvector expression index, or <c>null</c> if this is not a
    ///     tsvector expression index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/textsearch-tables.html#TEXTSEARCH-TABLES-INDEX
    /// </remarks>
    public static string? GetTsVectorConfig(this IReadOnlyIndex index)
        => (string?)index[NpgsqlAnnotationNames.TsVectorConfig];

    /// <summary>
    ///     Sets the text search configuration for this tsvector expression index, or <c>null</c> if this is not a
    ///     tsvector expression index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="config">
    ///     <para>
    ///         The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    ///         generated tsvector property.
    ///     </para>
    ///     <para>
    ///         See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
    ///     </para>
    /// </param>
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/textsearch-tables.html#TEXTSEARCH-TABLES-INDEX
    /// </remarks>
    public static void SetTsVectorConfig(this IMutableIndex index, string? config)
        => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.TsVectorConfig, config);

    /// <summary>
    ///     Sets the index to tsvector config name to be used.
    /// </summary>
    /// <param name="index">The index.</param>
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
    /// <remarks>
    ///     https://www.postgresql.org/docs/current/textsearch-tables.html#TEXTSEARCH-TABLES-INDEX
    /// </remarks>
    public static string? SetTsVectorConfig(
        this IConventionIndex index,
        string? config,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(config, nameof(config));

        index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.TsVectorConfig, config, fromDataAnnotation);

        return config;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the tsvector config.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the tsvector config.</returns>
    public static ConfigurationSource? GetTsVectorConfigConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(NpgsqlAnnotationNames.TsVectorConfig)?.GetConfigurationSource();

    #endregion ToTsVector

    #region Storage parameters

    /// <summary>
    ///     Gets all storage parameters for the index.
    /// </summary>
    public static Dictionary<string, object?> GetStorageParameters(this IReadOnlyIndex index)
        => index.GetAnnotations()
            .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal))
            .ToDictionary(
                a => a.Name.Substring(NpgsqlAnnotationNames.StorageParameterPrefix.Length),
                a => a.Value);

    /// <summary>
    ///     Gets a storage parameter for the index.
    /// </summary>
    public static string? GetStorageParameter(this IIndex index, string parameterName)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        return (string?)index[NpgsqlAnnotationNames.StorageParameterPrefix + parameterName];
    }

    /// <summary>
    ///     Sets a storage parameter on the index.
    /// </summary>
    public static void SetStorageParameter(this IMutableIndex index, string parameterName, object? parameterValue)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.StorageParameterPrefix + parameterName, parameterValue);
    }

    /// <summary>
    ///     Sets a storage parameter on the index.
    /// </summary>
    public static object SetStorageParameter(
        this IConventionIndex index,
        string parameterName,
        object? parameterValue,
        bool fromDataAnnotation = false)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.StorageParameterPrefix + parameterName, parameterValue, fromDataAnnotation);

        return parameterName;
    }

    /// <summary>
    ///     Gets the configuration source for a storage parameter for the table mapped to the entity type.
    /// </summary>
    public static ConfigurationSource? GetStorageParameterConfigurationSource(this IConventionIndex index, string parameterName)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        return index.FindAnnotation(NpgsqlAnnotationNames.StorageParameterPrefix + parameterName)?.GetConfigurationSource();
    }

    #endregion Storage parameters
}
