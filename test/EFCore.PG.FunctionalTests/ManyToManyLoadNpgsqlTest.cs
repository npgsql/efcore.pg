using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

public class ManyToManyLoadNpgsqlTest(ManyToManyLoadNpgsqlTest.ManyToManyLoadNpgsqlFixture fixture)
    : ManyToManyLoadTestBase<ManyToManyLoadNpgsqlTest.ManyToManyLoadNpgsqlFixture>(fixture)
{
    public class ManyToManyLoadNpgsqlFixture : ManyToManyLoadFixtureBase, ITestSqlLoggerFactory
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
            // supported.
            modelBuilder.Entity<EntityCompositeKey>()
                .Property(e => e.Key3)
                .HasColumnType("timestamp without time zone");

            modelBuilder
                .Entity<JoinOneSelfPayload>()
                .Property(e => e.Payload)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            modelBuilder
                .Entity<UnidirectionalEntityCompositeKey>()
                .Property(e => e.Key3)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            modelBuilder
                .Entity<UnidirectionalJoinOneSelfPayload>()
                .Property(e => e.Payload)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            modelBuilder
                .SharedTypeEntity<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                .IndexerProperty<string>("Payload")
                .HasDefaultValue("Generated");

            modelBuilder
                .Entity<JoinOneToThreePayloadFull>()
                .Property(e => e.Payload)
                .HasDefaultValue("Generated");
        }
    }
}
