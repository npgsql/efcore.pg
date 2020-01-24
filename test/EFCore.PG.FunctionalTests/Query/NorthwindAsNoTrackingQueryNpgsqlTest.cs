using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindAsNoTrackingQueryNpgsqlTest : NorthwindAsNoTrackingQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public NorthwindAsNoTrackingQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }
    }
}
