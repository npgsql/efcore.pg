using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    public class IncludeNpgsqlTest : IncludeTestBase<IncludeNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public IncludeNpgsqlTest(IncludeNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
