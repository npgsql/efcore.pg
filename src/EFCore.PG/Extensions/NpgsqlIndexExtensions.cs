using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

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
        public static string GetNpgsqlMethod([NotNull] this IIndex index)
            => (string)index[NpgsqlAnnotationNames.IndexMethod];

        /// <summary>
        /// Sets the index method to be used, or <c>null</c> if it hasn't been specified.
        /// <c>null</c> selects the default (currently <c>btree</c>).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        public static void SetNpgsqlMethod([NotNull] this IMutableIndex index, [CanBeNull] string method)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexMethod, method);

        /// <summary>
        /// Returns the column operators to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        public static IReadOnlyList<string> GetNpgsqlOperators([NotNull] this IIndex index)
            => (IReadOnlyList<string>)index[NpgsqlAnnotationNames.IndexOperators];

        /// <summary>
        /// Sets the column operators to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        public static void SetNpgsqlOperators([NotNull] this IMutableIndex index, [CanBeNull] IReadOnlyList<string> operators)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexMethod, operators);

        /// <summary>
        /// Returns the column collations to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        public static IReadOnlyList<string> GetNpgsqlCollation([NotNull] this IIndex index)
            => (IReadOnlyList<string>)index[NpgsqlAnnotationNames.IndexCollation];

        /// <summary>
        /// Sets the column collations to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        public static void SetNpgsqlCollation([NotNull] this IMutableIndex index, [CanBeNull] IReadOnlyList<string> collations)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexCollation, collations);

        /// <summary>
        /// Returns the column sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public static IReadOnlyList<SortOrder> GetNpgsqlSortOrder([NotNull] this IIndex index)
            => (IReadOnlyList<SortOrder>)index[NpgsqlAnnotationNames.IndexSortOrder];

        /// <summary>
        /// Sets the column sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public static void SetNpgsqlSortOrder([NotNull] this IMutableIndex index, [CanBeNull] IReadOnlyList<SortOrder> sortOrder)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexSortOrder, sortOrder);

        /// <summary>
        /// Returns the column NULL sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public static IReadOnlyList<NullSortOrder> GetNpgsqlNullSortOrder([NotNull] this IIndex index)
            => (IReadOnlyList<NullSortOrder>)index[NpgsqlAnnotationNames.IndexNullSortOrder];

        /// <summary>
        /// Sets the column NULL sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public static void SetNpgsqlNullSortOrder([NotNull] this IMutableIndex index, [CanBeNull] IReadOnlyList<NullSortOrder> nullSortOrder)
            => index.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IndexNullSortOrder, nullSortOrder);


        #region Included properties

        /// <summary>
        /// Returns included property names, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The included property names, or <c>null</c> if they have not been specified.</returns>
        public static IReadOnlyList<string> GetNpgsqlIncludeProperties([NotNull] this IIndex index)
            => (IReadOnlyList<string>)index[NpgsqlAnnotationNames.IndexInclude];

        /// <summary>
        /// Sets included property names.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="properties">The value to set.</param>
        public static void SetNpgsqlIncludeProperties([NotNull] this IMutableIndex index, [NotNull] IReadOnlyList<string> properties)
            => index.SetOrRemoveAnnotation(
                NpgsqlAnnotationNames.IndexInclude,
                properties);

        /// <summary>
        /// Sets included property names.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <param name="properties">The value to set.</param>
        public static void SetNpgsqlIncludeProperties(
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
        public static ConfigurationSource? GetNpgsqlIncludePropertiesConfigurationSource([NotNull] this IConventionIndex index)
            => index.FindAnnotation(NpgsqlAnnotationNames.IndexInclude)?.GetConfigurationSource();

        #endregion Included properties
    }
}
