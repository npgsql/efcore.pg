// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

public class NpgsqlNetTopologySuiteExtensionAddingConvention : IModelFinalizingConvention
{
    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        => modelBuilder.HasPostgresExtension("postgis");
}
