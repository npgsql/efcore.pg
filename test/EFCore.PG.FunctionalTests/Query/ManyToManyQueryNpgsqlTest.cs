using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ManyToManyQueryNpgsqlTest : ManyToManyQueryRelationalTestBase<ManyToManyQueryNpgsqlTest.ManyToManyQueryNpgsqlFixture>
    {
        public ManyToManyQueryNpgsqlTest(ManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/21636")]
        public override Task Join_with_skip_navigation(bool async)
            => base.Join_with_skip_navigation(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/21636")]
        public override Task Left_join_with_skip_navigation(bool async)
            => base.Left_join_with_skip_navigation(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/21636")]
        public override Task Skip_navigation_long_count_with_predicate(bool async)
            => base.Left_join_with_skip_navigation(async);

        public class ManyToManyQueryNpgsqlFixture : ManyToManyQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
