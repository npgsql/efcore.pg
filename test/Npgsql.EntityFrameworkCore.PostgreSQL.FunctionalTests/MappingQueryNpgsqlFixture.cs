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
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.TestModels;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class MappingQueryNpgsqlFixture : MappingQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly NpgsqlTestStore _testDatabase;

        public MappingQueryNpgsqlFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _testDatabase = NpgsqlNorthwindContext.GetSharedStore();

            _options = new DbContextOptionsBuilder()
                .UseModel(CreateModel())
                .UseNpgsql(_testDatabase.ConnectionString)
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public DbContext CreateContext()
        {
            var context = new DbContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }

        public void Dispose() => _testDatabase.Dispose();

        protected override string DatabaseSchema { get; } = null;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MappingQueryTestBase.MappedCustomer>(e =>
            {
                e.Property(c => c.CompanyName2).Metadata.Npgsql().ColumnName = "CompanyName";
                e.Metadata.Npgsql().TableName = "Customers";
            });
        }
    }
}
