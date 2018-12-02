using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations
{
    /// <summary>
    /// The default migrations model differ for Npgsql.
    /// </summary>
    public class NpgsqlMigrationsModelDiffer : MigrationsModelDiffer
    {
        public NpgsqlMigrationsModelDiffer(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] IMigrationsAnnotationProvider migrationsAnnotations,
            [NotNull] IChangeDetector changeDetector,
            [NotNull] StateManagerDependencies stateManagerDependencies,
            [NotNull] CommandBatchPreparerDependencies commandBatchPreparerDependencies)
            : base(typeMappingSource, migrationsAnnotations, changeDetector, stateManagerDependencies, commandBatchPreparerDependencies) {}

        // TODO: filter the base diff to remove annotations now handled here.
        protected override IEnumerable<MigrationOperation> Diff(IModel source, IModel target, DiffContext diffContext)
            => base.Diff(source, target, diffContext)
                   .Concat(DiffAnnotations(source, target));

        private IEnumerable<MigrationOperation> DiffAnnotations([CanBeNull] IModel source, [CanBeNull] IModel target)
        {
            // TODO: diff extensions, enums, and ranges here.
            yield break;
        }

        [NotNull]
        [ItemNotNull]
        protected override IEnumerable<string> GetSchemas(IModel model)
            => base.GetSchemas(model)
                   .Concat(model.Npgsql().PostgresExtensions.Select(x => x.Schema))
                   .Concat(model.Npgsql().PostgresEnums.Select(x => x.Schema))
                   .Concat(model.Npgsql().PostgresRanges.Select(x => x.Schema))
                   .Where(x => !string.IsNullOrEmpty(x))
                   .Distinct();
    }
}
