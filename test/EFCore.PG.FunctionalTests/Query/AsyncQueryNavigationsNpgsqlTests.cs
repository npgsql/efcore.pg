using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncQueryNavigationsNpgsqlTests : AsyncQueryNavigationsTestBase<NorthwindQueryNpgsqlFixture>
    {
        public AsyncQueryNavigationsNpgsqlTests(NorthwindQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            // TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }
    }
}
