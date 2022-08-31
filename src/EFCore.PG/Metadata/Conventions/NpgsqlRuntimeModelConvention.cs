using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

/// <summary>
/// A convention that creates an optimized copy of the mutable model.
/// </summary>
public class NpgsqlRuntimeModelConvention : RelationalRuntimeModelConvention
{
    /// <summary>
    /// Creates a new instance of <see cref="NpgsqlRuntimeModelConvention"/>.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
    public NpgsqlRuntimeModelConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <inheritdoc />
    protected override void ProcessModelAnnotations(
        Dictionary<string, object?> annotations,
        IModel model,
        RuntimeModel runtimeModel,
        bool runtime)
    {
        base.ProcessModelAnnotations(annotations, model, runtimeModel, runtime);

        if (!runtime)
        {
            annotations.Remove(NpgsqlAnnotationNames.DatabaseTemplate);
            annotations.Remove(NpgsqlAnnotationNames.Tablespace);
            annotations.Remove(NpgsqlAnnotationNames.CollationDefinitionPrefix);

#pragma warning disable CS0618
            annotations.Remove(NpgsqlAnnotationNames.DefaultColumnCollation);
#pragma warning restore CS0618

            foreach (var annotationName in annotations.Keys.Where(
                         k =>
                             k.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal)
                             || k.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal)
                             || k.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }
    }

    /// <inheritdoc />
    protected override void ProcessEntityTypeAnnotations(
        Dictionary<string, object?> annotations,
        IEntityType entityType,
        RuntimeEntityType runtimeEntityType,
        bool runtime)
    {
        base.ProcessEntityTypeAnnotations(annotations, entityType, runtimeEntityType, runtime);

        if (!runtime)
        {
            annotations.Remove(NpgsqlAnnotationNames.UnloggedTable);

            foreach (var annotationName in annotations.Keys.Where(
                         k => k.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAnnotations(
        Dictionary<string, object?> annotations, IProperty property, RuntimeProperty runtimeProperty, bool runtime)
    {
        base.ProcessPropertyAnnotations(annotations, property, runtimeProperty, runtime);

        if (!runtime)
        {
            annotations.Remove(NpgsqlAnnotationNames.IdentityOptions);
            annotations.Remove(NpgsqlAnnotationNames.TsVectorConfig);
            annotations.Remove(NpgsqlAnnotationNames.TsVectorProperties);

            if (!annotations.ContainsKey(NpgsqlAnnotationNames.ValueGenerationStrategy))
            {
                annotations[NpgsqlAnnotationNames.ValueGenerationStrategy] = property.GetValueGenerationStrategy();
            }
        }
    }

    /// <inheritdoc/>
    protected override void ProcessIndexAnnotations(
        Dictionary<string, object?> annotations,
        IIndex index,
        RuntimeIndex runtimeIndex,
        bool runtime)
    {
        base.ProcessIndexAnnotations(annotations, index, runtimeIndex, runtime);

        if (!runtime)
        {
            annotations.Remove(NpgsqlAnnotationNames.IndexMethod);
            annotations.Remove(NpgsqlAnnotationNames.IndexOperators);
            annotations.Remove(NpgsqlAnnotationNames.IndexSortOrder);
            annotations.Remove(NpgsqlAnnotationNames.IndexNullSortOrder);
            annotations.Remove(NpgsqlAnnotationNames.IndexInclude);
            annotations.Remove(NpgsqlAnnotationNames.CreatedConcurrently);
        }
    }
}
