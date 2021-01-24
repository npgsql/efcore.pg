using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Extension methods for <see cref="AlterDatabaseOperation" /> for Npgsql-specific metadata.
    /// </summary>
    public static class NpgsqlAlterDatabaseOperationExtensions
    {
        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<PostgresCollation> GetPostgresCollations([NotNull] this AlterDatabaseOperation operation)
            => PostgresCollation.GetCollations(operation).ToArray();

        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<PostgresCollation> GetOldPostgresCollations([NotNull] this AlterDatabaseOperation operation)
            => PostgresCollation.GetCollations(operation.OldDatabase).ToArray();

        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<PostgresExtension> GetPostgresExtensions([NotNull] this AlterDatabaseOperation operation)
            => PostgresExtension.GetPostgresExtensions(operation).ToArray();

        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<PostgresExtension> GetOldPostgresExtensions([NotNull] this AlterDatabaseOperation operation)
            => PostgresExtension.GetPostgresExtensions(operation.OldDatabase).ToArray();

        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<PostgresEnum> GetPostgresEnums([NotNull] this AlterDatabaseOperation operation)
            => PostgresEnum.GetPostgresEnums(operation).ToArray();

        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<PostgresEnum> GetOldPostgresEnums([NotNull] this AlterDatabaseOperation operation)
            => PostgresEnum.GetPostgresEnums(operation.OldDatabase).ToArray();

        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<PostgresRange> GetPostgresRanges([NotNull] this AlterDatabaseOperation operation)
            => PostgresRange.GetPostgresRanges(operation).ToArray();

        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<PostgresRange> GetOldPostgresRanges([NotNull] this AlterDatabaseOperation operation)
            => PostgresRange.GetPostgresRanges(operation.OldDatabase).ToArray();

        [NotNull]
        public static PostgresExtension GetOrAddPostgresExtension(
            [NotNull] this AlterDatabaseOperation operation,
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string version)
            => PostgresExtension.GetOrAddPostgresExtension(operation, schema, name, version);
    }
}
