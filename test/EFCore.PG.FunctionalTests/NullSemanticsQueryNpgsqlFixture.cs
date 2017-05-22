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

        private readonly DbContextOptions _options;

        private readonly string _connectionString = NpgsqlTestStore.CreateConnectionString(DatabaseName);

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public NullSemanticsQueryNpgsqlFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public override NpgsqlTestStore CreateTestStore()
        {
            return NpgsqlTestStore.GetOrCreateShared(DatabaseName, () =>
            {
                using (var context = new NullSemanticsContext(new DbContextOptionsBuilder(_options)
                    .UseNpgsql(_connectionString, b => b.ApplyConfiguration()).Options))
                {
                    context.Database.EnsureCreated();
                    NullSemanticsModelInitializer.Seed(context);
                }
            });
        }

        public override NullSemanticsContext CreateContext(NpgsqlTestStore testStore, bool useRelationalNulls)
        {
            var options = new DbContextOptionsBuilder(_options)
                .UseNpgsql(
                    testStore.Connection,
                    b =>
                    {
                        b.ApplyConfiguration();
                        if (useRelationalNulls)
                        {
                            b.UseRelationalNulls();
                        }
                    }).Options;

            var context = new NullSemanticsContext(options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
