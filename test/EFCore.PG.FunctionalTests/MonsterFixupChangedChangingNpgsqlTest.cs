using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class MonsterFixupChangedChangingNpgsqlTest :
        MonsterFixupTestBase<MonsterFixupChangedChangingNpgsqlTest.MonsterFixupChangedChangingNpgsqlFixture>
    {
        public MonsterFixupChangedChangingNpgsqlTest(MonsterFixupChangedChangingNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class MonsterFixupChangedChangingNpgsqlFixture : MonsterFixupChangedChangingFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            protected override void OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(
                ModelBuilder builder)
            {
                base.OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(builder);

                builder.Entity<TMessage>().Property(e => e.MessageId).UseSerialColumn();
                builder.Entity<TProductPhoto>().Property(e => e.PhotoId).UseSerialColumn();
                builder.Entity<TProductReview>().Property(e => e.ReviewId).UseSerialColumn();
            }
        }
    }
}
