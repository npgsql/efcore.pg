
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
    ///     Sets the <see cref="PostgresMatchType"/> for a multicolumn foreign key.
    /// </summary>
    /// <param name="foreignKey">the foreign key being configured.</param>
    /// <param name="matchType">the <see cref="PostgresMatchType"/> defining the used matching strategy.</param>
    /// <remarks>
    ///     <see href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-PARMS-REFERENCES"/>
    /// </remarks>
    public static void SetMatchType(this IMutableForeignKey foreignKey, PostgresMatchType matchType)
        => foreignKey.SetOrRemoveAnnotation(NpgsqlAnnotationNames.MatchType, matchType);
}