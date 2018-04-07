using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlServiceCollectionExtensionsTest : RelationalServiceCollectionExtensionsTestBase
    {
        public NpgsqlServiceCollectionExtensionsTest()
            : base(NpgsqlTestHelpers.Instance)
        {
        }
    }
}
