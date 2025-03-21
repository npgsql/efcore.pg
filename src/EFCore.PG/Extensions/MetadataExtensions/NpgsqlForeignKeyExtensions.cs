using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Npgsql specific extension methods for <see cref="IForeignKey" />.
/// </summary>
public static class NpgsqlForeignKeyExtensions
{
    /// <summary>
    ///     Sets the <see cref="PostgresMatchStrategy" /> for a foreign key.
    /// </summary>
    /// <param name="foreignKey">the foreign key being configured.</param>
    /// <param name="matchStrategy">the <see cref="PostgresMatchStrategy" /> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES" />
    /// </remarks>
    public static void SetMatchStrategy(this IMutableForeignKey foreignKey, PostgresMatchStrategy matchStrategy)
        => foreignKey.SetOrRemoveAnnotation(NpgsqlAnnotationNames.MatchStrategy, matchStrategy);

    /// <summary>
    ///     Returns the assigned <see cref="PostgresMatchStrategy" /> for the provided foreign key
    /// </summary>
    /// <param name="foreignKey">the foreign key</param>
    /// <returns>the <see cref="PostgresMatchStrategy" /> if assigned, null otherwise</returns>
    public static PostgresMatchStrategy? GetMatchStrategy(this IReadOnlyForeignKey foreignKey)
        => (PostgresMatchStrategy?)foreignKey[NpgsqlAnnotationNames.MatchStrategy];
}
