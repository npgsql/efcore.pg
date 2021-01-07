using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class FunkyDataQueryNpgsqlTest : FunkyDataQueryTestBase<FunkyDataQueryNpgsqlTest.FunkyDataQueryNpgsqlFixture>
    {
        public FunkyDataQueryNpgsqlTest(FunkyDataQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory(Skip = "Npgsql doesn't support reading an empty string as a char at the ADO level")]
        public override Task String_FirstOrDefault_and_LastOrDefault(bool async)
            => base.String_FirstOrDefault_and_LastOrDefault(async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task String_starts_with_on_argument_with_escape_constant(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("Some\\")));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task String_starts_with_on_argument_with_escape_parameter(bool async)
        {
            var param = "Some\\";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(param)));
        }

        public class FunkyDataQueryNpgsqlFixture : FunkyDataQueryFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            public override FunkyDataContext CreateContext()
            {
                var context = base.CreateContext();
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                return context;
            }

            public override ISetSource GetExpectedData()
                => new NpgsqlFunkyDataData();

            protected override void Seed(FunkyDataContext context)
            {
                context.FunkyCustomers.AddRange(NpgsqlFunkyDataData.CreateFunkyCustomers());
                context.SaveChanges();
            }

            public class NpgsqlFunkyDataData : FunkyDataData
            {
                public new IReadOnlyList<FunkyCustomer> FunkyCustomers { get; }

                public NpgsqlFunkyDataData()
                    => FunkyCustomers = CreateFunkyCustomers();

                public override IQueryable<TEntity> Set<TEntity>()
                    where TEntity : class
                {
                    if (typeof(TEntity) == typeof(FunkyCustomer))
                    {
                        return (IQueryable<TEntity>)FunkyCustomers.AsQueryable();
                    }

                    return base.Set<TEntity>();
                }

                public new static IReadOnlyList<FunkyCustomer> CreateFunkyCustomers()
                {
                    var customers = FunkyDataData.CreateFunkyCustomers();
                    var maxId = customers.Max(c => c.Id);

                    return customers
                        .Append(new FunkyCustomer
                        {
                            Id = maxId + 1,
                            FirstName = "Some\\Guy",
                            LastName = null
                        })
                        .ToList();
                }
            }
        }
    }
}
