using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public abstract class ConnectionInterceptionNpgsqlTestBase : ConnectionInterceptionTestBase
    {
        protected ConnectionInterceptionNpgsqlTestBase(InterceptionNpgsqlFixtureBase fixture)
            : base(fixture)
        {
        }

        public abstract class InterceptionNpgsqlFixtureBase : InterceptionFixtureBase
        {
            protected override string StoreName => "ConnectionInterception";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkNpgsql(), injectedInterceptors);
        }

        protected override BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder)
            => new BadUniverseContext(optionsBuilder.UseNpgsql(new FakeDbConnection()).Options);

        public class FakeDbConnection : DbConnection
        {
            public override string ConnectionString { get; set; }
            public override string Database => "Database";
            public override string DataSource => "DataSource";
            public override string ServerVersion => throw new NotImplementedException();
            public override ConnectionState State => ConnectionState.Closed;
            public override void ChangeDatabase(string databaseName) => throw new NotImplementedException();
            public override void Close() => throw new NotImplementedException();
            public override void Open() => throw new NotImplementedException();
            protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel) => throw new NotImplementedException();
            protected override DbCommand CreateDbCommand() => throw new NotImplementedException();
        }

        public class ConnectionInterceptionNpgsqlTest
            : ConnectionInterceptionNpgsqlTestBase, IClassFixture<ConnectionInterceptionNpgsqlTest.InterceptionNpgsqlFixture>
        {
            public ConnectionInterceptionNpgsqlTest(InterceptionNpgsqlFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => false;
            }
        }

        public class ConnectionInterceptionWithDiagnosticsNpgsqlTest
            : ConnectionInterceptionNpgsqlTestBase, IClassFixture<ConnectionInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture>
        {
            public ConnectionInterceptionWithDiagnosticsNpgsqlTest(InterceptionNpgsqlFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => true;
            }
        }
    }
}
