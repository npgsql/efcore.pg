using System.Collections.Generic;
using JetBrains.Annotations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Represents type-specific annotations specific to Npgsql.
    /// </summary>
    public interface INpgsqlAlterDatabaseOperationAnnotations
    {
        [NotNull]
        [ItemNotNull]
        IReadOnlyList<IPostgresExtension> PostgresExtensions { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<IPostgresExtension> OldPostgresExtensions { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<PostgresEnum> PostgresEnums { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<PostgresEnum> OldPostgresEnums { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<PostgresRange> PostgresRanges { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<PostgresRange> OldPostgresRanges { get; }
    }
}
