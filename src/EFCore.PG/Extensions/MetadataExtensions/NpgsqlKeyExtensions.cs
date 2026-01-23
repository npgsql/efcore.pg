using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="IKey" /> for Npgsql-specific metadata.
/// </summary>
public static class NpgsqlKeyExtensions
{
    #region WithoutOverlaps

    /// <summary>
    ///     Returns a value indicating whether the key uses the PostgreSQL WITHOUT OVERLAPS feature.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="key">The key.</param>
    /// <returns><see langword="true" /> if the key uses WITHOUT OVERLAPS.</returns>
    public static bool? GetWithoutOverlaps(this IReadOnlyKey key)
        => (bool?)key[NpgsqlAnnotationNames.WithoutOverlaps];

    /// <summary>
    ///     Sets a value indicating whether the key uses the PostgreSQL WITHOUT OVERLAPS feature.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="key">The key.</param>
    /// <param name="withoutOverlaps">The value to set.</param>
    public static void SetWithoutOverlaps(this IMutableKey key, bool? withoutOverlaps)
        => key.SetOrRemoveAnnotation(NpgsqlAnnotationNames.WithoutOverlaps, withoutOverlaps);

    /// <summary>
    ///     Sets a value indicating whether the key uses the PostgreSQL WITHOUT OVERLAPS feature.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="key">The key.</param>
    /// <param name="withoutOverlaps">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetWithoutOverlaps(this IConventionKey key, bool? withoutOverlaps, bool fromDataAnnotation = false)
    {
        key.SetOrRemoveAnnotation(NpgsqlAnnotationNames.WithoutOverlaps, withoutOverlaps, fromDataAnnotation);

        return withoutOverlaps;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for whether the key uses WITHOUT OVERLAPS.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="ConfigurationSource" />.</returns>
    public static ConfigurationSource? GetWithoutOverlapsConfigurationSource(this IConventionKey key)
        => key.FindAnnotation(NpgsqlAnnotationNames.WithoutOverlaps)?.GetConfigurationSource();

    #endregion WithoutOverlaps
}
