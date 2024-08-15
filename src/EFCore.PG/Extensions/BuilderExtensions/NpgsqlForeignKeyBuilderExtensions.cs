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
    /// <param name="matchType">The <see cref="PostgresMatchStrategy"/> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES"/>
    /// </remarks>
    public static ReferenceReferenceBuilder UsesMatchStrategy(this ReferenceReferenceBuilder builder, PostgresMatchStrategy matchType){
        Check.NotNull(builder, nameof(builder));
        Check.IsDefined(matchType);
        builder.Metadata.SetMatchType(matchType);
        return builder;
    }

    /// <summary>
    ///     Configure the matching strategy to be used with the foreign key.
    /// </summary>
    /// <param name="builder">The builder for the foreign key being configured.</param>
    /// <param name="matchType">The <see cref="PostgresMatchStrategy"/> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES"/>
    /// </remarks>
    public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> UsesMatchStrategy<TEntity, TRelatedEntity>(this ReferenceReferenceBuilder<TEntity, TRelatedEntity> builder, PostgresMatchStrategy matchType) 
        where TEntity : class
        where TRelatedEntity : class
            => (ReferenceReferenceBuilder<TEntity, TRelatedEntity>)UsesMatchStrategy((ReferenceReferenceBuilder)builder, matchType);

    /// <summary>
    ///     Configure the matching strategy to be used with the foreign key.
    /// </summary>
    /// <param name="builder">The builder for the foreign key being configured.</param>
    /// <param name="matchType">The <see cref="PostgresMatchStrategy"/> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES"/>
    /// </remarks>
    public static ReferenceCollectionBuilder UsesMatchStrategy(this ReferenceCollectionBuilder builder, PostgresMatchStrategy matchType){
        Check.NotNull(builder, nameof(builder));
        Check.IsDefined(matchType);
        builder.Metadata.SetMatchType(matchType);
        return builder;
    }

    /// <summary>
    ///     Configure the matching strategy to be used with the foreign key.
    /// </summary>
    /// <param name="builder">The builder for the foreign key being configured.</param>
    /// <param name="matchType">The <see cref="PostgresMatchStrategy"/> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES"/>
    /// </remarks>
    public static ReferenceCollectionBuilder UsesMatchStrategy<TEntity, TRelatedEntity>(this ReferenceCollectionBuilder<TEntity, TRelatedEntity> builder, PostgresMatchStrategy matchType)
        where TEntity : class
        where TRelatedEntity : class
        => (ReferenceCollectionBuilder<TEntity, TRelatedEntity>)UsesMatchStrategy((ReferenceCollectionBuilder)builder, matchType);
}