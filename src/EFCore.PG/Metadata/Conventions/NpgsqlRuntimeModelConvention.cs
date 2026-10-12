using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.ChangeTracking.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

/// <summary>
///     A convention that creates an optimized copy of the mutable model.
/// </summary>
/// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
/// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
public class NpgsqlRuntimeModelConvention(
    ProviderConventionSetBuilderDependencies dependencies,
    RelationalConventionSetBuilderDependencies relationalDependencies)
    : RelationalRuntimeModelConvention(dependencies, relationalDependencies)
{
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
            annotations.Remove(NpgsqlAnnotationNames.Encoding);

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
        Dictionary<string, object?> annotations,
        IProperty property,
        RuntimeProperty runtimeProperty,
        bool runtime)
    {
        base.ProcessPropertyAnnotations(annotations, property, runtimeProperty, runtime);

        // NpgsqlRange<T> doesn't implement IComparable (ranges are only partially ordered), so we must
        // provide a custom CurrentValueComparer for the runtime model. Without this, the update pipeline's
        // ModificationCommandComparer would fail when trying to sort commands by key values.
        if ((property.IsKey() || property.IsForeignKey())
            && property.FindTypeMapping() is NpgsqlRangeTypeMapping)
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            runtimeProperty.SetCurrentValueComparer(
                new EntryCurrentValueComparer(runtimeProperty, new NpgsqlRangeCurrentValueComparer(property.ClrType)));
#pragma warning restore EF1001 // Internal EF Core API usage.
        }

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

    /// <inheritdoc />
    protected override void ProcessIndexAnnotations(
        Dictionary<string, object?> annotations,
        IIndex index,
        RuntimeIndex runtimeIndex,
        bool runtime)
    {
        base.ProcessIndexAnnotations(annotations, index, runtimeIndex, runtime);

        if (!runtime)
        {
            annotations.Remove(NpgsqlAnnotationNames.IndexOperators);
            annotations.Remove(NpgsqlAnnotationNames.IndexSortOrder);
            annotations.Remove(NpgsqlAnnotationNames.IndexNullSortOrder);
            annotations.Remove(NpgsqlAnnotationNames.IndexInclude);
            annotations.Remove(NpgsqlAnnotationNames.CreatedConcurrently);
            annotations.Remove(NpgsqlAnnotationNames.NullsDistinct);

            foreach (var annotationName in annotations.Keys.Where(
                         k => k.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }
    }
}
