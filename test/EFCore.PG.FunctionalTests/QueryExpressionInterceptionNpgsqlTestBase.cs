using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public abstract class QueryExpressionInterceptionNpgsqlTestBase : QueryExpressionInterceptionTestBase
{
    protected QueryExpressionInterceptionNpgsqlTestBase(InterceptionNpgsqlFixtureBase fixture)
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

    public class QueryExpressionInterceptionNpgsqlTest
        : QueryExpressionInterceptionNpgsqlTestBase, IClassFixture<QueryExpressionInterceptionNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public QueryExpressionInterceptionNpgsqlTest(InterceptionNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterception";

            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class QueryExpressionInterceptionWithDiagnosticsNpgsqlTest
        : QueryExpressionInterceptionNpgsqlTestBase,
            IClassFixture<QueryExpressionInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public QueryExpressionInterceptionWithDiagnosticsNpgsqlTest(InterceptionNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterceptionWithDiagnostics";

            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
