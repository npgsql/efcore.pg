using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Npgsql specific extension methods for configuring foreign keys.
/// </summary>
public static class NpgsqlForeignKeyBuilderExtensions
{
    /// <summary>
    ///     Configure the matching strategy to be used with the multicolumn foreign keys.
    /// </summary>
    /// <param name="builder">The builder for the foreign key being configured.</param>
    /// <param name="matchType">The <see cref="PostgresMatchType"/> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES"/>
    /// </remarks>
    public static ReferenceReferenceBuilder HasMatchType(this ReferenceReferenceBuilder builder, PostgresMatchType matchType){
        builder.Metadata.SetMatchType(matchType);
        return builder;
    }

    /// <summary>
    ///     Configure the matching strategy to be used with the multicolumn foreign keys.
    /// </summary>
    /// <param name="builder">The builder for the foreign key being configured.</param>
    /// <param name="matchType">The <see cref="PostgresMatchType"/> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES"/>
    /// </remarks>
    public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasMatchType<TEntity, TRelatedEntity>(this ReferenceReferenceBuilder<TEntity, TRelatedEntity> builder, PostgresMatchType matchType) 
        where TEntity : class
        where TRelatedEntity : class
            => (ReferenceReferenceBuilder<TEntity, TRelatedEntity>)HasMatchType((ReferenceReferenceBuilder)builder, matchType);

    /// <summary>
    ///     Configure the matching strategy to be used with the multicolumn foreign keys.
    /// </summary>
    /// <param name="builder">The builder for the foreign key being configured.</param>
    /// <param name="matchType">The <see cref="PostgresMatchType"/> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES"/>
    /// </remarks>
    public static ReferenceCollectionBuilder HasMatchType(this ReferenceCollectionBuilder builder, PostgresMatchType matchType){
        builder.Metadata.SetMatchType(matchType);
        return builder;
    }

    /// <summary>
    ///     Configure the matching strategy to be used with the multicolumn foreign keys.
    /// </summary>
    /// <param name="builder">The builder for the foreign key being configured.</param>
    /// <param name="matchType">The <see cref="PostgresMatchType"/> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES"/>
    /// </remarks>
    public static ReferenceCollectionBuilder HasMatchType<TEntity, TRelatedEntity>(this ReferenceCollectionBuilder<TEntity, TRelatedEntity> builder, PostgresMatchType matchType)
        where TEntity : class
        where TRelatedEntity : class
        => (ReferenceCollectionBuilder<TEntity, TRelatedEntity>)HasMatchType((ReferenceCollectionBuilder)builder, matchType);
}