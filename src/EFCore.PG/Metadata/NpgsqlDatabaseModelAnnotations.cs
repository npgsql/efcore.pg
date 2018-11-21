using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class NpgsqlDatabaseModelAnnotations : RelationalAnnotations, INpgsqlDatabaseModelAnnotations
    {
        public virtual IReadOnlyList<IPostgresExtension> PostgresExtensions
            => PostgresExtension.GetPostgresExtensions(Metadata).ToArray();

        public virtual IReadOnlyList<PostgresEnum> PostgresEnums
            => PostgresEnum.GetPostgresEnums(Metadata).ToArray();

        public virtual IReadOnlyList<PostgresRange> PostgresRanges
            => PostgresRange.GetPostgresRanges(Metadata).ToArray();

        /// <inheritdoc />
        public NpgsqlDatabaseModelAnnotations([NotNull] DatabaseModel model) : base(model) {}
    }
}
