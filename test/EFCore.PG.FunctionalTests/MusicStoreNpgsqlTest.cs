using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.MusicStore;
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

        public class MusicStoreNpgsqlFixture : MusicStoreFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<CartItem>().Property(s => s.DateCreated).HasColumnType("timestamp without time zone");
                modelBuilder.Entity<Order>().Property(s => s.OrderDate).HasColumnType("timestamp without time zone");
            }
        }
    }
}
