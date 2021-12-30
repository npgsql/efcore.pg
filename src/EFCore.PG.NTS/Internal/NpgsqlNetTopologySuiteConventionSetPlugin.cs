// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

public class NpgsqlNetTopologySuiteConventionSetPlugin : IConventionSetPlugin
{
    public virtual ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.ModelFinalizingConventions.Add(new NpgsqlNetTopologySuiteExtensionAddingConvention());

        return conventionSet;
    }
}

