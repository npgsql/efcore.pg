using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncQueryNavigationsNpgsqlTests : AsyncQueryNavigationsTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public AsyncQueryNavigationsNpgsqlTests(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            // TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }
    }
}
