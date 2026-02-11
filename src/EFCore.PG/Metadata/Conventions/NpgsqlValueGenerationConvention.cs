using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

/// <summary>
///     A convention that configures store value generation as <see cref="ValueGenerated.OnAdd" /> on properties that are
///     part of the primary key and not part of any foreign keys, were configured to have a database default value
///     or were configured to use a <see cref="NpgsqlValueGenerationStrategy" />.
///     It also configures properties as <see cref="ValueGenerated.OnAddOrUpdate" /> if they were configured as computed columns.
/// </summary>
public class NpgsqlValueGenerationConvention(
    ProviderConventionSetBuilderDependencies dependencies,
    RelationalConventionSetBuilderDependencies relationalDependencies)
    : RelationalValueGenerationConvention(dependencies, relationalDependencies)
{

    /// <summary>
    ///     Called after an annotation is changed on a property.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public override void ProcessPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == NpgsqlAnnotationNames.ValueGenerationStrategy)
        {
            propertyBuilder.ValueGenerated(GetValueGenerated(propertyBuilder.Metadata));
            return;
        }

        if (name == NpgsqlAnnotationNames.TsVectorConfig && propertyBuilder.Metadata.GetTsVectorConfig() is not null)
        {
            propertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate);
            return;
        }

        base.ProcessPropertyAnnotationChanged(propertyBuilder, name, annotation, oldAnnotation, context);
    }

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    protected override ValueGenerated? GetValueGenerated(IConventionProperty property)
    {
        // TODO: move to relational?
        if (property.DeclaringType.IsMappedToJson()
#pragma warning disable EF1001 // Internal EF Core API usage.
            && property.IsOrdinalKeyProperty()
#pragma warning restore EF1001 // Internal EF Core API usage.
            && (property.DeclaringType as IReadOnlyEntityType)?.FindOwnership()!.IsUnique == false)
        {
            return ValueGenerated.OnAdd;
        }

        var declaringTable = property.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
        if (declaringTable.Name == null)
        {
            return null;
        }

        // If the first mapping can be value generated then we'll consider all mappings to be value generated
        // as this is a client-side configuration and can't be specified per-table.
        return GetValueGenerated(property, declaringTable, Dependencies.TypeMappingSource);
    }

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property"> The property. </param>
    /// <param name="storeObject"> The identifier of the store object. </param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    public static new ValueGenerated? GetValueGenerated(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        => ShouldSuppressValueGeneration(property, storeObject)
            ? GetNonStrategyValueGenerated(property, storeObject)
            : RelationalValueGenerationConvention.GetValueGenerated(property, storeObject)
            ?? (property.GetValueGenerationStrategy(storeObject) != NpgsqlValueGenerationStrategy.None
                ? ValueGenerated.OnAdd
                : null);

    private ValueGenerated? GetValueGenerated(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource typeMappingSource)
            => ShouldSuppressValueGeneration(property, storeObject)
                ? GetNonStrategyValueGenerated(property, storeObject)
                : RelationalValueGenerationConvention.GetValueGenerated(property, storeObject)
                ?? (property.GetValueGenerationStrategy(storeObject, typeMappingSource) != NpgsqlValueGenerationStrategy.None
                    ? ValueGenerated.OnAdd
                    : null);

    /// <summary>
    ///     Returns whether <see cref="ValueGenerated.OnAdd" /> should be suppressed for the given property because the
    ///     model has been configured with <see cref="NpgsqlValueGenerationStrategy.None" /> and the property has no
    ///     explicit value generation strategy override.
    /// </summary>
    private static bool ShouldSuppressValueGeneration(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        => property.DeclaringType.Model.GetValueGenerationStrategy() is NpgsqlValueGenerationStrategy.None
            && property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy) is null
            && property.FindOverrides(storeObject)?.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy) is null;

    /// <summary>
    ///     Returns value generation based on non-strategy sources (default values, computed columns) when
    ///     the Npgsql value generation strategy has been suppressed.
    /// </summary>
    private static ValueGenerated? GetNonStrategyValueGenerated(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        => property.GetComputedColumnSql(storeObject) is not null
            ? ValueGenerated.OnAddOrUpdate
            : property.TryGetDefaultValue(storeObject, out _) || property.GetDefaultValueSql(storeObject) is not null
                ? ValueGenerated.OnAdd
                : null;
}
