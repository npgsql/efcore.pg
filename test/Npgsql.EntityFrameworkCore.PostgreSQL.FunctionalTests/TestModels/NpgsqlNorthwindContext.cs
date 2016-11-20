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

        public NpgsqlNorthwindContext(DbContextOptions options,
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            : base(options, queryTrackingBehavior)
        {
        }

        public static NpgsqlTestStore GetSharedStore()
            => NpgsqlTestStore.GetOrCreateShared(
                DatabaseName,
                () => NpgsqlTestStore.ExecuteScript(DatabaseName, @"Northwind.sql"),
                cleanDatabase: false);
    }
}
