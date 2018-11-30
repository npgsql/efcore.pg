using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Provides <see cref="AddColumnOperation"/> annotations specific to Npgsql.
    /// </summary>
    public class NpgsqlAddColumnOperationAnnotations : NpgsqlMigrationOperationAnnotations
    {
        /// <inheritdoc />
        public NpgsqlAddColumnOperationAnnotations([NotNull] AddColumnOperation operation) : base(operation) {}

        [CanBeNull]
        public virtual NpgsqlValueGenerationStrategy? ValueGenerationStrategy
            => (NpgsqlValueGenerationStrategy?)Metadata[NpgsqlAnnotationNames.ValueGenerationStrategy];
    }
}
