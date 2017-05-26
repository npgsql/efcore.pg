using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class LoadNpgsqlTest
        : LoadTestBase<NpgsqlTestStore, LoadNpgsqlTest.LoadNpgsqlFixture>
    {
        public LoadNpgsqlTest(LoadNpgsqlFixture fixture)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        public override void RecordLog() => Sql = Fixture.TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);

        private const string FileLineEnding = @"
";

        private string Sql { get; set; }

        public class LoadNpgsqlFixture : LoadFixtureBase
        {
            private const string DatabaseName = "LoadTest";
            private readonly DbContextOptions _options;

            public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

            public LoadNpgsqlFixture()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkNpgsql()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                    .BuildServiceProvider();

                _options = new DbContextOptionsBuilder()
                    .UseNpgsql(NpgsqlTestStore.CreateConnectionString(DatabaseName), b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(serviceProvider)
                    .EnableSensitiveDataLogging()
                    .Options;
            }

            public override NpgsqlTestStore CreateTestStore()
            {
                return NpgsqlTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = new LoadContext(_options))
                    {
                        context.Database.EnsureCreated();
                        Seed(context);
                    }
                });
            }

            public override DbContext CreateContext(NpgsqlTestStore testStore)
                => new LoadContext(_options);
        }
    }
}
