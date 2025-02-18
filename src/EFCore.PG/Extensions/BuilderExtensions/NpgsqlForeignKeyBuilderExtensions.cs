using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Npgsql specific extension methods for configuring foreign keys.
/// </summary>
public static class NpgsqlForeignKeyBuilderExtensions
{
    /// <summary>
    ///     Configure the matching strategy to be used with the foreign key.
    /// </summary>
    /// <param name="builder">The builder for the foreign key being configured.</param>
    /// <param name="matchStrategy">The <see cref="PostgresMatchStrategy" /> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES" />
    /// </remarks>
    public static ReferenceReferenceBuilder UsesMatchStrategy(this ReferenceReferenceBuilder builder, PostgresMatchStrategy matchStrategy)
    {
        Check.NotNull(builder, nameof(builder));
        Check.IsDefined(matchStrategy, nameof(matchStrategy));
        builder.Metadata.SetMatchStrategy(matchStrategy);
        return builder;
    }

    /// <summary>
    ///     Configure the matching strategy to be used with the foreign key.
    /// </summary>
    /// <param name="builder">The builder for the foreign key being configured.</param>
    /// <param name="matchStrategy">The <see cref="PostgresMatchStrategy" /> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES" />
    /// </remarks>
    public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> UsesMatchStrategy<TEntity, TRelatedEntity>(
        this ReferenceReferenceBuilder<TEntity, TRelatedEntity> builder,
        PostgresMatchStrategy matchStrategy)
        where TEntity : class
        where TRelatedEntity : class
        => (ReferenceReferenceBuilder<TEntity, TRelatedEntity>)UsesMatchStrategy((ReferenceReferenceBuilder)builder, matchStrategy);

    /// <summary>
    ///     Configure the matching strategy to be used with the foreign key.
    /// </summary>
    /// <param name="builder">The builder for the foreign key being configured.</param>
    /// <param name="matchStrategy">The <see cref="PostgresMatchStrategy" /> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES" />
    /// </remarks>
    public static ReferenceCollectionBuilder UsesMatchStrategy(this ReferenceCollectionBuilder builder, PostgresMatchStrategy matchStrategy)
    {
        Check.NotNull(builder, nameof(builder));
        Check.IsDefined(matchStrategy, nameof(matchStrategy));
        builder.Metadata.SetMatchStrategy(matchStrategy);
        return builder;
    }

    /// <summary>
    ///     Configure the matching strategy to be used with the foreign key.
    /// </summary>
    /// <param name="builder">The builder for the foreign key being configured.</param>
    /// <param name="matchStrategy">The <see cref="PostgresMatchStrategy" /> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES" />
    /// </remarks>
    public static ReferenceCollectionBuilder UsesMatchStrategy<TEntity, TRelatedEntity>(
        this ReferenceCollectionBuilder<TEntity, TRelatedEntity> builder,
        PostgresMatchStrategy matchStrategy)
        where TEntity : class
        where TRelatedEntity : class
        => (ReferenceCollectionBuilder<TEntity, TRelatedEntity>)UsesMatchStrategy((ReferenceCollectionBuilder)builder, matchStrategy);
}
