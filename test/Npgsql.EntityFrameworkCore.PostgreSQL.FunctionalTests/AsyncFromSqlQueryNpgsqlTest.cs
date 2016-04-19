using Microsoft.EntityFrameworkCore.FunctionalTests;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class AsyncFromSqlQueryNpgsqlTest : AsyncFromSqlQueryTestBase<NorthwindQueryNpgsqlFixture>
    {
        public AsyncFromSqlQueryNpgsqlTest(NorthwindQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
