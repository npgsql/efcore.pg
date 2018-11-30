using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Provides <see cref="CreateIndexOperation"/> annotations specific to Npgsql.
    /// </summary>
    public class NpgsqlCreateIndexOperationAnnotations : NpgsqlMigrationOperationAnnotations
    {
        /// <inheritdoc />
        public NpgsqlCreateIndexOperationAnnotations([NotNull] CreateIndexOperation operation) : base(operation) {}

        [CanBeNull]
        public virtual string Method => (string)Metadata[NpgsqlAnnotationNames.IndexMethod];

        [CanBeNull]
        [ItemCanBeNull]
        public virtual string[] Operators => (string[])Metadata[NpgsqlAnnotationNames.IndexOperators];

        [CanBeNull]
        [ItemCanBeNull]
        public virtual string[] IncludeProperties => (string[])Metadata[NpgsqlAnnotationNames.IndexInclude];
    }
}
