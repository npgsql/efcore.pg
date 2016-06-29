// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Resources;
using Microsoft.EntityFrameworkCore.Infrastructure;

[assembly: AssemblyTitle("Npgsql.EntityFrameworkCore.PostgreSQL")]
[assembly: AssemblyDescription("PostgreSQL provider for Entity Framework Core")]
[assembly: DesignTimeProviderServices(
    typeName: "Microsoft.EntityFrameworkCore.Scaffolding.Internal.NpgsqlDesignTimeServices",
    assemblyName: "Npgsql.EntityFrameworkCore.PostgreSQL.Design",
    packageName: "Npgsql.EntityFrameworkCore.PostgreSQL.Design")]
