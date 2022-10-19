// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

    /// <summary>
    ///     Creates a new instance of <see cref="NpgsqlPostgresModelFinalizingConvention" />.
    /// </summary>
    /// <param name="typeMappingSource">The type mapping source to use.</param>
    public NpgsqlPostgresModelFinalizingConvention(IRelationalTypeMappingSource typeMappingSource)
    {
        _typeMappingSource = typeMappingSource;
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                DiscoverPostgresExtensions(property, modelBuilder);
                ProcessRowVersionProperty(property);
            }
        }
    }

    /// <summary>
    ///     Discovers certain common PostgreSQL extensions based on property store types (e.g. hstore).
    /// </summary>
    /// <param name="property"></param>
    /// <param name="modelBuilder"></param>
    protected virtual void DiscoverPostgresExtensions(IConventionProperty property, IConventionModelBuilder modelBuilder)
    {
        var typeMapping = (RelationalTypeMapping?)property.FindTypeMapping()
            ?? _typeMappingSource.FindMapping((IProperty)property);

        if (typeMapping is not null)
        {
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
    }

    /// <summary>
    ///     Detects properties which are uint, OnAddOrUpdate and configured as concurrency tokens, and maps these to the PostgreSQL
    ///     internal "xmin" column, which changes every time the row is modified.
    /// </summary>
    protected virtual void ProcessRowVersionProperty(IConventionProperty property)
    {
        if (property.ValueGenerated == ValueGenerated.OnAddOrUpdate
            && property.IsConcurrencyToken
            && property.ClrType == typeof(uint)
            && property.GetValueConverter() is null)
        {
            property.Builder.HasColumnName("xmin")?.HasColumnType("xid");
        }
    }
}
