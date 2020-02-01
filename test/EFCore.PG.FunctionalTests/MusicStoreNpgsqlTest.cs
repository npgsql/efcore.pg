using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class MusicStoreNpgsqlTest : MusicStoreTestBase<MusicStoreNpgsqlTest.MusicStoreNpgsqlFixture>
    {
        public MusicStoreNpgsqlTest(MusicStoreNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Some issue loading System.Runtime.CompilerServices.Unsafe, should go away by 5.0")]
        public override Task AddressAndPayment_RedirectToCompleteWhenSuccessful() => Task.CompletedTask;

        public class MusicStoreNpgsqlFixture : MusicStoreFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
