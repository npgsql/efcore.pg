using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceNpgsqlFixture : InheritanceRelationalFixture<NpgsqlTestStore>
    {
        protected virtual string DatabaseName => "InheritanceNpgsqlTest";

        private readonly DbContextOptions _options;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public InheritanceNpgsqlFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public override NpgsqlTestStore CreateTestStore()
        {
            return NpgsqlTestStore.GetOrCreateShared(
                DatabaseName, () =>
                {
                    using (var context = new InheritanceContext(
                        new DbContextOptionsBuilder(_options)
                            .UseNpgsql(
                                NpgsqlTestStore.CreateConnectionString(DatabaseName),
                                b => b.ApplyConfiguration())
                            .Options))
                    {
                        context.Database.EnsureCreated();
                        InheritanceModelInitializer.SeedData(context);
                    }
                });
        }

        public override InheritanceContext CreateContext(NpgsqlTestStore testStore)
        {
            var context = new InheritanceContext(
                new DbContextOptionsBuilder(_options)
                    .UseNpgsql(testStore.Connection, b => b.ApplyConfiguration())
                    .Options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
