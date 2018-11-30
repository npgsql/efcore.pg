using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Provides <see cref="MigrationOperation"/> annotations specific to Npgsql.
    /// </summary>
    public class NpgsqlMigrationOperationAnnotations : RelationalAnnotations
    {
        /// <inheritdoc />
        protected NpgsqlMigrationOperationAnnotations([NotNull] MigrationOperation operation) : base(operation) {}

        [CanBeNull]
        public virtual string Comment => (string)Metadata[NpgsqlAnnotationNames.Comment];
    }
}
