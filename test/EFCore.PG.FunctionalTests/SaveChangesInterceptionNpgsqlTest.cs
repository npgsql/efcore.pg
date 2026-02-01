using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

public abstract class SaveChangesInterceptionNpgsqlTestBase(SaveChangesInterceptionNpgsqlTestBase.InterceptionNpgsqlFixtureBase fixture)
    : SaveChangesInterceptionTestBase(fixture)
{
    public abstract class InterceptionNpgsqlFixtureBase : InterceptionFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkNpgsql(), injectedInterceptors);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new NpgsqlDbContextOptionsBuilder(base.AddOptions(builder))
                .ExecutionStrategy(d => new NpgsqlExecutionStrategy(d));
            return builder;
        }
    }

    public class SaveChangesInterceptionNpgsqlTest(SaveChangesInterceptionNpgsqlTest.InterceptionNpgsqlFixture fixture)
        : SaveChangesInterceptionNpgsqlTestBase(fixture), IClassFixture<SaveChangesInterceptionNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
            protected override string StoreName
                => "SaveChangesInterception";

            protected override bool ShouldSubscribeToDiagnosticListener
                => false;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                new NpgsqlDbContextOptionsBuilder(base.AddOptions(builder))
                    .ExecutionStrategy(d => new NpgsqlExecutionStrategy(d));
                return builder;
            }
        }
    }

    public class SaveChangesInterceptionWithDiagnosticsNpgsqlTest(
        SaveChangesInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture fixture)
        : SaveChangesInterceptionNpgsqlTestBase(fixture),
            IClassFixture<SaveChangesInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
            protected override string StoreName
                => "SaveChangesInterceptionWithDiagnostics";

            protected override bool ShouldSubscribeToDiagnosticListener
                => true;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                new NpgsqlDbContextOptionsBuilder(base.AddOptions(builder))
                    .ExecutionStrategy(d => new NpgsqlExecutionStrategy(d));
                return builder;
            }
        }
    }
}
