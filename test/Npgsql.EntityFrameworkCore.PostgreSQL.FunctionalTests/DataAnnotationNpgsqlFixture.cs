using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class DataAnnotationNpgsqlFixture : DataAnnotationFixtureBase<NpgsqlTestStore>
    {
        public static readonly string DatabaseName = "DataAnnotations";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = NpgsqlTestStore.CreateConnectionString(DatabaseName);

        public DataAnnotationNpgsqlFixture()
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

                using (var context = new DataAnnotationContext(optionsBuilder.Options))
                {
                    // TODO: Delete DB if model changed
                    context.Database.EnsureDeleted();
                    if (context.Database.EnsureCreated())
                    {
                        DataAnnotationModelInitializer.Seed(context);
                    }

                    TestSqlLoggerFactory.SqlStatements.Clear();
                }
            });
        }

        public override DataAnnotationContext CreateContext(NpgsqlTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseNpgsql(testStore.Connection)
                .UseInternalServiceProvider(_serviceProvider);

            var context = new DataAnnotationContext(optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
