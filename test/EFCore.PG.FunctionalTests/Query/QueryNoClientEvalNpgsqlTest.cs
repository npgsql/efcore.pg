using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class QueryNoClientEvalNpgsqlTest : QueryNoClientEvalTestBase<QueryNoClientEvalNpgsqlFixture>
    {
        public QueryNoClientEvalNpgsqlTest(QueryNoClientEvalNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/18674")]
        public override void Throws_when_join() {}

        [ConditionalFact(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/18674")]
        public override void Throws_when_group_join() {}
    }
}
