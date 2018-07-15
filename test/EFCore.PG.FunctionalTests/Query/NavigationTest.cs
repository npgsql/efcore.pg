using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NavigationTest : IClassFixture<NavigationTestFixture>
    {
        [Fact]
        public void Duplicate_entries_are_not_created_for_navigations_to_principal()
        {
            using (var context = _fixture.CreateContext())
            {
                context.ConfigAction = modelBuilder =>
                {
                    modelBuilder.Entity<GoTPerson>().HasMany(p => p.Siblings).WithOne(p => p.SiblingReverse).IsRequired(false);
                    modelBuilder.Entity<GoTPerson>().HasOne(p => p.Lover).WithOne(p => p.LoverReverse).IsRequired(false);
                    return 0;
                };

                var model = context.Model;
                var entityType = model.GetEntityTypes().First();

                Assert.Equal(
                    "ForeignKey: GoTPerson {'LoverId'} -> GoTPerson {'Id'} Unique ToDependent: LoverReverse ToPrincipal: Lover",
                    entityType.GetForeignKeys().First().ToString());

                Assert.Equal(
                    "ForeignKey: GoTPerson {'SiblingReverseId'} -> GoTPerson {'Id'} ToDependent: Siblings ToPrincipal: SiblingReverse",
                    entityType.GetForeignKeys().Skip(1).First().ToString());
            }
        }

        [Fact]
        public void Duplicate_entries_are_not_created_for_navigations_to_dependant()
        {
            using (var context = _fixture.CreateContext())
            {
                context.ConfigAction = modelBuilder =>
                {
                    modelBuilder.Entity<GoTPerson>().HasOne(p => p.SiblingReverse).WithMany(p => p.Siblings).IsRequired(false);
                    modelBuilder.Entity<GoTPerson>().HasOne(p => p.Lover).WithOne(p => p.LoverReverse).IsRequired(false);
                    return 0;
                };

                var model = context.Model;
                var entityType = model.GetEntityTypes().First();

                Assert.Equal(
                    "ForeignKey: GoTPerson {'LoverId'} -> GoTPerson {'Id'} Unique ToDependent: LoverReverse ToPrincipal: Lover",
                    entityType.GetForeignKeys().First().ToString());

                Assert.Equal(
                    "ForeignKey: GoTPerson {'SiblingReverseId'} -> GoTPerson {'Id'} ToDependent: Siblings ToPrincipal: SiblingReverse",
                    entityType.GetForeignKeys().Skip(1).First().ToString());
            }
        }

        private readonly NavigationTestFixture _fixture;

        public NavigationTest(NavigationTestFixture fixture) => _fixture = fixture;
    }

    public class GoTPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<GoTPerson> Siblings { get; set; }
        public GoTPerson Lover { get; set; }
        public GoTPerson LoverReverse { get; set; }
        public GoTPerson SiblingReverse { get; set; }
    }

    public class GoTContext : DbContext
    {
        public GoTContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<GoTPerson> People { get; set; }
        public Func<ModelBuilder, int> ConfigAction { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) => ConfigAction.Invoke(modelBuilder);
    }

    public class NavigationTestFixture
    {
        private readonly DbContextOptions _options;

        public NavigationTestFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .BuildServiceProvider();

            var connStrBuilder = new NpgsqlConnectionStringBuilder(TestEnvironment.DefaultConnection)
            {
                Database = "StateManagerBug"
            };

            _options = new DbContextOptionsBuilder()
                .UseNpgsql(connStrBuilder.ConnectionString)
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public virtual GoTContext CreateContext() => new GoTContext(_options);
    }
}
