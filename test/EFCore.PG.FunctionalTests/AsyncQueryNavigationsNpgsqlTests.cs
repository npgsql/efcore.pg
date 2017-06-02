using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
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
