// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

public class NpgsqlNetTopologySuiteConventionSetPlugin : IConventionSetPlugin
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly IDbContextOptions _options;

    public NpgsqlNetTopologySuiteConventionSetPlugin(IRelationalTypeMappingSource typeMappingSource, IDbContextOptions options)
    {
        _typeMappingSource = typeMappingSource;
        _options = options;
    }

    public virtual ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.ModelFinalizingConventions.Add(new NpgsqlNetTopologySuiteExtensionDiscoveryConvention(_typeMappingSource));

        return conventionSet;
    }
}

