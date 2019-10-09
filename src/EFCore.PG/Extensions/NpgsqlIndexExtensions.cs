using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IIndex" /> for Npgsql-specific metadata.
    /// </summary>
    public static class NpgsqlIndexExtensions
    {
        /// <summary>
        /// Returns the index method to be used, or <c>null</c> if it hasn't been specified.
        /// <c>null</c> selects the default (currently <c>btree</c>).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        public static string GetMethod([NotNull] this IIndex index)
            => (string)index[NpgsqlAnnotationNames.IndexMethod];

        /// <summary>
        /// Sets the index method to be used, or <c>null</c> if it hasn't been specified.
        /// <c>null</c> selects the default (currently <c>btree</c>).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        public static void SetMethod([NotNull] this IMutableIndex index, [CanBeNull] string method)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexMethod, method);

        /// <summary>
        /// Sets the index method to be used, or <c>null</c> if it hasn't been specified.
        /// <c>null</c> selects the default (currently <c>btree</c>).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        public static void SetMethod([NotNull] this IConventionIndex index, [CanBeNull] string method, bool fromDataAnnotation = false)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexMethod, method, fromDataAnnotation);

        /// <summary>
        /// Returns the column operators to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        public static IReadOnlyList<string> GetOperators([NotNull] this IIndex index)
            => (IReadOnlyList<string>)index[NpgsqlAnnotationNames.IndexOperators];

        /// <summary>
        /// Sets the column operators to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        public static void SetOperators([NotNull] this IMutableIndex index, [CanBeNull] IReadOnlyList<string> operators)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexOperators, operators);

        /// <summary>
        /// Sets the column operators to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        public static void SetOperators([NotNull] this IConventionIndex index, [CanBeNull] IReadOnlyList<string> operators, bool fromDataAnnotation)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexOperators, operators, fromDataAnnotation);

        /// <summary>
        /// Returns the column collations to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        public static IReadOnlyList<string> GetCollation([NotNull] this IIndex index)
            => (IReadOnlyList<string>)index[NpgsqlAnnotationNames.IndexCollation];

        /// <summary>
        /// Sets the column collations to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        public static void SetCollation([NotNull] this IMutableIndex index, [CanBeNull] IReadOnlyList<string> collations)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexCollation, collations);

        /// <summary>
        /// Sets the column collations to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        public static void SetCollation([NotNull] this IConventionIndex index, [CanBeNull] IReadOnlyList<string> collations, bool fromDataAnnotation)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexCollation, collations, fromDataAnnotation);

        /// <summary>
        /// Returns the column sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public static IReadOnlyList<SortOrder> GetSortOrder([NotNull] this IIndex index)
            => (IReadOnlyList<SortOrder>)index[NpgsqlAnnotationNames.IndexSortOrder];

        /// <summary>
        /// Sets the column sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public static void SetSortOrder([NotNull] this IMutableIndex index, [CanBeNull] IReadOnlyList<SortOrder> sortOrder)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexSortOrder, sortOrder);

        /// <summary>
        /// Sets the column sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public static void SetSortOrder([NotNull] this IConventionIndex index, [CanBeNull] IReadOnlyList<SortOrder> sortOrder, bool fromDataAnnotation)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexSortOrder, sortOrder, fromDataAnnotation);

        /// <summary>
        /// Returns the column NULL sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public static IReadOnlyList<NullSortOrder> GetNullSortOrder([NotNull] this IIndex index)
            => (IReadOnlyList<NullSortOrder>)index[NpgsqlAnnotationNames.IndexNullSortOrder];

        /// <summary>
        /// Sets the column NULL sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public static void SetNullSortOrder([NotNull] this IMutableIndex index, [CanBeNull] IReadOnlyList<NullSortOrder> nullSortOrder)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexNullSortOrder, nullSortOrder);

        /// <summary>
        /// Sets the column NULL sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public static void SetNullSortOrder([NotNull] this IConventionIndex index,
            [CanBeNull] IReadOnlyList<NullSortOrder> nullSortOrder, bool fromDataAnnotation)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexNullSortOrder, nullSortOrder, fromDataAnnotation);

        #region Included properties

        /// <summary>
        /// Returns included property names, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The included property names, or <c>null</c> if they have not been specified.</returns>
        public static IReadOnlyList<string> GetIncludeProperties([NotNull] this IIndex index)
            => (IReadOnlyList<string>)index[NpgsqlAnnotationNames.IndexInclude];

        /// <summary>
        /// Sets included property names.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="properties">The value to set.</param>
        public static void SetIncludeProperties([NotNull] this IMutableIndex index, [NotNull] IReadOnlyList<string> properties)
            => index.SetOrRemoveAnnotation(
                NpgsqlAnnotationNames.IndexInclude,
                properties);

        /// <summary>
        /// Sets included property names.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <param name="properties">The value to set.</param>
        public static void SetIncludeProperties(
            [NotNull] this IConventionIndex index, [NotNull] IReadOnlyList<string> properties, bool fromDataAnnotation = false)
            => index.SetOrRemoveAnnotation(
                NpgsqlAnnotationNames.IndexInclude,
                properties,
                fromDataAnnotation);

        /// <summary>
        /// Returns the <see cref="ConfigurationSource" /> for the included property names.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the included property names.</returns>
        public static ConfigurationSource? GetIncludePropertiesConfigurationSource([NotNull] this IConventionIndex index)
            => index.FindAnnotation(NpgsqlAnnotationNames.IndexInclude)?.GetConfigurationSource();

        #endregion Included properties

        #region Created concurrently

        /// <summary>
        /// Returns a value indicating whether the index is created concurrently.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns><c>true</c> if the index is created concurrently.</returns>
        public static bool? IsCreatedConcurrently([NotNull] this IIndex index)
            => (bool?)index[NpgsqlAnnotationNames.CreatedConcurrently];

        /// <summary>
        /// Sets a value indicating whether the index is created concurrently.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="createdConcurrently">The value to set.</param>
        public static void SetIsCreatedConcurrently([NotNull] this IMutableIndex index, bool? createdConcurrently)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.CreatedConcurrently, createdConcurrently);

        /// <summary>
        /// Sets a value indicating whether the index is created concurrently.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="createdConcurrently">The value to set.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        public static void SetIsCreatedConcurrently(
            [NotNull] this IConventionIndex index, bool? createdConcurrently, bool fromDataAnnotation = false)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.CreatedConcurrently, createdConcurrently, fromDataAnnotation);

        /// <summary>
        /// Returns the <see cref="ConfigurationSource" /> for whether the index is created concurrently.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for whether the index is created concurrently.</returns>
        public static ConfigurationSource? GetIsCreatedConcurrentlyConfigurationSource([NotNull] this IConventionIndex index)
            => index.FindAnnotation(NpgsqlAnnotationNames.CreatedConcurrently)?.GetConfigurationSource();

        #endregion Created concurrently
    }
}
