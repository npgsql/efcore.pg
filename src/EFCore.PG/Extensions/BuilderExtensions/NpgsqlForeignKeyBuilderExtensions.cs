using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Npgsql specific extension methods for relationship builders.
/// </summary>
public static class NpgsqlForeignKeyBuilderExtensions
{
    #region Period

    /// <summary>
    ///     Configures the foreign key to use the PostgreSQL PERIOD feature for temporal foreign keys.
    ///     The last column in the foreign key must be a PostgreSQL range type, and the referenced
    ///     principal key must have WITHOUT OVERLAPS configured.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="referenceCollectionBuilder">The builder being used to configure the relationship.</param>
    /// <param name="withPeriod">A value indicating whether to use PERIOD.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ReferenceCollectionBuilder WithPeriod(
        this ReferenceCollectionBuilder referenceCollectionBuilder,
        bool withPeriod = true)
    {
        Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder));

        referenceCollectionBuilder.Metadata.SetPeriod(withPeriod);

        return referenceCollectionBuilder;
    }

    /// <summary>
    ///     Configures the foreign key to use the PostgreSQL PERIOD feature for temporal foreign keys.
    ///     The last column in the foreign key must be a PostgreSQL range type, and the referenced
    ///     principal key must have WITHOUT OVERLAPS configured.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="referenceCollectionBuilder">The builder being used to configure the relationship.</param>
    /// <param name="withPeriod">A value indicating whether to use PERIOD.</param>
    /// <typeparam name="TEntity">The principal entity type in this relationship.</typeparam>
    /// <typeparam name="TRelatedEntity">The dependent entity type in this relationship.</typeparam>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ReferenceCollectionBuilder<TEntity, TRelatedEntity> WithPeriod<TEntity, TRelatedEntity>(
        this ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder,
        bool withPeriod = true)
        where TEntity : class
        where TRelatedEntity : class
        => (ReferenceCollectionBuilder<TEntity, TRelatedEntity>)WithPeriod(
            (ReferenceCollectionBuilder)referenceCollectionBuilder, withPeriod);

    /// <summary>
    ///     Configures the foreign key to use the PostgreSQL PERIOD feature for temporal foreign keys.
    ///     The last column in the foreign key must be a PostgreSQL range type, and the referenced
    ///     principal key must have WITHOUT OVERLAPS configured.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="referenceReferenceBuilder">The builder being used to configure the relationship.</param>
    /// <param name="withPeriod">A value indicating whether to use PERIOD.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ReferenceReferenceBuilder WithPeriod(
        this ReferenceReferenceBuilder referenceReferenceBuilder,
        bool withPeriod = true)
    {
        Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));

        referenceReferenceBuilder.Metadata.SetPeriod(withPeriod);

        return referenceReferenceBuilder;
    }

    /// <summary>
    ///     Configures the foreign key to use the PostgreSQL PERIOD feature for temporal foreign keys.
    ///     The last column in the foreign key must be a PostgreSQL range type, and the referenced
    ///     principal key must have WITHOUT OVERLAPS configured.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="referenceReferenceBuilder">The builder being used to configure the relationship.</param>
    /// <param name="withPeriod">A value indicating whether to use PERIOD.</param>
    /// <typeparam name="TEntity">The entity type on one end of the relationship.</typeparam>
    /// <typeparam name="TRelatedEntity">The entity type on the other end of the relationship.</typeparam>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> WithPeriod<TEntity, TRelatedEntity>(
        this ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder,
        bool withPeriod = true)
        where TEntity : class
        where TRelatedEntity : class
        => (ReferenceReferenceBuilder<TEntity, TRelatedEntity>)WithPeriod(
            (ReferenceReferenceBuilder)referenceReferenceBuilder, withPeriod);

    /// <summary>
    ///     Configures the foreign key to use the PostgreSQL PERIOD feature for temporal foreign keys.
    ///     The last column in the foreign key must be a PostgreSQL range type, and the referenced
    ///     principal key must have WITHOUT OVERLAPS configured.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="ownershipBuilder">The builder being used to configure the relationship.</param>
    /// <param name="withPeriod">A value indicating whether to use PERIOD.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnershipBuilder WithPeriod(
        this OwnershipBuilder ownershipBuilder,
        bool withPeriod = true)
    {
        Check.NotNull(ownershipBuilder, nameof(ownershipBuilder));

        ownershipBuilder.Metadata.SetPeriod(withPeriod);

        return ownershipBuilder;
    }

    /// <summary>
    ///     Configures the foreign key to use the PostgreSQL PERIOD feature for temporal foreign keys.
    ///     The last column in the foreign key must be a PostgreSQL range type, and the referenced
    ///     principal key must have WITHOUT OVERLAPS configured.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="ownershipBuilder">The builder being used to configure the relationship.</param>
    /// <param name="withPeriod">A value indicating whether to use PERIOD.</param>
    /// <typeparam name="TEntity">The entity type on one end of the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The entity type on the other end of the relationship.</typeparam>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnershipBuilder<TEntity, TDependentEntity> WithPeriod<TEntity, TDependentEntity>(
        this OwnershipBuilder<TEntity, TDependentEntity> ownershipBuilder,
        bool withPeriod = true)
        where TEntity : class
        where TDependentEntity : class
        => (OwnershipBuilder<TEntity, TDependentEntity>)WithPeriod(
            (OwnershipBuilder)ownershipBuilder, withPeriod);

    /// <summary>
    ///     Configures the foreign key to use the PostgreSQL PERIOD feature for temporal foreign keys.
    ///     The last column in the foreign key must be a PostgreSQL range type, and the referenced
    ///     principal key must have WITHOUT OVERLAPS configured.
    /// </summary>
    /// <remarks>
    ///     See https://www.postgresql.org/docs/current/sql-createtable.html for more information.
    /// </remarks>
    /// <param name="foreignKeyBuilder">The builder being used to configure the relationship.</param>
    /// <param name="withPeriod">A value indicating whether to use PERIOD.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionForeignKeyBuilder? WithPeriod(
        this IConventionForeignKeyBuilder foreignKeyBuilder,
        bool? withPeriod = true,
        bool fromDataAnnotation = false)
    {
        if (foreignKeyBuilder.CanSetPeriod(withPeriod, fromDataAnnotation))
        {
            foreignKeyBuilder.Metadata.SetPeriod(withPeriod, fromDataAnnotation);

            return foreignKeyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether PERIOD can be configured.
    /// </summary>
    /// <param name="foreignKeyBuilder">The builder being used to configure the relationship.</param>
    /// <param name="withPeriod">A value indicating whether to use PERIOD.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the foreign key can be configured with PERIOD.</returns>
    public static bool CanSetPeriod(
        this IConventionForeignKeyBuilder foreignKeyBuilder,
        bool? withPeriod = true,
        bool fromDataAnnotation = false)
    {
        Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));

        return foreignKeyBuilder.CanSetAnnotation(NpgsqlAnnotationNames.Period, withPeriod, fromDataAnnotation);
    }

    #endregion Period
}
