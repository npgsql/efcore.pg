using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class MusicStoreNpgsqlTest : MusicStoreTestBase<MusicStoreNpgsqlTest.MusicStoreNpgsqlFixture>
    {
        public MusicStoreNpgsqlTest(MusicStoreNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class MusicStoreNpgsqlFixture : MusicStoreFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
