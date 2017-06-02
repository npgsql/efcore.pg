// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class NorthwindQueryNpgsqlFixture : NorthwindQueryRelationalFixture, IDisposable
    {
        private readonly Func<Action<ModelBuilder>, Func<IServiceProvider, IModelSource>> _modelSourceFactory;

        public NorthwindQueryNpgsqlFixture() : this(TestModelSource.GetFactory)
        {
        }

        protected NorthwindQueryNpgsqlFixture(Func<Action<ModelBuilder>, Func<IServiceProvider, IModelSource>> modelSourceFactory)
        {
            _modelSourceFactory = modelSourceFactory;
        }

        private readonly NpgsqlTestStore _testStore = NpgsqlTestStore.GetNorthwindStore();

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public override DbContextOptions BuildOptions(IServiceCollection additionalServices = null)
            => ConfigureOptions(
                    new DbContextOptionsBuilder()
                        .ConfigureWarnings(w => w.Log(CoreEventId.IncludeIgnoredWarning))
                        .EnableSensitiveDataLogging()
                        .UseInternalServiceProvider(
                            (additionalServices ?? new ServiceCollection())
                            .AddEntityFrameworkNpgsql()
                            .AddSingleton(_modelSourceFactory(OnModelCreating))
                            .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                            .BuildServiceProvider()))
                .UseNpgsql(
                    _testStore.ConnectionString,
                    b =>
                    {
                        b.ApplyConfiguration();
                        ConfigureOptions(b);
                        b.ApplyConfiguration();
                    })
                .Options;

        protected virtual DbContextOptionsBuilder ConfigureOptions(DbContextOptionsBuilder dbContextOptionsBuilder)
            => dbContextOptionsBuilder;

        protected virtual void ConfigureOptions(NpgsqlDbContextOptionsBuilder NpgsqlDbContextOptionsBuilder)
        {
        }

        public void Dispose() => _testStore.Dispose();
    }
}
