using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class OwnedEntityQueryNpgsqlTest : OwnedEntityQueryRelationalTestBase
    {
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
    }
}
