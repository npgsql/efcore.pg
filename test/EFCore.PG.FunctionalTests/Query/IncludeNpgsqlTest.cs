using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeNpgsqlTest : IncludeTestBase<IncludeNpgsqlFixture>
    {
        public IncludeNpgsqlTest(IncludeNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
