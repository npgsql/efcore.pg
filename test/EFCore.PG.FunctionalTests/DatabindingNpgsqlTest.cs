using Microsoft.EntityFrameworkCore;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class DatabindingNpgsqlTest : DatabindingTestBase<F1NpgsqlFixture>
    {
        public DatabindingNpgsqlTest(F1NpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
