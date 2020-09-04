using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ManyToManyNoTrackingQueryNpgsqlTest
        : ManyToManyNoTrackingQueryRelationalTestBase<ManyToManyQueryNpgsqlFixture>
    {
        public ManyToManyNoTrackingQueryNpgsqlTest(ManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // TODO: #1232
        // protected override bool CanExecuteQueryString => true;
    }
}
