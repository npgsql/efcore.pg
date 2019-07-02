using Microsoft.EntityFrameworkCore.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // TODO: Disabled upstream
    internal class QueryNoClientEvalNpgsqlTest : QueryNoClientEvalTestBase<QueryNoClientEvalNpgsqlFixture>
    {
        public QueryNoClientEvalNpgsqlTest(QueryNoClientEvalNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
