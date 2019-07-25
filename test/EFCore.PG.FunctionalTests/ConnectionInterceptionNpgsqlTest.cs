using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class ConnectionInterceptionNpgsqlTest
        : ConnectionInterceptionTestBase, IClassFixture<ConnectionInterceptionNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public ConnectionInterceptionNpgsqlTest(InterceptionNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionNpgsqlFixture : InterceptionFixtureBase
        {
            protected override string StoreName => "ConnectionInterception";

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkNpgsql(), injectedInterceptors);
        }

        protected override BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder)
            => new BadUniverseContext(optionsBuilder.UseNpgsql("Database = IIzBad").Options);
    }
}
