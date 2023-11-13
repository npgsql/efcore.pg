using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public abstract class SaveChangesInterceptionNpgsqlTestBase : SaveChangesInterceptionTestBase
{
    protected SaveChangesInterceptionNpgsqlTestBase(InterceptionNpgsqlFixtureBase fixture)
        : base(fixture)
    {
    }

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

    public class SaveChangesInterceptionNpgsqlTest
        : SaveChangesInterceptionNpgsqlTestBase, IClassFixture<SaveChangesInterceptionNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public SaveChangesInterceptionNpgsqlTest(InterceptionNpgsqlFixture fixture)
            : base(fixture)
        {
        }

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

    public class SaveChangesInterceptionWithDiagnosticsNpgsqlTest
        : SaveChangesInterceptionNpgsqlTestBase,
            IClassFixture<SaveChangesInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public SaveChangesInterceptionWithDiagnosticsNpgsqlTest(InterceptionNpgsqlFixture fixture)
            : base(fixture)
        {
        }

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
