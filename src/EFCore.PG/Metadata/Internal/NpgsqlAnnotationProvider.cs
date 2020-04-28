using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

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

            if (column.PropertyMappings.Select(m => m.Property.GetTsVectorConfig())
                .FirstOrDefault(c => c != null) is string tsVectorConfig)
            {
                yield return new Annotation(NpgsqlAnnotationNames.TsVectorConfig, tsVectorConfig);
            }

            property = column.PropertyMappings.Select(m => m.Property)
                .FirstOrDefault(p => p.GetTsVectorProperties() != null);
            if (property != null)
            {
                yield return new Annotation(
                    NpgsqlAnnotationNames.TsVectorProperties,
                    property.GetTsVectorProperties()
                        .Select(p2 => property.DeclaringEntityType.FindProperty(p2).GetColumnName())
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
                yield return new Annotation(
                    NpgsqlAnnotationNames.IndexInclude,
                    includeProperties
                        .Select(p => modelIndex.DeclaringEntityType.FindProperty(p).GetColumnName())
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
