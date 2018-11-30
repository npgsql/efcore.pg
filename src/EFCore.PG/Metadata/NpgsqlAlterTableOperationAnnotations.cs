using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Provides <see cref="AlterTableOperation"/> annotations specific to Npgsql.
    /// </summary>
    public class NpgsqlAlterTableOperationAnnotations : NpgsqlAlterMigrationOperationAnnotations<AlterTableOperation>
    {
        /// <inheritdoc />
        public NpgsqlAlterTableOperationAnnotations([NotNull] AlterTableOperation operation) : base(operation) {}
    }
}
