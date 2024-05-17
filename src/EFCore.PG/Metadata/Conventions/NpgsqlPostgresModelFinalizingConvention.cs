using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

/// <summary>
///     A convention that discovers certain common PostgreSQL extensions based on store types used in the model (e.g. hstore).
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>.
/// </remarks>
public class NpgsqlPostgresModelFinalizingConvention : IModelFinalizingConvention
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly IReadOnlyList<EnumDefinition> _enumDefinitions;

    /// <summary>
    ///     Creates a new instance of <see cref="NpgsqlPostgresModelFinalizingConvention" />.
    /// </summary>
    /// <param name="typeMappingSource">The type mapping source to use.</param>
    /// <param name="enumDefinitions"></param>
    public NpgsqlPostgresModelFinalizingConvention(
        IRelationalTypeMappingSource typeMappingSource,
        IReadOnlyList<EnumDefinition> enumDefinitions)
    {
        _typeMappingSource = typeMappingSource;
        _enumDefinitions = enumDefinitions;
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                var typeMapping = (RelationalTypeMapping?)property.FindTypeMapping()
                    ?? _typeMappingSource.FindMapping((IProperty)property);

                if (typeMapping is not null)
                {
                    DiscoverPostgresExtensions(property, typeMapping, modelBuilder);
                    ProcessRowVersionProperty(property, typeMapping);
                }
            }
        }

        SetupEnums(modelBuilder);
    }

    /// <summary>
    ///     Configures the model to create PostgreSQL enums based on the user's enum definitions in the context options.
    /// </summary>
    protected virtual void SetupEnums(IConventionModelBuilder modelBuilder)
    {
        foreach (var enumDefinition in _enumDefinitions)
        {
            modelBuilder.HasPostgresEnum(
                enumDefinition.StoreTypeSchema,
                enumDefinition.StoreTypeName,
                enumDefinition.Labels.Values.Order(StringComparer.Ordinal).ToArray());
        }
    }

    /// <summary>
    ///     Discovers certain common PostgreSQL extensions based on property store types (e.g. hstore).
    /// </summary>
    protected virtual void DiscoverPostgresExtensions(
        IConventionProperty property,
        RelationalTypeMapping typeMapping,
        IConventionModelBuilder modelBuilder)
    {
        // TODO: does not work if CREATE EXTENSION was done on a non-default schema. #3177
        switch (typeMapping.StoreType)
        {
            case "hstore":
                modelBuilder.HasPostgresExtension("hstore");
                break;
            case "citext":
                modelBuilder.HasPostgresExtension("citext");
                break;
            case "ltree":
            case "lquery":
            case "ltxtquery":
                modelBuilder.HasPostgresExtension("ltree");
                break;
        }
    }

    /// <summary>
    ///     Detects properties which are uint, OnAddOrUpdate and configured as concurrency tokens, and maps these to the PostgreSQL
    ///     internal "xmin" column, which changes every time the row is modified.
    /// </summary>
    protected virtual void ProcessRowVersionProperty(IConventionProperty property, RelationalTypeMapping typeMapping)
    {
        if (property is { ValueGenerated: ValueGenerated.OnAddOrUpdate, IsConcurrencyToken: true }
            && typeMapping.StoreType == "xid")
        {
            property.Builder.HasColumnName("xmin");
        }
    }
}
