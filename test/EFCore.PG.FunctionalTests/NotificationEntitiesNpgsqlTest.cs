using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class NotificationEntitiesNpgsqlTest
        : NotificationEntitiesTestBase<NpgsqlTestStore, NotificationEntitiesNpgsqlTest.NotificationEntitiesNpgsqlFixture>
    {
        public NotificationEntitiesNpgsqlTest(NotificationEntitiesNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesNpgsqlFixture : NotificationEntitiesFixtureBase
        {
            public const string DatabaseName = "NotificationEntities";

            private readonly DbContextOptions _options;

            public NotificationEntitiesNpgsqlFixture()
            {
                _options = new DbContextOptionsBuilder()
                    .UseNpgsql(NpgsqlTestStore.CreateConnectionString(DatabaseName), b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkNpgsql()
                        .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider())
                    .Options;
            }

            public override DbContext CreateContext()
                => new DbContext(_options);

            public override NpgsqlTestStore CreateTestStore()
                => NpgsqlTestStore.GetOrCreateShared(DatabaseName, EnsureCreated);
        }
    }
}
