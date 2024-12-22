using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore;

public abstract class ConnectionInterceptionNpgsqlTestBase(ConnectionInterceptionNpgsqlTestBase.InterceptionNpgsqlFixtureBase fixture)
    : ConnectionInterceptionTestBase(fixture)
{
    [ConditionalTheory(Skip = "#2368")]
    public override Task Intercept_connection_creation_passively(bool async)
        => base.Intercept_connection_creation_passively(async);

    [ConditionalTheory(Skip = "#2368")]
    public override Task Intercept_connection_creation_with_multiple_interceptors(bool async)
        => base.Intercept_connection_creation_with_multiple_interceptors(async);

    [ConditionalTheory(Skip = "#2368")]
    public override Task Intercept_connection_to_override_connection_after_creation(bool async)
        => base.Intercept_connection_to_override_connection_after_creation(async);

    [ConditionalTheory(Skip = "#2368")]
    public override Task Intercept_connection_to_override_creation(bool async)
        => base.Intercept_connection_to_override_creation(async);

    public abstract class InterceptionNpgsqlFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "ConnectionInterception";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkNpgsql(), injectedInterceptors);
    }

    protected override DbContextOptionsBuilder ConfigureProvider(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql();

    protected override BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder)
        => new(optionsBuilder.UseNpgsql(new FakeDbConnection()).Options);

    public class FakeDbConnection : DbConnection
    {
        [AllowNull]
        public override string ConnectionString { get; set; }

        public override string Database
            => "Database";

        public override string DataSource
            => "DataSource";

        public override string ServerVersion
            => throw new NotImplementedException();

        public override ConnectionState State
            => ConnectionState.Closed;

        public override void ChangeDatabase(string databaseName)
            => throw new NotImplementedException();

        public override void Close()
            => throw new NotImplementedException();

        public override void Open()
            => throw new NotImplementedException();

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => throw new NotImplementedException();

        protected override DbCommand CreateDbCommand()
            => throw new NotImplementedException();
    }

    public class ConnectionInterceptionNpgsqlTest(ConnectionInterceptionNpgsqlTest.InterceptionNpgsqlFixture fixture)
        : ConnectionInterceptionNpgsqlTestBase(fixture), IClassFixture<ConnectionInterceptionNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class ConnectionInterceptionWithDiagnosticsNpgsqlTest(
        ConnectionInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture fixture)
        : ConnectionInterceptionNpgsqlTestBase(fixture),
            IClassFixture<ConnectionInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
