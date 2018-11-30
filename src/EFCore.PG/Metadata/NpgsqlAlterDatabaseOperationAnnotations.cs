using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Provides <see cref="AlterDatabaseOperation"/> annotations specific to Npgsql.
    /// </summary>
    public class NpgsqlAlterDatabaseOperationAnnotations : NpgsqlAlterMigrationOperationAnnotations<AlterDatabaseOperation>
    {
        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresExtension> PostgresExtensions
            => PostgresExtension.GetPostgresExtensions(Metadata).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresExtension> OldPostgresExtensions
            => PostgresExtension.GetPostgresExtensions(OldMetadata).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresEnum> PostgresEnums
            => PostgresEnum.GetPostgresEnums(Metadata).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresEnum> OldPostgresEnums
            => PostgresEnum.GetPostgresEnums(OldMetadata).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresRange> PostgresRanges
            => PostgresRange.GetPostgresRanges(Metadata).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresRange> OldPostgresRanges
            => PostgresRange.GetPostgresRanges(OldMetadata).ToArray();

        /// <inheritdoc />
        public NpgsqlAlterDatabaseOperationAnnotations([NotNull] AlterDatabaseOperation operation) : base(operation) {}

        [NotNull]
        public virtual PostgresExtension GetOrAddPostgresExtension(
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string version)
            => PostgresExtension.GetOrAddPostgresExtension((IMutableAnnotatable)Metadata, schema, name, version);

        [NotNull]
        public virtual PostgresEnum GetOrAddPostgresEnum(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string[] labels)
            => PostgresEnum.GetOrAddPostgresEnum((IMutableAnnotatable)Metadata, schema, name, labels);
    }
}
