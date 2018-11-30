using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Provides <see cref="CreateTableOperation"/> annotations specific to Npgsql.
    /// </summary>
    public class NpgsqlCreateTableOperationAnnotations : NpgsqlMigrationOperationAnnotations
    {
        /// <inheritdoc />
        public NpgsqlCreateTableOperationAnnotations([NotNull] CreateTableOperation operation) : base(operation) {}

        [CanBeNull]
        public virtual string CockroachDbInterleaveInParent => (string)Metadata[CockroachDbAnnotationNames.InterleaveInParent];
    }
}
