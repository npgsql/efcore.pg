// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.InheritanceRelationships;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class InheritanceRelationshipsQueryNpgsqlFixture : InheritanceRelationshipsQueryRelationalFixture<NpgsqlTestStore>
    {
        public static readonly string DatabaseName = "InheritanceRelationships";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = NpgsqlTestStore.CreateConnectionString(DatabaseName);

        public InheritanceRelationshipsQueryNpgsqlFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddSingleton(TestNpgsqlModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override NpgsqlTestStore CreateTestStore()
        {
            return NpgsqlTestStore.GetOrCreateShared(DatabaseName, () =>
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseNpgsql(_connectionString)
                    .UseInternalServiceProvider(_serviceProvider);

                using (var context = new InheritanceRelationshipsContext(optionsBuilder.Options))
                {
                    // TODO: Delete DB if model changed
                    context.Database.EnsureDeleted();
                    if (context.Database.EnsureCreated())
                    {
                        InheritanceRelationshipsModelInitializer.Seed(context);
                    }

                    TestSqlLoggerFactory.SqlStatements.Clear();
                }
            });
        }

        public override InheritanceRelationshipsContext CreateContext(NpgsqlTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseNpgsql(testStore.Connection)
                .UseInternalServiceProvider(_serviceProvider);

            var context = new InheritanceRelationshipsContext(optionsBuilder.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
