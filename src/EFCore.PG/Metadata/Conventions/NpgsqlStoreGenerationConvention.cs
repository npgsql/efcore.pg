using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

/// <summary>
/// A convention that ensures that properties aren't configured to have a default value, as computed column
/// or using a <see cref="NpgsqlValueGenerationStrategy"/> at the same time.
/// </summary>
public class NpgsqlStoreGenerationConvention : StoreGenerationConvention
{
    /// <summary>
    /// Creates a new instance of <see cref="NpgsqlStoreGenerationConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
    public NpgsqlStoreGenerationConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <summary>
    /// Called after an annotation is changed on a property.
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
        if (annotation is null
            || oldAnnotation?.Value is not null)
        {
            return;
        }

        var configurationSource = annotation.GetConfigurationSource();
        var fromDataAnnotation = configurationSource != ConfigurationSource.Convention;
        switch (name)
        {
            case RelationalAnnotationNames.DefaultValue:
                if (propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) is null
                    && propertyBuilder.HasDefaultValue(null, fromDataAnnotation) is not null)
                {
                    context.StopProcessing();
                    return;
                }

                break;
            case RelationalAnnotationNames.DefaultValueSql:
                if (propertyBuilder.Metadata.GetValueGenerationStrategy() != NpgsqlValueGenerationStrategy.Sequence
                    && propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) == null
                    && propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) != null)
                {
                    context.StopProcessing();
                    return;
                }

                break;
            case RelationalAnnotationNames.ComputedColumnSql:
                if (propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) is null
                    && propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) is not null)
                {
                    context.StopProcessing();
                    return;
                }

                break;
            case NpgsqlAnnotationNames.ValueGenerationStrategy:
                if (((propertyBuilder.Metadata.GetValueGenerationStrategy() != NpgsqlValueGenerationStrategy.Sequence
                            && (propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                                || propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null
                                || propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null))
                        || (propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                            || propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null))
                    && propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) != null)
                {
                    context.StopProcessing();
                    return;
                }

                break;
        }

        base.ProcessPropertyAnnotationChanged(propertyBuilder, name, annotation, oldAnnotation, context);
    }

    /// <inheritdoc />
    protected override void Validate(IConventionProperty property, in StoreObjectIdentifier storeObject)
    {
        if (property.GetValueGenerationStrategyConfigurationSource() is not null)
        {
            var generationStrategy = property.GetValueGenerationStrategy(storeObject);
            if (generationStrategy == NpgsqlValueGenerationStrategy.None)
            {
                base.Validate(property, storeObject);
                return;
            }

            if (property.TryGetDefaultValue(storeObject, out _))
            {
                throw new InvalidOperationException(
                    RelationalStrings.ConflictingColumnServerGeneration(
                        "NpgsqlValueGenerationStrategy", property.Name, "DefaultValue"));
            }

            if(property.GetDefaultValueSql() is not null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ConflictingColumnServerGeneration(
                        "NpgsqlValueGenerationStrategy", property.Name, "DefaultValueSql"));
            }

            if (property.GetComputedColumnSql() is not null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ConflictingColumnServerGeneration(
                        "NpgsqlValueGenerationStrategy", property.Name, "ComputedColumnSql"));
            }
        }

        base.Validate(property, storeObject);
    }
}
