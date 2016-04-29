// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.NullSemanticsModel;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class NullSemanticsQueryNpgsqlFixture : NullSemanticsQueryRelationalFixture<NpgsqlTestStore>
    {
        public static readonly string DatabaseName = "NullSemanticsQueryTest";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = NpgsqlTestStore.CreateConnectionString(DatabaseName);

        public NullSemanticsQueryNpgsqlFixture()
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
                using (var context = new NullSemanticsContext(new DbContextOptionsBuilder()
                    .UseNpgsql(_connectionString)
                    .UseInternalServiceProvider(_serviceProvider).Options))
                {
                    // TODO: Delete DB if model changed

                    if (context.Database.EnsureCreated())
                    {
                        NullSemanticsModelInitializer.Seed(context);
                    }

                    TestSqlLoggerFactory.SqlStatements.Clear();
                }
            });
        }

        public override NullSemanticsContext CreateContext(NpgsqlTestStore testStore, bool useRelationalNulls)
        {
            var context = new NullSemanticsContext(new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(_serviceProvider)
                .UseNpgsql(
                    testStore.Connection,
                    b =>
                    {
                        if (useRelationalNulls)
                        {
                            b.UseRelationalNulls();
                        }
                    }).Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
