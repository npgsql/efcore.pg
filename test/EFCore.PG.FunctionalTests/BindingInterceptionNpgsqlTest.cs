#nullable enable

using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class BindingInterceptionNpgsqlTest : BindingInterceptionTestBase,
    IClassFixture<BindingInterceptionNpgsqlTest.BindingInterceptionNpgsqlFixture>
{
    public BindingInterceptionNpgsqlTest(BindingInterceptionNpgsqlFixture fixture)
        : base(fixture)
    {
    }

    public class BindingInterceptionNpgsqlFixture : SingletonInterceptorsFixtureBase
    {
        protected override string StoreName
            => "BindingInterception";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<ISingletonInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkNpgsql(), injectedInterceptors);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new NpgsqlDbContextOptionsBuilder(base.AddOptions(builder))
                .ExecutionStrategy(d => new NpgsqlExecutionStrategy(d));
            return builder;
        }
    }
}
