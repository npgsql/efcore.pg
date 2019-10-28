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

        [ConditionalFact(Skip = "https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/996")]
        public override void String_contains_on_argument_with_wildcard_column() {}

        [ConditionalFact(Skip = "https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/996")]
        public override void String_contains_on_argument_with_wildcard_column_negated() {}

        [ConditionalFact(Skip = "https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/996")]
        public override void String_contains_on_argument_with_wildcard_constant() {}

        [ConditionalFact(Skip = "https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/996")]
        public override void String_contains_on_argument_with_wildcard_parameter() {}

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
