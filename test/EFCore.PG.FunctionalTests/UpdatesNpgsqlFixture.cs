using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class UpdatesNpgsqlFixture : UpdatesFixtureBase<NpgsqlTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public UpdatesNpgsqlFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();
        }

        protected virtual string DatabaseName => "PartialUpdateNpgsqlTest";

        public override NpgsqlTestStore CreateTestStore()
            => NpgsqlTestStore.GetOrCreateShared(DatabaseName, () =>
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseNpgsql(NpgsqlTestStore.CreateConnectionString(DatabaseName), b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(_serviceProvider);

                using (var context = new UpdatesContext(optionsBuilder.Options))
                {
                    context.Database.EnsureCreated();
                    UpdatesModelInitializer.Seed(context);
                }
            });

        public override UpdatesContext CreateContext(NpgsqlTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseNpgsql(testStore.Connection, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(_serviceProvider);

            var context = new UpdatesContext(optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price).HasColumnType("decimal(18,2)");
        }
    }
}
