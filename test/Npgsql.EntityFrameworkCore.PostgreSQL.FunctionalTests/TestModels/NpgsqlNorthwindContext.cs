// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.TestModels
{
    public class NpgsqlNorthwindContext : NorthwindContext
    {
        public static readonly string DatabaseName = StoreName;
        public static readonly string ConnectionString = NpgsqlTestStore.CreateConnectionString(DatabaseName);

        public NpgsqlNorthwindContext(DbContextOptions options)
            : base(options)
        {
        }
        public static NpgsqlTestStore GetSharedStore()
        {
            return NpgsqlTestStore.GetOrCreateShared(
                DatabaseName,
                () => NpgsqlTestStore.CreateDatabase(DatabaseName, scriptPath: "Northwind.sql")); // relative from bin/<config>
        }
    }
}
