using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class GroupByQueryNpgsqlTest : GroupByQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public GroupByQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/16693")]
        public override Task GroupBy_aggregate_projecting_conditional_expression_based_on_group_key(bool isAsync) => null;

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
