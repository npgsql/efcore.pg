using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(w => w.Log(RelationalEventId.QueryClientEvaluationWarning));

            protected override void OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(
                ModelBuilder builder)
            {
                base.OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(builder);

                builder.Entity<TMessage>().Property(e => e.MessageId).UseNpgsqlSerialColumn();

                builder.Entity<TProduct>()
                    .OwnsOne(c => (TDimensions)c.Dimensions, db =>
                    {
                        db.Property(d => d.Depth).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Width).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Height).HasColumnType("decimal(18,2)");
                    });

                builder.Entity<TProductPhoto>().Property(e => e.PhotoId).UseNpgsqlSerialColumn();
                builder.Entity<TProductReview>().Property(e => e.ReviewId).UseNpgsqlSerialColumn();

                builder.Entity<TComputerDetail>()
                    .OwnsOne(c => (TDimensions)c.Dimensions, db =>
                    {
                        db.Property(d => d.Depth).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Width).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Height).HasColumnType("decimal(18,2)");
                    });
            }
        }
    }
}
