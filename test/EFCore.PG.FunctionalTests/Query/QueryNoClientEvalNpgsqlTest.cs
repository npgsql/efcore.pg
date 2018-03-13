using Microsoft.EntityFrameworkCore.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class QueryNoClientEvalNpgsqlTest : QueryNoClientEvalTestBase<QueryNoClientEvalNpgsqlFixture>
    {
        public QueryNoClientEvalNpgsqlTest(QueryNoClientEvalNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
