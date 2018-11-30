using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Provides <see cref="IAlterMigrationOperation"/> annotations specific to Npgsql.
    /// </summary>
    public abstract class NpgsqlAlterMigrationOperationAnnotations<T> : NpgsqlMigrationOperationAnnotations where T : MigrationOperation, IAlterMigrationOperation
    {
        [NotNull]
        protected virtual IAnnotatable OldMetadata { get; }

        /// <inheritdoc />
        protected NpgsqlAlterMigrationOperationAnnotations([NotNull] T operation)
            : base(operation)
            => OldMetadata = operation.OldAnnotations;

        [CanBeNull]
        public virtual string OldComment => (string)OldMetadata[NpgsqlAnnotationNames.Comment];
    }
}
