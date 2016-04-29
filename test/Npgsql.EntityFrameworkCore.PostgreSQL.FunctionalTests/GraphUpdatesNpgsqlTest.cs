using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class GraphUpdatesNpgsqlTest
        : GraphUpdatesTestBase<NpgsqlTestStore, GraphUpdatesNpgsqlTest.GraphUpdatesNpgsqlFixture>
    {
        public GraphUpdatesNpgsqlTest(GraphUpdatesNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesNpgsqlFixture : GraphUpdatesFixtureBase
        {
            private const string DatabaseName = "GraphUpdatesTest";

            private readonly IServiceProvider _serviceProvider;

            public GraphUpdatesNpgsqlFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkNpgsql()
                    .AddSingleton(TestNpgsqlModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override NpgsqlTestStore CreateTestStore()
            {
                return NpgsqlTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseNpgsql(NpgsqlTestStore.CreateConnectionString(DatabaseName))
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new GraphUpdatesContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureDeleted();
                        if (context.Database.EnsureCreated())
                        {
                            Seed(context);
                        }
                    }
                });
            }

            public override DbContext CreateContext(NpgsqlTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseNpgsql(testStore.Connection)
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new GraphUpdatesContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);
                return context;
            }
        }
    }
}
