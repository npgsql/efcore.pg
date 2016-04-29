using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class ComplexNavigationsQueryNpgsqlFixture : ComplexNavigationsQueryRelationalFixture<NpgsqlTestStore>
    {
        public static readonly string DatabaseName = "ComplexNavigations";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = NpgsqlTestStore.CreateConnectionString(DatabaseName);

        public ComplexNavigationsQueryNpgsqlFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddSingleton(TestNpgsqlModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override NpgsqlTestStore CreateTestStore() =>
            NpgsqlTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseNpgsql(_connectionString)
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new ComplexNavigationsContext(optionsBuilder.Options))
                    {
                        // TODO: Delete DB if model changed
                        context.Database.EnsureDeleted();
                        if (context.Database.EnsureCreated())
                        {
                            ComplexNavigationsModelInitializer.Seed(context);
                        }

                        TestSqlLoggerFactory.SqlStatements.Clear();
                    }
                });

        public override ComplexNavigationsContext CreateContext(NpgsqlTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseNpgsql(testStore.Connection)
                .UseInternalServiceProvider(_serviceProvider);

            var context = new ComplexNavigationsContext(optionsBuilder.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
