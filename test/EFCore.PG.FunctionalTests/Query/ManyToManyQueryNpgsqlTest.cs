using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ManyToManyQueryNpgsqlTest : ManyToManyQueryRelationalTestBase<ManyToManyQueryNpgsqlFixture>
    {
        public ManyToManyQueryNpgsqlTest(ManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
