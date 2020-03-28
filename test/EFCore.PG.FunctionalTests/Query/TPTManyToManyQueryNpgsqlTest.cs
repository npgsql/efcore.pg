using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class TPTManyToManyQueryNpgsqlTest : TPTManyToManyQueryRelationalTestBase<TPTManyToManyQueryNpgsqlFixture>
    {
        public TPTManyToManyQueryNpgsqlTest(TPTManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // TODO: #1232
        // protected override bool CanExecuteQueryString => true;

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/21895")]
        public override Task Select_skip_navigation_first_or_default(bool async)
            => base.Select_skip_navigation_first_or_default(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/21895")]
        public override Task Left_join_with_skip_navigation(bool async)
            => base.Left_join_with_skip_navigation(async);
    }
}
