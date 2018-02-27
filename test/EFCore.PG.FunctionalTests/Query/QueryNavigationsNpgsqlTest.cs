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
    }
}
