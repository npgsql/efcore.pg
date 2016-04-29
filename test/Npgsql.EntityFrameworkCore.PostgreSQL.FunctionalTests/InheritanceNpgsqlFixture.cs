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
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Inheritance;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class InheritanceNpgsqlFixture : TempInheritanceRelationalFixture
    {
        private readonly DbContextOptions _options;
        private readonly NpgsqlTestStore _testStore;

        public InheritanceNpgsqlFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddSingleton(TestNpgsqlModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _testStore = NpgsqlTestStore.CreateScratch();

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseNpgsql(_testStore.Connection)
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using (var context = CreateContext())
            {
                context.Database.EnsureCreated();
                SeedData(context);
            }
        }

        public override InheritanceContext CreateContext() => new InheritanceContext(_options);
        public void Dispose() => _testStore.Dispose();
    }
}
