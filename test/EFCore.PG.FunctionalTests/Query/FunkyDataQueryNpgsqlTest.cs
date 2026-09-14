using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class FunkyDataQueryNpgsqlTest : FunkyDataQueryTestBase<FunkyDataQueryNpgsqlTest.FunkyDataQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public FunkyDataQueryNpgsqlTest(FunkyDataQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task String_FirstOrDefault_and_LastOrDefault(bool async)
        => Task.CompletedTask; // Npgsql doesn't support reading an empty string as a char at the ADO level

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task String_starts_with_on_argument_with_escape_constant(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName!.StartsWith("Some\\")),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName != null && c.FirstName.StartsWith("Some\\")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task String_starts_with_on_argument_with_escape_parameter(bool async)
    {
        var param = "Some\\";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName!.StartsWith(param)),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName != null && c.FirstName.StartsWith(param)));
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class FunkyDataQueryNpgsqlFixture : FunkyDataQueryFixtureBase, ITestSqlLoggerFactory
    {
        private NpgsqlFunkyData? _expectedData;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public override FunkyDataContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }

        public override ISetSource GetExpectedData()
            => _expectedData ??= new NpgsqlFunkyData();

        protected override async Task SeedAsync(FunkyDataContext context)
        {
            context.FunkyCustomers.AddRange(GetExpectedData().Set<FunkyCustomer>());
            await context.SaveChangesAsync();
        }

        private class NpgsqlFunkyData : ISetSource
        {
            private readonly IReadOnlyList<FunkyCustomer> _customers;

            public NpgsqlFunkyData()
            {
                var customers = FunkyDataData.CreateFunkyCustomers();
                _customers = [.. customers, new FunkyCustomer { Id = customers.Max(c => c.Id) + 1, FirstName = "Some\\Guy" }];
            }

            public IQueryable<TEntity> Set<TEntity>()
                where TEntity : class
                => typeof(TEntity) == typeof(FunkyCustomer)
                    ? (IQueryable<TEntity>)_customers.AsQueryable()
                    : throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }
    }
}
