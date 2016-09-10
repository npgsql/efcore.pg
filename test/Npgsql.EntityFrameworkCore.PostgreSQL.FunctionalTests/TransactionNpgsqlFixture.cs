// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class TransactionNpgsqlFixture : TransactionFixtureBase<NpgsqlTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionNpgsqlFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddSingleton(TestNpgsqlModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();
        }

        public override NpgsqlTestStore CreateTestStore()
        {
            return NpgsqlTestStore.GetOrCreateShared(DatabaseName, false, () =>
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseNpgsql(NpgsqlTestStore.CreateConnectionString(DatabaseName))
                    .UseInternalServiceProvider(_serviceProvider);

                using (var context = new DbContext(optionsBuilder.Options))
                {
                    context.Database.EnsureClean();
                }
            });
        }

        public override DbContext CreateContext(NpgsqlTestStore testStore)
            => new DbContext(new DbContextOptionsBuilder()
                .UseNpgsql(testStore.ConnectionString)
                .UseInternalServiceProvider(_serviceProvider).Options);

        public override DbContext CreateContext(DbConnection connection)
            => new DbContext(new DbContextOptionsBuilder()
                .UseNpgsql(connection)
                .UseInternalServiceProvider(_serviceProvider).Options);
    }
}
