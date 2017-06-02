using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class DataAnnotationNpgsqlFixture : DataAnnotationFixtureBase<NpgsqlTestStore>
    {
        public static readonly string DatabaseName = "DataAnnotations";

        private readonly string _connectionString = NpgsqlTestStore.CreateConnectionString(DatabaseName);
        private readonly DbContextOptions _options;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();
        public DataAnnotationNpgsqlFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(serviceProvider)
                .ConfigureWarnings(w =>
                {
                    w.Default(WarningBehavior.Throw);
                    w.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning);
                }).Options;
        }

        public override NpgsqlTestStore CreateTestStore()
            => NpgsqlTestStore.GetOrCreateShared(DatabaseName, () =>
            {
                var options = new DbContextOptionsBuilder(_options)
                    .UseNpgsql(_connectionString, b => b.ApplyConfiguration())
                    .Options;

                using (var context = new DataAnnotationContext(options))
                {
                    context.Database.EnsureCreated();
                    DataAnnotationModelInitializer.Seed(context);
                }
            });

        public override DataAnnotationContext CreateContext(NpgsqlTestStore testStore)
        {
            var options = new DbContextOptionsBuilder(_options)
                .UseNpgsql(testStore.Connection, b => b.ApplyConfiguration())
                .Options;

            var context = new DataAnnotationContext(options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
