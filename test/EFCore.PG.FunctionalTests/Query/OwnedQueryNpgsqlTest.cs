using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // Disabled upstream, https://github.com/aspnet/EntityFrameworkCore/issues/15285
    internal class OwnedQueryNpgsqlTest : RelationalOwnedQueryTestBase<OwnedQueryNpgsqlTest.OwnedQueryNpgsqlFixture>
    {
        public OwnedQueryNpgsqlTest(OwnedQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class OwnedQueryNpgsqlFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
