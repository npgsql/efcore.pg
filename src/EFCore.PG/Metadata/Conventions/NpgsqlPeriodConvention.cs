using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

/// <summary>
///     A convention that sets the delete behavior to <see cref="DeleteBehavior.NoAction" /> for foreign keys with PERIOD,
///     since PostgreSQL does not support cascading deletes for temporal foreign keys.
/// </summary>
/// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
/// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
public class NpgsqlPeriodConvention(
    ProviderConventionSetBuilderDependencies dependencies,
    RelationalConventionSetBuilderDependencies relationalDependencies)
    : IForeignKeyAnnotationChangedConvention
{
    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; } = dependencies;

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; } = relationalDependencies;

    /// <inheritdoc />
    public virtual void ProcessForeignKeyAnnotationChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == NpgsqlAnnotationNames.Period && annotation?.Value is true)
        {
            relationshipBuilder.OnDelete(DeleteBehavior.NoAction);
        }
    }
}
