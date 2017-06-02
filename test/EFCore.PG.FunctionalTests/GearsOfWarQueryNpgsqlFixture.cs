using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class GearsOfWarQueryNpgsqlFixture : GearsOfWarQueryRelationalFixture<NpgsqlTestStore>
    {
        public const string DatabaseName = "GearsOfWarQueryTest";

        private readonly DbContextOptions _options;

        private readonly string _connectionString = NpgsqlTestStore.CreateConnectionString(DatabaseName);

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public GearsOfWarQueryNpgsqlFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(w => w.Log(CoreEventId.IncludeIgnoredWarning))
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public override NpgsqlTestStore CreateTestStore()
        {
            return NpgsqlTestStore.GetOrCreateShared(DatabaseName, () =>
            {
                using (var context = new GearsOfWarContext(
                    new DbContextOptionsBuilder(_options)
                        .UseNpgsql(_connectionString, b => b.ApplyConfiguration())
                        .Options))
                {
                    context.Database.EnsureCreated();
                    GearsOfWarModelInitializer.Seed(context);
                }
            });
        }

        public override GearsOfWarContext CreateContext(NpgsqlTestStore testStore)
        {
            var context = new GearsOfWarContext(
                new DbContextOptionsBuilder(_options)
                    .UseNpgsql(testStore.Connection, b => b.ApplyConfiguration())
                    .Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");

            base.OnModelCreating(modelBuilder);
        }
    }
}
