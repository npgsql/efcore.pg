using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class AsyncSimpleQueryNpgsqlTest : AsyncSimpleQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public AsyncSimpleQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        #region Skipped tests
        [Fact(Skip = "Test skipped in EFCore (SqlServer/Sqlite)")]
        public override Task Projection_when_arithmetic_expressions() => null;

        [Fact(Skip = "Test skipped in EFCore (SqlServer/Sqlite)")]
        public override Task Projection_when_arithmetic_mixed() => null;

        [Fact(Skip = "Test skipped in EFCore (SqlServer/Sqlite)")]
        public override Task Projection_when_arithmetic_mixed_subqueries() => null;

        [Fact(Skip = "Fails on Npgsql because it does close in connecting state (https://github.com/npgsql/npgsql/issues/1127)")]
        public override Task Throws_on_concurrent_query_first() => null;

        #endregion
    }
}
