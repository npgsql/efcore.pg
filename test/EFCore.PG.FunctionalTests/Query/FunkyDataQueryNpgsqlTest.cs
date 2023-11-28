using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

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
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("Some\\")),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName != null && c.FirstName.StartsWith("Some\\")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task String_starts_with_on_argument_with_escape_parameter(bool async)
    {
        var param = "Some\\";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(param)),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName != null && c.FirstName.StartsWith(param)));
    }

    [ConditionalTheory] // TODO: Remove, test was introduced upstream
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Contains_and_StartsWith_with_same_parameter(bool async)
    {
        var s = "B";

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(
                c => c.FirstName.Contains(s) || c.LastName.StartsWith(s)),
            ss => ss.Set<FunkyCustomer>().Where(
                c => c.FirstName.MaybeScalar(f => f.Contains(s)) == true || c.LastName.MaybeScalar(l => l.StartsWith(s)) == true));

        AssertSql(
            """
@__s_0_contains='%B%'
@__s_0_startswith='B%'

SELECT f."Id", f."FirstName", f."LastName", f."NullableBool"
FROM "FunkyCustomers" AS f
WHERE f."FirstName" LIKE @__s_0_contains ESCAPE '\' OR f."LastName" LIKE @__s_0_startswith ESCAPE '\'
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class FunkyDataQueryNpgsqlFixture : FunkyDataQueryFixtureBase
    {
        private FunkyDataData _expectedData;

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
        {
            if (_expectedData is null)
            {
                _expectedData = (FunkyDataData)base.GetExpectedData();

                var maxId = _expectedData.FunkyCustomers.Max(c => c.Id);

                var mutableCustomersOhYeah = (List<FunkyCustomer>)_expectedData.FunkyCustomers;

                mutableCustomersOhYeah.Add(
                    new FunkyCustomer
                    {
                        Id = maxId + 1,
                        FirstName = "Some\\Guy",
                        LastName = null
                    });
            }

            return _expectedData;
        }

        protected override void Seed(FunkyDataContext context)
        {
            context.FunkyCustomers.AddRange(GetExpectedData().Set<FunkyCustomer>());
            context.SaveChanges();
        }
    }
}
