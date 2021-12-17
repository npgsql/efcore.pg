// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

public class NpgsqlPostgresExtensionDiscoveryConvention : IModelFinalizingConvention
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    public NpgsqlPostgresExtensionDiscoveryConvention(IRelationalTypeMappingSource typeMappingSource)
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

                switch (typeMapping.StoreType)
                {
                    case "hstore":
                        modelBuilder.HasPostgresExtension("hstore");
                        continue;
                    case "citext":
                        modelBuilder.HasPostgresExtension("citext");
                        continue;
                    case "ltree":
                    case "lquery":
                    case "ltxtquery":
                        modelBuilder.HasPostgresExtension("ltree");
                        continue;
                }
            }
        }
    }
}
