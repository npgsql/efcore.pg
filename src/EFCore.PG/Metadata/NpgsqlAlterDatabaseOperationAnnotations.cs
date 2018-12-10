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
    [PublicAPI]
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

        #region Extensions

        [NotNull]
        public virtual PostgresExtension GetOrAddPostgresExtension(
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string version)
            => PostgresExtension.GetOrAddPostgresExtension((IMutableAnnotatable)Metadata, schema, name, version);

        [NotNull]
        public virtual PostgresExtension GetOrAddOldPostgresExtension(
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string version)
            => PostgresExtension.GetOrAddPostgresExtension((IMutableAnnotatable)_oldDatabase, schema, name, version);

        [NotNull]
        [ItemNotNull]
        public virtual IEnumerable<PostgresExtension> GetPostgresExtensionsToCreate()
            => PostgresExtensions.Where(ne => OldPostgresExtensions.All(oe => oe.Name != ne.Name));

        // TODO: Some forms of extension dropping are actually supported...
        [NotNull]
        [ItemNotNull]
        public virtual IEnumerable<PostgresExtension> GetPostgresExtensionsToDrop()
            => Enumerable.Empty<PostgresExtension>();

        // TODO: Some forms of extension alterations are actually supported...
        [NotNull]
        [ItemNotNull]
        public virtual IEnumerable<PostgresExtension> GetPostgresExtensionsToAlter()
            => Enumerable.Empty<PostgresExtension>();

        #endregion

        #region Enums

        [NotNull]
        public virtual PostgresEnum GetOrAddPostgresEnum(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string[] labels)
            => PostgresEnum.GetOrAddPostgresEnum((IMutableAnnotatable)Metadata, schema, name, labels);

        [NotNull]
        public virtual PostgresEnum GetOrAddOldPostgresEnum(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string[] labels)
            => PostgresEnum.GetOrAddPostgresEnum((IMutableAnnotatable)_oldDatabase, schema, name, labels);

        [NotNull]
        [ItemNotNull]
        public virtual IEnumerable<PostgresEnum> GetPostgresEnumsToCreate()
            => PostgresEnums.Where(ne => OldPostgresEnums.All(oe => oe.Name != ne.Name));

        [NotNull]
        [ItemNotNull]
        public virtual IEnumerable<PostgresEnum> GetPostgresEnumsToDrop()
            => OldPostgresEnums.Where(oe => PostgresEnums.All(ne => ne.Name != oe.Name));

        // TODO: Some forms of enum alterations are actually supported...
        [NotNull]
        [ItemNotNull]
        public virtual IEnumerable<PostgresEnum> GetPostgresEnumsToAlter()
            => Enumerable.Empty<PostgresEnum>();

        #endregion

        #region Ranges

        [NotNull]
        public virtual PostgresRange GetOrAddPostgresRange(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string subtype,
            string canonicalFunction = null,
            string subtypeOpClass = null,
            string collation = null,
            string subtypeDiff = null)
            => PostgresRange.GetOrAddPostgresRange(
                (IMutableAnnotatable)Metadata,
                schema,
                name,
                subtype,
                canonicalFunction,
                subtypeOpClass,
                collation,
                subtypeDiff);

        [NotNull]
        public virtual PostgresRange GetOrAddOldPostgresRange(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string subtype,
            string canonicalFunction = null,
            string subtypeOpClass = null,
            string collation = null,
            string subtypeDiff = null)
            => PostgresRange.GetOrAddPostgresRange(
                (IMutableAnnotatable)_oldDatabase,
                schema,
                name,
                subtype,
                canonicalFunction,
                subtypeOpClass,
                collation,
                subtypeDiff);

        [NotNull]
        [ItemNotNull]
        public virtual IEnumerable<PostgresRange> GetPostgresRangesToCreate()
            => PostgresRanges.Where(ne => OldPostgresRanges.All(oe => oe.Name != ne.Name));

        [NotNull]
        [ItemNotNull]
        public virtual IEnumerable<PostgresRange> GetPostgresRangesToDrop()
            => OldPostgresRanges.Where(oe => PostgresRanges.All(ne => ne.Name != oe.Name));

        // TODO: Some forms of range alterations are actually supported...
        [NotNull]
        [ItemNotNull]
        public virtual IEnumerable<PostgresRange> GetPostgresRangesToAlter()
            => Enumerable.Empty<PostgresRange>();

        #endregion
    }
}
