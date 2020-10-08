using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using RelationalPropertyExtensions = Microsoft.EntityFrameworkCore.RelationalPropertyExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal
{
    public class NpgsqlAnnotationProvider : RelationalAnnotationProvider
    {
        public NpgsqlAnnotationProvider([NotNull] RelationalAnnotationProviderDependencies dependencies)
            : base(dependencies)
        {
        }

        public override IEnumerable<IAnnotation> For(ITable table)
        {
            // Model validation ensures that these facets are the same on all mapped entity types
            var entityType = table.EntityTypeMappings.First().EntityType;

            if (entityType.GetIsUnlogged())
                yield return new Annotation(NpgsqlAnnotationNames.UnloggedTable, entityType.GetIsUnlogged());
            if (entityType[CockroachDbAnnotationNames.InterleaveInParent] != null)
                yield return new Annotation(CockroachDbAnnotationNames.InterleaveInParent, entityType[CockroachDbAnnotationNames.InterleaveInParent]);
            foreach (var storageParamAnnotation in entityType.GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix)))
            {
                yield return storageParamAnnotation;
            }
        }

        public override IEnumerable<IAnnotation> For(IColumn column)
        {
            var property = column.PropertyMappings.Select(m => m.Property)
                .FirstOrDefault(p => p.GetValueGenerationStrategy() != NpgsqlValueGenerationStrategy.None);
            if (property != null)
            {
                var valueGenerationStrategy = property.GetValueGenerationStrategy();
                yield return new Annotation(NpgsqlAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy);

                if (valueGenerationStrategy == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn ||
                    valueGenerationStrategy == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn)
                {
                    if (property[NpgsqlAnnotationNames.IdentityOptions] is string identityOptions)
                    {
                        yield return new Annotation(NpgsqlAnnotationNames.IdentityOptions, identityOptions);
                    }
                }
            }

            // If the property has a collation explicitly defined on it via the standard EF mechanism, it will get
            // passed on the Collation property (we don't need to do anything).
            // Otherwise, a model-wide default column collation exists, pass that through our custom annotation.
            if (column.PropertyMappings.All(m => RelationalPropertyExtensions.GetCollation(m.Property) == null) &&
                column.PropertyMappings.Select(m => m.Property.GetDefaultCollation())
                    .FirstOrDefault(c => c != null) is string defaultColumnCollation)
            {
                yield return new Annotation(NpgsqlAnnotationNames.DefaultColumnCollation, defaultColumnCollation);
            }

            if (column.PropertyMappings.Select(m => m.Property.GetTsVectorConfig())
                .FirstOrDefault(c => c != null) is string tsVectorConfig)
            {
                yield return new Annotation(NpgsqlAnnotationNames.TsVectorConfig, tsVectorConfig);
            }

            property = column.PropertyMappings.Select(m => m.Property)
                .FirstOrDefault(p => p.GetTsVectorProperties() != null);
            if (property != null)
            {
                var tableIdentifier = StoreObjectIdentifier.Table(column.Table.Name, column.Table.Schema);

                yield return new Annotation(
                    NpgsqlAnnotationNames.TsVectorProperties,
                    property.GetTsVectorProperties()
                        .Select(p2 => property.DeclaringEntityType.FindProperty(p2).GetColumnName(tableIdentifier))
                        .ToArray());
            }
        }

        public override IEnumerable<IAnnotation> For(ITableIndex index)
        {
            // Model validation ensures that these facets are the same on all mapped indexes
            var modelIndex = index.MappedIndexes.First();

            if (modelIndex.GetCollation() is IReadOnlyList<string> collation)
                yield return new Annotation(RelationalAnnotationNames.Collation, collation);

            if (modelIndex.GetMethod() is string method)
                yield return new Annotation(NpgsqlAnnotationNames.IndexMethod, method);
            if (modelIndex.GetOperators() is IReadOnlyList<string> operators)
                yield return new Annotation(NpgsqlAnnotationNames.IndexOperators, operators);
            if (modelIndex.GetSortOrder() is IReadOnlyList<SortOrder> sortOrder)
                yield return new Annotation(NpgsqlAnnotationNames.IndexSortOrder, sortOrder);
            if (modelIndex.GetNullSortOrder() is IReadOnlyList<SortOrder> nullSortOrder)
                yield return new Annotation(NpgsqlAnnotationNames.IndexNullSortOrder, nullSortOrder);
            if (modelIndex.GetTsVectorConfig() is string configName)
                yield return new Annotation(NpgsqlAnnotationNames.TsVectorConfig, configName);
            if (modelIndex.GetIncludeProperties() is IReadOnlyList<string> includeProperties)
            {
                var tableIdentifier = StoreObjectIdentifier.Table(index.Table.Name, index.Table.Schema);

                yield return new Annotation(
                    NpgsqlAnnotationNames.IndexInclude,
                    includeProperties
                        .Select(p => modelIndex.DeclaringEntityType.FindProperty(p).GetColumnName(tableIdentifier))
                        .ToArray());
            }

            var isCreatedConcurrently = modelIndex.IsCreatedConcurrently();
            if (isCreatedConcurrently.HasValue)
            {
                yield return new Annotation(
                    NpgsqlAnnotationNames.CreatedConcurrently,
                    isCreatedConcurrently.Value);
            }
        }

        public override IEnumerable<IAnnotation> For(IRelationalModel model)
            => model.Model.GetAnnotations().Where(a =>
                a.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal) ||
                a.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal) ||
                a.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal) ||
                a.Name.StartsWith(NpgsqlAnnotationNames.CollationDefinitionPrefix, StringComparison.Ordinal));
    }
}
