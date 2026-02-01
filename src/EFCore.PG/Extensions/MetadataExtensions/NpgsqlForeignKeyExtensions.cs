using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="IForeignKey" /> for Npgsql-specific metadata.
/// </summary>
public static class NpgsqlForeignKeyExtensions
{
    #region Period

    /// <summary>
    ///     Returns a value indicating whether the foreign key uses the PostgreSQL PERIOD feature for temporal foreign keys.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns><see langword="true" /> if the foreign key uses PERIOD.</returns>
    public static bool? GetPeriod(this IReadOnlyForeignKey foreignKey)
        => (bool?)foreignKey[NpgsqlAnnotationNames.Period];

    /// <summary>
    ///     Sets a value indicating whether the foreign key uses the PostgreSQL PERIOD feature for temporal foreign keys.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="period">The value to set.</param>
    public static void SetPeriod(this IMutableForeignKey foreignKey, bool? period)
        => foreignKey.SetOrRemoveAnnotation(NpgsqlAnnotationNames.Period, period);

    /// <summary>
    ///     Sets a value indicating whether the foreign key uses the PostgreSQL PERIOD feature for temporal foreign keys.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="period">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetPeriod(this IConventionForeignKey foreignKey, bool? period, bool fromDataAnnotation = false)
    {
        foreignKey.SetOrRemoveAnnotation(NpgsqlAnnotationNames.Period, period, fromDataAnnotation);

        return period;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for whether the foreign key uses PERIOD.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The <see cref="ConfigurationSource" />.</returns>
    public static ConfigurationSource? GetPeriodConfigurationSource(this IConventionForeignKey foreignKey)
        => foreignKey.FindAnnotation(NpgsqlAnnotationNames.Period)?.GetConfigurationSource();

    #endregion Period
}
