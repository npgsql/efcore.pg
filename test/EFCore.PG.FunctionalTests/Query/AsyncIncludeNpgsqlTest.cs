using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class AsyncIncludeNpgsqlTest : IncludeAsyncTestBase<IncludeNpgsqlFixture>
    {
        public AsyncIncludeNpgsqlTest(IncludeNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
