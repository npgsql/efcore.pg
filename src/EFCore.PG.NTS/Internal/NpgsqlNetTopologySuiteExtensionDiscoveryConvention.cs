// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

public class NpgsqlNetTopologySuiteExtensionDiscoveryConvention : IModelFinalizingConvention
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    public NpgsqlNetTopologySuiteExtensionDiscoveryConvention(IRelationalTypeMappingSource typeMappingSource)
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
                var typeMapping = (RelationalTypeMapping?)property.FindTypeMapping()
                    ?? _typeMappingSource.FindMapping((IProperty)property);

                if (typeMapping is null)
                {
                    continue;
                }

                switch (typeMapping.StoreTypeNameBase)
                {
                    case "geometry":
                    case "geography":
                        modelBuilder.HasPostgresExtension("postgis");
                        continue;
                }
            }
        }
    }
}
