using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class FieldMappingNpgsqlTest
        : FieldMappingTestBase<NpgsqlTestStore, FieldMappingNpgsqlTest.FieldMappingNpgsqlFixture>
    {
        public FieldMappingNpgsqlTest(FieldMappingNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class FieldMappingNpgsqlFixture : FieldMappingFixtureBase
        {
            private const string DatabaseName = "FieldMapping";

            private readonly IServiceProvider _serviceProvider;

            public FieldMappingNpgsqlFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkNpgsql()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override NpgsqlTestStore CreateTestStore()
            {
                return NpgsqlTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseNpgsql(NpgsqlTestStore.CreateConnectionString(DatabaseName), b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new FieldMappingContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureCreated();
                        Seed(context);
                    }
                });
            }

            public override DbContext CreateContext(NpgsqlTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseNpgsql(testStore.Connection, b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new FieldMappingContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);

                return context;
            }
        }
    }
}
