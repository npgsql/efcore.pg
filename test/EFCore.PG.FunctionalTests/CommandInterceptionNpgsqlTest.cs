using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

public abstract class CommandInterceptionNpgsqlTestBase(CommandInterceptionNpgsqlTestBase.InterceptionNpgsqlFixtureBase fixture)
    : CommandInterceptionTestBase(fixture)
{
    public abstract class InterceptionNpgsqlFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "CommandInterception";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkNpgsql(), injectedInterceptors);
    }

    public class CommandInterceptionNpgsqlTest(CommandInterceptionNpgsqlTest.InterceptionNpgsqlFixture fixture)
        : CommandInterceptionNpgsqlTestBase(fixture), IClassFixture<CommandInterceptionNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
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

    public class CommandInterceptionWithDiagnosticsNpgsqlTest(
        CommandInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture fixture)
        : CommandInterceptionNpgsqlTestBase(fixture), IClassFixture<CommandInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
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
