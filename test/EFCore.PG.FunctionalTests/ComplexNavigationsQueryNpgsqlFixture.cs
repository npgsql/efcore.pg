using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class ComplexNavigationsQueryNpgsqlFixture
        : ComplexNavigationsQueryFixtureBase<NpgsqlTestStore>
    {
        public static readonly string DatabaseName = "ComplexNavigations";

        private readonly IServiceProvider _serviceProvider;

        private readonly DbContextOptions _options;

        private readonly string _connectionString =
            NpgsqlTestStore.CreateConnectionString(DatabaseName);

        public ComplexNavigationsQueryNpgsqlFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseNpgsql(_connectionString, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(_serviceProvider).Options;
        }

        public override NpgsqlTestStore CreateTestStore()
        {
            return NpgsqlTestStore.GetOrCreateShared(DatabaseName, () =>
            {
                using (var context = new ComplexNavigationsContext(_options))
                {
                    context.Database.EnsureCreated();
                    ComplexNavigationsModelInitializer.Seed(context);
                }
            });
        }

        public override ComplexNavigationsContext CreateContext(NpgsqlTestStore testStore)
        {
            var context = new ComplexNavigationsContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
