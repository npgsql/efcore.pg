using System.Collections.Generic;
using JetBrains.Annotations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public interface INpgsqlDatabaseModelAnnotations
    {
        [NotNull]
        [ItemNotNull]
        IReadOnlyList<IPostgresExtension> PostgresExtensions { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<PostgresEnum> PostgresEnums { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<PostgresRange> PostgresRanges { get; }
    }
}
