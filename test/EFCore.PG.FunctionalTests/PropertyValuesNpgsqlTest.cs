using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class PropertyValuesNpgsqlTest
        : PropertyValuesTestBase<NpgsqlTestStore, PropertyValuesNpgsqlTest.PropertyValuesNpgsqlFixture>
    {
        public PropertyValuesNpgsqlTest(PropertyValuesNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class PropertyValuesNpgsqlFixture : PropertyValuesFixtureBase
        {
            private const string DatabaseName = "PropertyValues";

            private readonly IServiceProvider _serviceProvider;

            public PropertyValuesNpgsqlFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkNpgsql()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override NpgsqlTestStore CreateTestStore()
                => NpgsqlTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseNpgsql(NpgsqlTestStore.CreateConnectionString(DatabaseName), b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new AdvancedPatternsMasterContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureCreated();
                        Seed(context);
                    }
                });

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Building>()
                    .Property(b => b.Value).ForNpgsqlHasColumnType("decimal(18,2)");

                modelBuilder.Entity<CurrentEmployee>()
                    .Property(ce => ce.LeaveBalance).ForNpgsqlHasColumnType("decimal(18,2)");
            }

            public override DbContext CreateContext(NpgsqlTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseNpgsql(testStore.Connection, b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new AdvancedPatternsMasterContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);

                return context;
            }
        }
    }
}
