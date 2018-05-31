using Microsoft.EntityFrameworkCore.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class IncludeAsyncNpgsqlTest : IncludeAsyncTestBase<IncludeNpgsqlFixture>
    {
        public IncludeAsyncNpgsqlTest(IncludeNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
