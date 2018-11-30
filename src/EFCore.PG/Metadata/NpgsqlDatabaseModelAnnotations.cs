using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class NpgsqlDatabaseModelAnnotations : RelationalAnnotations
    {
        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresExtension> PostgresExtensions
            => PostgresExtension.GetPostgresExtensions(Metadata).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresEnum> PostgresEnums
            => PostgresEnum.GetPostgresEnums(Metadata).ToArray();

        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<PostgresRange> PostgresRanges
            => PostgresRange.GetPostgresRanges(Metadata).ToArray();

        /// <inheritdoc />
        public NpgsqlDatabaseModelAnnotations([NotNull] DatabaseModel model) : base(model) {}

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
