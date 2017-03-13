using Microsoft.EntityFrameworkCore.Specification.Tests;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class AsTrackingNpgsqlTest : AsTrackingTestBase<NorthwindQueryNpgsqlFixture>
    {
        public AsTrackingNpgsqlTest(NorthwindQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
