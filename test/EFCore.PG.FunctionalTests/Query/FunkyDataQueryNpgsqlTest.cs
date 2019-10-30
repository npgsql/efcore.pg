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

        [ConditionalTheory(Skip = "https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/996")]
        public override Task String_contains_on_argument_with_wildcard_column(bool isAsync)
            => base.String_contains_on_argument_with_wildcard_column(isAsync);

        [ConditionalTheory(Skip = "https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/996")]
        public override Task String_contains_on_argument_with_wildcard_column_negated(bool isAsync)
            => base.String_contains_on_argument_with_wildcard_column_negated(isAsync);

        [ConditionalTheory(Skip = "https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/996")]
        public override Task String_contains_on_argument_with_wildcard_constant(bool isAsync)
            => base.String_contains_on_argument_with_wildcard_constant(isAsync);

        [ConditionalTheory(Skip = "https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/996")]
        public override Task String_contains_on_argument_with_wildcard_parameter(bool isAsync)
            => base.String_contains_on_argument_with_wildcard_parameter(isAsync);

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
        }
    }
}
