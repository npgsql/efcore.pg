using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryNavigationsNpgsqlTest : QueryNavigationsTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public QueryNavigationsNpgsqlTest(
            NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        [Fact(Skip="https://github.com/aspnet/EntityFramework/issues/9039")]
        public override void Select_collection_navigation_simple() {}

        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/9039")]
        public override void Select_collection_navigation_multi_part() {}

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
