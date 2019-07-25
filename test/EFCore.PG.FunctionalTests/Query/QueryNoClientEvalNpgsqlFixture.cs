using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class QueryNoClientEvalNpgsqlFixture : NorthwindQueryNpgsqlFixture<NoopModelCustomizer>
    {
    }
}
