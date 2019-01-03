using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    public class QueryFilterFuncletizationNpgsqlTest
        : QueryFilterFuncletizationTestBase<QueryFilterFuncletizationNpgsqlTest.QueryFilterFuncletizationNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public QueryFilterFuncletizationNpgsqlTest(
            QueryFilterFuncletizationNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        public class QueryFilterFuncletizationNpgsqlFixture : QueryFilterFuncletizationRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
