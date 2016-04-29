using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.GearsOfWarModel;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class GearsOfWarQueryNpgsqlFixture : GearsOfWarQueryRelationalFixture<NpgsqlTestStore>
    {
        public static readonly string DatabaseName = "GearsOfWarQueryTest";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = NpgsqlTestStore.CreateConnectionString(DatabaseName);

        public GearsOfWarQueryNpgsqlFixture()
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

                using (var context = new GearsOfWarContext(optionsBuilder.Options))
                {
                    // TODO: Delete DB if model changed
                    context.Database.EnsureDeleted();
                    if (context.Database.EnsureCreated())
                    {
                        GearsOfWarModelInitializer.Seed(context);
                    }

                    TestSqlLoggerFactory.SqlStatements.Clear();
                }
            });
        }

        public override GearsOfWarContext CreateContext(NpgsqlTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            optionsBuilder
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(_serviceProvider)
                .UseNpgsql(testStore.Connection);

            var context = new GearsOfWarContext(optionsBuilder.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
