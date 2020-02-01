using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindQueryFiltersQueryNpgsqlTest : NorthwindQueryFiltersQueryTestBase<NorthwindQueryNpgsqlFixture<NorthwindQueryFiltersCustomizer>>
    {
        public NorthwindQueryFiltersQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NorthwindQueryFiltersCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
