using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Npgsql specific extension methods for <see cref="KeyBuilder" />.
/// </summary>
public static class NpgsqlKeyBuilderExtensions
{
    #region WithoutOverlaps

    /// <summary>
    ///     Configures the key to use the PostgreSQL WITHOUT OVERLAPS feature.
    ///     The last property in the key must be a PostgreSQL range type.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="withoutOverlaps">A value indicating whether to use WITHOUT OVERLAPS.</param>
    /// <returns>A builder to further configure the key.</returns>
    public static KeyBuilder WithoutOverlaps(this KeyBuilder keyBuilder, bool withoutOverlaps = true)
    {
        Check.NotNull(keyBuilder, nameof(keyBuilder));

        keyBuilder.Metadata.SetWithoutOverlaps(withoutOverlaps);

        return keyBuilder;
    }

    /// <summary>
    ///     Configures the key to use the PostgreSQL WITHOUT OVERLAPS feature.
    ///     The last property in the key must be a PostgreSQL range type.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="withoutOverlaps">A value indicating whether to use WITHOUT OVERLAPS.</param>
    /// <returns>A builder to further configure the key.</returns>
    public static KeyBuilder<TEntity> WithoutOverlaps<TEntity>(this KeyBuilder<TEntity> keyBuilder, bool withoutOverlaps = true)
        => (KeyBuilder<TEntity>)WithoutOverlaps((KeyBuilder)keyBuilder, withoutOverlaps);

    /// <summary>
    ///     Configures the key to use the PostgreSQL WITHOUT OVERLAPS feature.
    ///     The last property in the key must be a PostgreSQL range type.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="withoutOverlaps">A value indicating whether to use WITHOUT OVERLAPS.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the key.</returns>
    public static IConventionKeyBuilder? WithoutOverlaps(
        this IConventionKeyBuilder keyBuilder,
        bool? withoutOverlaps = true,
        bool fromDataAnnotation = false)
    {
        if (keyBuilder.CanSetWithoutOverlaps(withoutOverlaps, fromDataAnnotation))
        {
            keyBuilder.Metadata.SetWithoutOverlaps(withoutOverlaps, fromDataAnnotation);

            return keyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether WITHOUT OVERLAPS can be configured.
    /// </summary>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="withoutOverlaps">A value indicating whether to use WITHOUT OVERLAPS.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the key can be configured with WITHOUT OVERLAPS.</returns>
    public static bool CanSetWithoutOverlaps(
        this IConventionKeyBuilder keyBuilder,
        bool? withoutOverlaps = true,
        bool fromDataAnnotation = false)
    {
        Check.NotNull(keyBuilder, nameof(keyBuilder));

        return keyBuilder.CanSetAnnotation(NpgsqlAnnotationNames.WithoutOverlaps, withoutOverlaps, fromDataAnnotation);
    }

    #endregion WithoutOverlaps
}
