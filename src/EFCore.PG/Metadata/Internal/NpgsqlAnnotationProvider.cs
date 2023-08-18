using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlAnnotationProvider : RelationalAnnotationProvider
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlAnnotationProvider(RelationalAnnotationProviderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<IAnnotation> For(ITable table, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        // Model validation ensures that these facets are the same on all mapped entity types
        var entityType = (IEntityType)table.EntityTypeMappings.First().TypeBase;

        if (entityType.GetIsUnlogged())
        {
            yield return new Annotation(NpgsqlAnnotationNames.UnloggedTable, entityType.GetIsUnlogged());
        }

        if (entityType[CockroachDbAnnotationNames.InterleaveInParent] is not null)
        {
            yield return new Annotation(CockroachDbAnnotationNames.InterleaveInParent, entityType[CockroachDbAnnotationNames.InterleaveInParent]);
        }

        foreach (var storageParamAnnotation in entityType.GetAnnotations()
                     .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
        {
            yield return storageParamAnnotation;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<IAnnotation> For(IColumn column, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        var table = StoreObjectIdentifier.Table(column.Table.Name, column.Table.Schema);
        var valueGeneratedProperty = column.PropertyMappings.Where(
                m => (m.TableMapping.IsSharedTablePrincipal ?? true)
                    && m.TableMapping.TypeBase == m.Property.DeclaringType)
            .Select(m => m.Property)
            .FirstOrDefault(
                p => p.GetValueGenerationStrategy(table) switch
                {
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn => true,
                    NpgsqlValueGenerationStrategy.IdentityAlwaysColumn    => true,
                    NpgsqlValueGenerationStrategy.SerialColumn            => true,
                    _                                                     => false
                });

        if (valueGeneratedProperty is not null)
        {
            var valueGenerationStrategy = valueGeneratedProperty.GetValueGenerationStrategy();
            yield return new Annotation(NpgsqlAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy);

            if (valueGenerationStrategy is NpgsqlValueGenerationStrategy.IdentityByDefaultColumn or NpgsqlValueGenerationStrategy.IdentityAlwaysColumn)
            {
                if (valueGeneratedProperty[NpgsqlAnnotationNames.IdentityOptions] is string identityOptions)
                {
                    yield return new Annotation(NpgsqlAnnotationNames.IdentityOptions, identityOptions);
                }
            }
        }

        // If the property has a collation explicitly defined on it via the standard EF mechanism, it will get
        // passed on the Collation property (we don't need to do anything).
        // Otherwise, if a model-wide default column collation exists, pass that through our custom annotation.
        // Note that this mechanism is obsolete, and EF Core's bulk model configuration can be used instead; but we continue to support
        // it for backwards compat.
#pragma warning disable CS0618
        if (column.PropertyMappings.All(m => m.Property.GetCollation() is null) &&
            column.PropertyMappings.Select(m => m.Property.GetDefaultCollation())
                .FirstOrDefault(c => c is not null) is { } defaultColumnCollation)
        {
            yield return new Annotation(NpgsqlAnnotationNames.DefaultColumnCollation, defaultColumnCollation);
        }
#pragma warning restore CS0618

        if (column.PropertyMappings.Select(m => m.Property.GetTsVectorConfig())
                .FirstOrDefault(c => c is not null) is { } tsVectorConfig)
        {
            yield return new Annotation(NpgsqlAnnotationNames.TsVectorConfig, tsVectorConfig);
        }

        valueGeneratedProperty = column.PropertyMappings.Select(m => m.Property)
            .FirstOrDefault(p => p.GetTsVectorProperties() is not null);
        if (valueGeneratedProperty is not null)
        {
            var tableIdentifier = StoreObjectIdentifier.Table(column.Table.Name, column.Table.Schema);

            yield return new Annotation(
                NpgsqlAnnotationNames.TsVectorProperties,
                valueGeneratedProperty.GetTsVectorProperties()!
                    .Select(p2 => valueGeneratedProperty.DeclaringType.FindProperty(p2)!.GetColumnName(tableIdentifier))
                    .ToArray());
        }

        // JSON columns have no property mappings so all annotations that rely on property mappings should be skipped for them
        if (column is not JsonColumn
            && column.PropertyMappings.FirstOrDefault()?.Property.GetCompressionMethod() is { } compressionMethod)
        {
            // Model validation ensures that these facets are the same on all mapped properties
            yield return new Annotation(NpgsqlAnnotationNames.CompressionMethod, compressionMethod);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<IAnnotation> For(ITableIndex index, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        // Model validation ensures that these facets are the same on all mapped indexes
        var modelIndex = index.MappedIndexes.First();

        if (modelIndex.GetCollation() is { } collation)
        {
            yield return new Annotation(RelationalAnnotationNames.Collation, collation);
        }

        if (modelIndex.GetMethod() is { } method)
        {
            yield return new Annotation(NpgsqlAnnotationNames.IndexMethod, method);
        }

        if (modelIndex.GetOperators() is { } operators)
        {
            yield return new Annotation(NpgsqlAnnotationNames.IndexOperators, operators);
        }

        if (modelIndex.GetNullSortOrder() is { } nullSortOrder)
        {
            yield return new Annotation(NpgsqlAnnotationNames.IndexNullSortOrder, nullSortOrder);
        }

        if (modelIndex.GetTsVectorConfig() is { } configName)
        {
            yield return new Annotation(NpgsqlAnnotationNames.TsVectorConfig, configName);
        }

        if (modelIndex.GetIncludeProperties() is { } includeProperties)
        {
            var tableIdentifier = StoreObjectIdentifier.Table(index.Table.Name, index.Table.Schema);

            yield return new Annotation(
                NpgsqlAnnotationNames.IndexInclude,
                includeProperties
                    .Select(p => modelIndex.DeclaringEntityType.FindProperty(p)!.GetColumnName(tableIdentifier))
                    .ToArray());
        }

        if (modelIndex.IsCreatedConcurrently() is { } isCreatedConcurrently)
        {
            yield return new Annotation(NpgsqlAnnotationNames.CreatedConcurrently, isCreatedConcurrently);
        }

        if (modelIndex.GetAreNullsDistinct() is { } nullsDistinct)
        {
            yield return new Annotation(NpgsqlAnnotationNames.NullsDistinct, nullsDistinct);
        }

        // Support legacy annotation for index ordering
        if (modelIndex[NpgsqlAnnotationNames.IndexSortOrder] is IReadOnlyList<SortOrder> legacySortOrder)
        {
            yield return new Annotation(NpgsqlAnnotationNames.IndexSortOrder, legacySortOrder);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<IAnnotation> For(IRelationalModel model, bool designTime)
    {
        if (!designTime)
        {
            return Array.Empty<IAnnotation>();
        }

        return model.Model.GetAnnotations().Where(
            a =>
                a.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal)
                || a.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal)
                || a.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal)
                || a.Name.StartsWith(NpgsqlAnnotationNames.CollationDefinitionPrefix, StringComparison.Ordinal));
    }
}
