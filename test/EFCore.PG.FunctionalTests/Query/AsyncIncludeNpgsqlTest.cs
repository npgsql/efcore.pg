using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    public class AsyncIncludeNpgsqlTest : IncludeAsyncTestBase<IncludeNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public AsyncIncludeNpgsqlTest(IncludeNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
