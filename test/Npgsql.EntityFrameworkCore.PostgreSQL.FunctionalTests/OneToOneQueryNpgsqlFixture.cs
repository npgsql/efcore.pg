using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class OneToOneQueryNpgsqlFixture : OneToOneQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly NpgsqlTestStore _testStore;

        public OneToOneQueryNpgsqlFixture()
        {
            _testStore = NpgsqlTestStore.CreateScratch();

            _options = new DbContextOptionsBuilder()
                .UseNpgsql(_testStore.ConnectionString)
                .UseInternalServiceProvider(new ServiceCollection()
                    .AddEntityFrameworkNpgsql()
                    .AddSingleton(TestNpgsqlModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                    .BuildServiceProvider())
                .Options;

            using (var context = new DbContext(_options))
            {
                context.Database.EnsureCreated();

                AddTestData(context);
            }
        }

        public DbContext CreateContext() => new DbContext(_options);

        public void Dispose() => _testStore.Dispose();
    }
}
