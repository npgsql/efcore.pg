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
        /// <inheritdoc />
        public NpgsqlMigrationsModelDiffer(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] IMigrationsAnnotationProvider migrationsAnnotations,
            [NotNull] IChangeDetector changeDetector,
            [NotNull] StateManagerDependencies stateManagerDependencies,
            [NotNull] CommandBatchPreparerDependencies commandBatchPreparerDependencies)
            : base(typeMappingSource, migrationsAnnotations, changeDetector, stateManagerDependencies, commandBatchPreparerDependencies) {}

        [NotNull]
        [ItemNotNull]
        protected override IEnumerable<string> GetSchemas(IModel model)
            => base.GetSchemas(model)
                   .Concat(model.Npgsql().PostgresExtensions.Select(x => x.Schema))
                   .Concat(model.Npgsql().PostgresEnums.Select(x => x.Schema))
                   .Concat(model.Npgsql().PostgresRanges.Select(x => x.Schema))
                   .Where(x => !string.IsNullOrEmpty(x))
                   .Distinct();

        protected override IEnumerable<MigrationOperation> Diff(IModel source, IModel target, DiffContext diffContext)
            => base.Diff(source, target, diffContext)
                   .Concat(DiffExtensions(source, target))
                   .Concat(DiffEnums(source, target))
                   .Concat(DiffRanges(source, target));

        [NotNull]
        [ItemNotNull]
        protected virtual IEnumerable<MigrationOperation> DiffExtensions([CanBeNull] IModel source, [CanBeNull] IModel target)
        {
            var sourceExtensions = source?.Npgsql().PostgresExtensions;
            var targetExtensions = target?.Npgsql().PostgresExtensions;

            if (source == null && targetExtensions != null)
            {
                if (targetExtensions.Count == 0)
                    yield break;

                var alterDatabaseOperation = new AlterDatabaseOperation();

                foreach (var postgresExtension in targetExtensions)
                {
                    alterDatabaseOperation.Npgsql().GetOrAddPostgresExtension(
                        postgresExtension.Schema,
                        postgresExtension.Name,
                        postgresExtension.Version);
                }

                yield return alterDatabaseOperation;
                yield break;
            }

            if (source != null && target == null)
            {
                if (sourceExtensions.Count == 0)
                    yield break;

                var alterDatabaseOperation = new AlterDatabaseOperation();

                foreach (var postgresExtension in sourceExtensions)
                {
                    alterDatabaseOperation.Npgsql().GetOrAddOldPostgresExtension(
                        postgresExtension.Schema,
                        postgresExtension.Name,
                        postgresExtension.Version);
                }

                yield return alterDatabaseOperation;
                yield break;
            }

            // TODO: annotate for alter operations
//            if (HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
//            {
//                var alterDatabaseOperation = new AlterDatabaseOperation();
//                alterDatabaseOperation.AddAnnotations(targetMigrationsAnnotations);
//                alterDatabaseOperation.OldDatabase.AddAnnotations(sourceMigrationsAnnotations);
//                yield return alterDatabaseOperation;
//            }
        }

        [NotNull]
        [ItemNotNull]
        protected virtual IEnumerable<MigrationOperation> DiffEnums([CanBeNull] IModel source, [CanBeNull] IModel target)
        {
            var sourceEnums = source?.Npgsql().PostgresEnums;
            var targetEnums = target?.Npgsql().PostgresEnums;

            if (source == null && targetEnums != null)
            {
                if (targetEnums.Count == 0)
                    yield break;

                var alterDatabaseOperation = new AlterDatabaseOperation();

                foreach (var postgresEnum in targetEnums)
                {
                    alterDatabaseOperation.Npgsql().GetOrAddPostgresEnum(
                        postgresEnum.Schema,
                        postgresEnum.Name,
                        postgresEnum.Labels.ToArray());
                }

                yield return alterDatabaseOperation;
                yield break;
            }

            if (source != null && target == null)
            {
                if (sourceEnums.Count == 0)
                    yield break;

                var alterDatabaseOperation = new AlterDatabaseOperation();

                foreach (var postgresEnum in sourceEnums)
                {
                    alterDatabaseOperation.Npgsql().GetOrAddOldPostgresEnum(
                        postgresEnum.Schema,
                        postgresEnum.Name,
                        postgresEnum.Labels.ToArray());
                }

                yield return alterDatabaseOperation;
                yield break;
            }

            // TODO: annotate for alter operations
//            if (HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
//            {
//                var alterDatabaseOperation = new AlterDatabaseOperation();
//                alterDatabaseOperation.AddAnnotations(targetMigrationsAnnotations);
//                alterDatabaseOperation.OldDatabase.AddAnnotations(sourceMigrationsAnnotations);
//                yield return alterDatabaseOperation;
//            }
        }

        [NotNull]
        [ItemNotNull]
        protected virtual IEnumerable<MigrationOperation> DiffRanges([CanBeNull] IModel source, [CanBeNull] IModel target)
        {
            var sourceRanges = source?.Npgsql().PostgresRanges;
            var targetRanges = target?.Npgsql().PostgresRanges;

            if (source == null && targetRanges != null)
            {
                if (targetRanges.Count == 0)
                    yield break;

                var alterDatabaseOperation = new AlterDatabaseOperation();

                foreach (var postgresRange in targetRanges)
                {
                    alterDatabaseOperation.Npgsql().GetOrAddPostgresRange(
                        postgresRange.Schema,
                        postgresRange.Name,
                        postgresRange.Subtype,
                        postgresRange.CanonicalFunction,
                        postgresRange.SubtypeOpClass,
                        postgresRange.Collation,
                        postgresRange.SubtypeDiff);
                }

                yield return alterDatabaseOperation;
                yield break;
            }

            if (source != null && target == null)
            {
                if (sourceRanges.Count == 0)
                    yield break;

                var alterDatabaseOperation = new AlterDatabaseOperation();

                foreach (var postgresRange in sourceRanges)
                {
                    alterDatabaseOperation.Npgsql().GetOrAddOldPostgresRange(
                        postgresRange.Schema,
                        postgresRange.Name,
                        postgresRange.Subtype,
                        postgresRange.CanonicalFunction,
                        postgresRange.SubtypeOpClass,
                        postgresRange.Collation,
                        postgresRange.SubtypeDiff);
                }

                yield return alterDatabaseOperation;
                yield break;
            }

            // TODO: annotate for alter operations
//            if (HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
//            {
//                var alterDatabaseOperation = new AlterDatabaseOperation();
//                alterDatabaseOperation.AddAnnotations(targetMigrationsAnnotations);
//                alterDatabaseOperation.OldDatabase.AddAnnotations(sourceMigrationsAnnotations);
//                yield return alterDatabaseOperation;
//            }
        }
    }
}
