using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore;

public abstract class ConnectionInterceptionNpgsqlTestBase(ConnectionInterceptionNpgsqlTestBase.InterceptionNpgsqlFixtureBase fixture)
    : ConnectionInterceptionTestBase(fixture)
{
    public override async Task Intercept_connection_creation_passively(bool async)
    {
        var interceptor = new ConnectionCreationInterceptor();
        var context = new ConnectionStringContext(ConfigureProvider);
        context.Interceptors.Add(interceptor);

        _ = context.Model;

        Assert.False(interceptor.CreatingCalled);
        Assert.False(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);

        var connection = context.Database.GetDbConnection();

        Assert.True(interceptor.CreatingCalled);
        Assert.True(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Same(connection, interceptor.Connection);
        Assert.Equal(connection.ConnectionString, interceptor.ConnectionString ?? "");

        if (async)
        {
            await context.DisposeAsync();
        }
        else
        {
            context.Dispose();
        }

        Assert.True(interceptor.DisposingCalled);
        Assert.True(interceptor.DisposedCalled);
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
    }

    public override async Task Intercept_connection_creation_with_multiple_interceptors(bool async)
    {
        using var tempContext1 = new ConnectionStringContext(ConfigureProvider);
        var replacementConnection1 = tempContext1.Database.GetDbConnection();

        using var tempContext2 = new ConnectionStringContext(ConfigureProvider);
        var replacementConnection2 = tempContext2.Database.GetDbConnection();

        var interceptors = new[]
        {
            new ConnectionCreationInterceptor(),
            new ConnectionCreationOverrideInterceptor(replacementConnection1),
            new ConnectionCreationInterceptor(),
            new ConnectionCreationOverrideInterceptor(replacementConnection2)
        };

        var context = new ConnectionStringContext(ConfigureProvider);
        context.Interceptors.AddRange(interceptors);

        var connection = context.Database.GetDbConnection();
        Assert.Same(replacementConnection2, connection);

        foreach (var interceptor in interceptors)
        {
            Assert.True(interceptor.CreatingCalled);
            Assert.True(interceptor.CreatedCalled);
            Assert.False(interceptor.DisposingCalled);
            Assert.False(interceptor.DisposedCalled);
            Assert.Same(context, interceptor.Context);
        }

        if (async)
        {
            await context.DisposeAsync();
        }
        else
        {
            context.Dispose();
        }

        foreach (var interceptor in interceptors)
        {
            Assert.True(interceptor.DisposingCalled);
            Assert.True(interceptor.DisposedCalled);
            Assert.Equal(async, interceptor.AsyncCalled);
            Assert.NotEqual(async, interceptor.SyncCalled);
        }
    }

    public override async Task Intercept_connection_to_override_connection_after_creation(bool async)
    {
        using var tempContext = new ConnectionStringContext(ConfigureProvider);
        var replacementConnection = tempContext.Database.GetDbConnection();

        var interceptor = new ConnectionCreationReplaceInterceptor(replacementConnection);
        var context = new ConnectionStringContext(ConfigureProvider);
        context.Interceptors.Add(interceptor);

        _ = context.Model;

        Assert.False(interceptor.CreatingCalled);
        Assert.False(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);

        var connection = context.Database.GetDbConnection();
        Assert.Same(replacementConnection, connection);

        Assert.True(interceptor.CreatingCalled);
        Assert.True(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Same(connection, interceptor.Connection);
        Assert.Equal(connection.ConnectionString, interceptor.ConnectionString ?? "");

        if (async)
        {
            await context.DisposeAsync();
        }
        else
        {
            context.Dispose();
        }

        Assert.True(interceptor.DisposingCalled);
        Assert.True(interceptor.DisposedCalled);
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
    }

    public override async Task Intercept_connection_to_override_creation(bool async)
    {
        using var tempContext = new ConnectionStringContext(ConfigureProvider);
        var replacementConnection = tempContext.Database.GetDbConnection();

        var interceptor = new ConnectionCreationOverrideInterceptor(replacementConnection);
        var context = new ConnectionStringContext(ConfigureProvider);
        context.Interceptors.Add(interceptor);

        _ = context.Model;

        Assert.False(interceptor.CreatingCalled);
        Assert.False(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);

        var connection = context.Database.GetDbConnection();
        Assert.Same(replacementConnection, connection);

        Assert.True(interceptor.CreatingCalled);
        Assert.True(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Same(connection, interceptor.Connection);
        Assert.Equal(connection.ConnectionString, interceptor.ConnectionString ?? "");

        if (async)
        {
            await context.DisposeAsync();
        }
        else
        {
            context.Dispose();
        }

        Assert.True(interceptor.DisposingCalled);
        Assert.True(interceptor.DisposedCalled);
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
    }

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
        => optionsBuilder.UseNpgsql("Host=localhost;Database=Dummy");

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
