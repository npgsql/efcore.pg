
// ReSharper disable once CheckNamespace
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Npgsql specific extension methods for <see cref="IForeignKey"/>.
/// </summary>
public static class NpgsqlForeignKeyExtensions
{
    /// <summary>
    ///     Sets the <see cref="PostgresMatchStrategy"/> for a foreign key.
    /// </summary>
    /// <param name="foreignKey">the foreign key being configured.</param>
    /// <param name="matchType">the <see cref="PostgresMatchStrategy"/> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES"/>
    /// </remarks>
    public static void SetMatchType(this IMutableForeignKey foreignKey, PostgresMatchStrategy matchType)
        => foreignKey.SetOrRemoveAnnotation(NpgsqlAnnotationNames.MatchType, matchType);

    /// <summary>
    ///     Returns the assigned MATCH strategy for the provided foreign key
    /// </summary>
    /// <param name="foreignKey">the foreign key</param>
    /// <returns>the <see cref="MatchType"/> if assigned, null otherwise</returns>
    public static PostgresMatchStrategy? GetMatchType(this IReadOnlyForeignKey foreignKey) => 
        (PostgresMatchStrategy?)foreignKey[NpgsqlAnnotationNames.MatchType];
}