using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class MigrationsNpgsqlFixture : MigrationsFixtureBase
    {
        private readonly DbContextOptions _options;

        public MigrationsNpgsqlFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .BuildServiceProvider();

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(TestEnvironment.DefaultConnection)
            {
                Database = nameof(MigrationsNpgsqlTest)
            };

            _options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider)
                .UseNpgsql(connectionStringBuilder.ConnectionString).Options;
        }

        public override MigrationsContext CreateContext() => new MigrationsContext(_options);

        public override EmptyMigrationsContext CreateEmptyContext() => new EmptyMigrationsContext(_options);
    }
}
