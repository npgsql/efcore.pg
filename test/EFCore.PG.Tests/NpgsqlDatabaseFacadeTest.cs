using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlDatabaseFacadeTest
    {
        [Fact]
        public void IsNpgsql_when_using_OnConfguring()
        {
            using (var context = new NpgsqlOnConfiguringContext())
            {
                Assert.True(context.Database.IsNpgsql());
            }
        }

        [Fact]
        public void IsNpgsql_in_OnModelCreating_when_using_OnConfguring()
        {
            using (var context = new NpgsqlOnModelContext())
            {
                var _ = context.Model; // Trigger context initialization
                Assert.True(context.IsNpgsqlSet);
            }
        }

        [Fact]
        public void IsNpgsql_in_constructor_when_using_OnConfguring()
        {
            using (var context = new NpgsqlConstructorContext())
            {
                var _ = context.Model; // Trigger context initialization
                Assert.True(context.IsNpgsqlSet);
            }
        }

        [Fact]
        public void Cannot_use_IsNpgsql_in_OnConfguring()
        {
            using (var context = new NpgsqlUseInOnConfiguringContext())
            {
                Assert.Equal(
                    CoreStrings.RecursiveOnConfiguring,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        {
                            var _ = context.Model; // Trigger context initialization
                        }).Message);
            }
        }

        [Fact]
        public void IsNpgsql_when_using_constructor()
        {
            using (var context = new ProviderContext(
                new DbContextOptionsBuilder().UseNpgsql("Database=Maltesers").Options))
            {
                Assert.True(context.Database.IsNpgsql());
            }
        }

        [Fact]
        public void IsNpgsql_in_OnModelCreating_when_using_constructor()
        {
            using (var context = new ProviderOnModelContext(
                new DbContextOptionsBuilder().UseNpgsql("Database=Maltesers").Options))
            {
                var _ = context.Model; // Trigger context initialization
                Assert.True(context.IsNpgsqlSet);
            }
        }

        [Fact]
        public void IsNpgsql_in_constructor_when_using_constructor()
        {
            using (var context = new ProviderConstructorContext(
                new DbContextOptionsBuilder().UseNpgsql("Database=Maltesers").Options))
            {
                var _ = context.Model; // Trigger context initialization
                Assert.True(context.IsNpgsqlSet);
            }
        }

        [Fact]
        public void Cannot_use_IsNpgsql_in_OnConfguring_with_constructor()
        {
            using (var context = new ProviderUseInOnConfiguringContext(
                new DbContextOptionsBuilder().UseNpgsql("Database=Maltesers").Options))
            {
                Assert.Equal(
                    CoreStrings.RecursiveOnConfiguring,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        {
                            var _ = context.Model; // Trigger context initialization
                        }).Message);
            }
        }

        /*
        [Fact]
        public void Not_IsNpgsql_when_using_different_provider()
        {
            using (var context = new ProviderContext(
                new DbContextOptionsBuilder().UseInMemoryDatabase("Maltesers").Options))
            {
                Assert.False(context.Database.IsNpgsql());
            }
        }*/

        private class ProviderContext : DbContext
        {
            protected ProviderContext()
            {
            }

            public ProviderContext(DbContextOptions options)
                : base(options)
            {
            }

            public bool? IsNpgsqlSet { get; protected set; }
        }

        private class NpgsqlOnConfiguringContext : ProviderContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseNpgsql("Database=Maltesers");
        }

        private class NpgsqlOnModelContext : NpgsqlOnConfiguringContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => IsNpgsqlSet = Database.IsNpgsql();
        }

        private class NpgsqlConstructorContext : NpgsqlOnConfiguringContext
        {
            public NpgsqlConstructorContext()
                => IsNpgsqlSet = Database.IsNpgsql();
        }

        private class NpgsqlUseInOnConfiguringContext : NpgsqlOnConfiguringContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                base.OnConfiguring(optionsBuilder);

                IsNpgsqlSet = Database.IsNpgsql();
            }
        }

        private class ProviderOnModelContext : ProviderContext
        {
            public ProviderOnModelContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => IsNpgsqlSet = Database.IsNpgsql();
        }

        private class ProviderConstructorContext : ProviderContext
        {
            public ProviderConstructorContext(DbContextOptions options)
                : base(options)
                => IsNpgsqlSet = Database.IsNpgsql();
        }

        private class ProviderUseInOnConfiguringContext : ProviderContext
        {
            public ProviderUseInOnConfiguringContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => IsNpgsqlSet = Database.IsNpgsql();
        }
    }
}
