using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Provides <see cref="AlterColumnOperation"/> annotations specific to Npgsql.
    /// </summary>
    public class NpgsqlAlterColumnOperationAnnotations : NpgsqlAlterMigrationOperationAnnotations<AlterColumnOperation>
    {
        /// <inheritdoc />
        public NpgsqlAlterColumnOperationAnnotations([NotNull] AlterColumnOperation operation) : base(operation) {}

        [CanBeNull]
        public virtual NpgsqlValueGenerationStrategy? ValueGenerationStrategy
            => (NpgsqlValueGenerationStrategy?)Metadata[NpgsqlAnnotationNames.ValueGenerationStrategy];

        [CanBeNull]
        public virtual NpgsqlValueGenerationStrategy? OldValueGenerationStrategy
            => (NpgsqlValueGenerationStrategy?)OldMetadata[NpgsqlAnnotationNames.ValueGenerationStrategy];
    }
}
