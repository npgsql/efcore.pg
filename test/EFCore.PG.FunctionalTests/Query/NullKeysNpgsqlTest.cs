using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NullKeysNpgsqlTest : NullKeysTestBase<NullKeysNpgsqlTest.NullKeysNpgsqlFixture>
    {
        public NullKeysNpgsqlTest(NullKeysNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysNpgsqlFixture : NullKeysFixtureBase
        {
            protected override string StoreName { get; } = "StringsContext";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
