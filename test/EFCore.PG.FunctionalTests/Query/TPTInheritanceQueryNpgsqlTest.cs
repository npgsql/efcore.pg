using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class TPTInheritanceQueryNpgsqlTest : TPTInheritanceQueryTestBase<TPTInheritanceQueryNpgsqlFixture>
    {
        public TPTInheritanceQueryNpgsqlTest(TPTInheritanceQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/pull/21895")]
        public override void Can_insert_update_delete() {}
    }
}
