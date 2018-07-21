using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class AsyncGearsOfWarQuerySqlServerTest : AsyncGearsOfWarQueryTestBase<GearsOfWarQueryNpgsqlFixture>
    {
        public AsyncGearsOfWarQuerySqlServerTest(GearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
