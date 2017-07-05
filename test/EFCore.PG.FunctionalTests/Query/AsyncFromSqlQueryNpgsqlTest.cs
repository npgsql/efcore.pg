using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncFromSqlQueryNpgsqlTest : AsyncFromSqlQueryTestBase<NorthwindQueryNpgsqlFixture>
    {
        public AsyncFromSqlQueryNpgsqlTest(NorthwindQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
