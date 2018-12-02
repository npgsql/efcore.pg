using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Provides relational-specific annotations specific to Npgsql.
    /// </summary>
    public class NpgsqlAlterDatabaseOperationAnnotations : RelationalAnnotations
    {
        [NotNull] readonly IAnnotatable _oldDatabase;

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresExtension> PostgresExtensions
            => PostgresExtension.GetPostgresExtensions(Metadata).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresExtension> OldPostgresExtensions
            => PostgresExtension.GetPostgresExtensions(_oldDatabase).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresEnum> PostgresEnums
            => PostgresEnum.GetPostgresEnums(Metadata).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresEnum> OldPostgresEnums
            => PostgresEnum.GetPostgresEnums(_oldDatabase).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresRange> PostgresRanges
            => PostgresRange.GetPostgresRanges(Metadata).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresRange> OldPostgresRanges
            => PostgresRange.GetPostgresRanges(_oldDatabase).ToArray();

        /// <inheritdoc />
        public NpgsqlAlterDatabaseOperationAnnotations([NotNull] AlterDatabaseOperation operation)
            : base(operation)
            => _oldDatabase = operation.OldDatabase;

        [NotNull]
        public virtual PostgresExtension GetOrAddPostgresExtension(
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string version)
            => PostgresExtension.GetOrAddPostgresExtension((IMutableAnnotatable)Metadata, schema, name, version);
    }
}
