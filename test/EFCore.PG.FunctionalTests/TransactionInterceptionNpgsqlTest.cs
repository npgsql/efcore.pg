namespace Microsoft.EntityFrameworkCore;

public abstract class TransactionInterceptionNpgsqlTestBase(TransactionInterceptionNpgsqlTestBase.InterceptionNpgsqlFixtureBase fixture)
    : TransactionInterceptionTestBase(fixture)
{
    public abstract class InterceptionNpgsqlFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "TransactionInterception";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkNpgsql(), injectedInterceptors);
    }

    public class TransactionInterceptionNpgsqlTest(TransactionInterceptionNpgsqlTest.InterceptionNpgsqlFixture fixture)
        : TransactionInterceptionNpgsqlTestBase(fixture), IClassFixture<TransactionInterceptionNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class TransactionInterceptionWithDiagnosticsNpgsqlTest(
        TransactionInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture fixture)
        : TransactionInterceptionNpgsqlTestBase(fixture),
            IClassFixture<TransactionInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
