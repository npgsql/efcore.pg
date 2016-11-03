// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Resources;
using Microsoft.EntityFrameworkCore.Infrastructure;

[assembly: AssemblyTitle("Npgsql.EntityFrameworkCore.PostgreSQL")]
[assembly: AssemblyDescription("PostgreSQL provider for Entity Framework Core")]
[assembly: DesignTimeProviderServices(
    typeName: "Microsoft.EntityFrameworkCore.Scaffolding.Internal.NpgsqlDesignTimeServices",
    assemblyName: "Npgsql.EntityFrameworkCore.PostgreSQL.Design, Version=1.2.0.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7",
    packageName: "Npgsql.EntityFrameworkCore.PostgreSQL.Design")]
