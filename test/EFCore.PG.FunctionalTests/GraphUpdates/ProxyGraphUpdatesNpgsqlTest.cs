using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.GraphUpdates
{
    public abstract class ProxyGraphUpdatesNpgsqlTest
    {
        public abstract class ProxyGraphUpdatesNpgsqlTestBase<TFixture> : ProxyGraphUpdatesTestBase<TFixture>
            where TFixture : ProxyGraphUpdatesNpgsqlTestBase<TFixture>.ProxyGraphUpdatesNpgsqlFixtureBase, new()
        {
            protected ProxyGraphUpdatesNpgsqlTestBase(TFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public abstract class ProxyGraphUpdatesNpgsqlFixtureBase : ProxyGraphUpdatesFixtureBase
            {
                public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
                protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            }
        }

        public class LazyLoading : ProxyGraphUpdatesNpgsqlTestBase<LazyLoading.ProxyGraphUpdatesWithLazyLoadingNpgsqlFixture>
        {
            public LazyLoading(ProxyGraphUpdatesWithLazyLoadingNpgsqlFixture fixture)
                : base(fixture)
            {
            }

            protected override bool DoesLazyLoading => true;
            protected override bool DoesChangeTracking => false;

            public class ProxyGraphUpdatesWithLazyLoadingNpgsqlFixture : ProxyGraphUpdatesNpgsqlFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphLazyLoadingUpdatesTest";

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.UseLazyLoadingProxies());

                protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                    => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    if (TestEnvironment.PostgresVersion >= new Version(10, 0))
                        modelBuilder.UseIdentityColumns();

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class ChangeTracking : ProxyGraphUpdatesNpgsqlTestBase<ChangeTracking.ProxyGraphUpdatesWithChangeTrackingNpgsqlFixture>
        {
            public ChangeTracking(ProxyGraphUpdatesWithChangeTrackingNpgsqlFixture fixture)
                : base(fixture)
            {
            }

            protected override bool DoesLazyLoading => false;
            protected override bool DoesChangeTracking => true;

            public class ProxyGraphUpdatesWithChangeTrackingNpgsqlFixture : ProxyGraphUpdatesNpgsqlFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphChangeTrackingUpdatesTest";

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.UseChangeTrackingProxies());

                protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                    => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    if (TestEnvironment.PostgresVersion >= new Version(10, 0))
                        modelBuilder.UseIdentityColumns();

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class ChangeTrackingAndLazyLoading : ProxyGraphUpdatesNpgsqlTestBase<ChangeTrackingAndLazyLoading.ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingNpgsqlFixture>
        {
            public ChangeTrackingAndLazyLoading(ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingNpgsqlFixture fixture)
                : base(fixture)
            {
            }

            protected override bool DoesLazyLoading => true;
            protected override bool DoesChangeTracking => true;

            public class ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingNpgsqlFixture : ProxyGraphUpdatesNpgsqlFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphChangeTrackingAndLazyLoadingUpdatesTest";

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.UseLazyLoadingProxies().UseChangeTrackingProxies());

                protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                    => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    if (TestEnvironment.PostgresVersion >= new Version(10, 0))
                        modelBuilder.UseIdentityColumns();

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }
    }
}
