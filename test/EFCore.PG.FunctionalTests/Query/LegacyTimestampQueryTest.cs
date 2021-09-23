using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

#if DEBUG

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // Because we can't play around with the LegacyTimestampBehavior flag at the ADO level (different assembly already compile in
    // RELEASE), this test suite is limited to playing around at the EF Core level only.
    [Collection("LegacyTimestampQueryTest")]
    public class LegacyTimestampQueryTest : IClassFixture<LegacyTimestampQueryTest.LegacyTimestampQueryFixture>
    {
        protected LegacyTimestampQueryFixture Fixture { get; }

        // ReSharper disable once UnusedParameter.Local
        public LegacyTimestampQueryTest(LegacyTimestampQueryFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact]
        public void DateTime_maps_to_timestamp_by_default()
        {
            using var ctx = CreateContext();

            Assert.Equal(
                "timestamp without time zone",
                ctx.Model.GetEntityTypes().Single().GetProperty(nameof(Entity.TimestampDateTime)).GetColumnType());
        }

        [ConditionalFact]
        public virtual async Task Where_datetime_now()
        {
            using var ctx = CreateContext();

            // Because we can't play around with the LegacyTimestampBehavior flag at the ADO level (different assembly already compile in
            // RELEASE), we need to make the ADO layer happy by sending a UTC DateTime - but it should be the same with non-UTC in legacy.
            var myDatetime = new DateTime(2015, 4, 10, 0, 0, 0, DateTimeKind.Utc);

            _ = await ctx.Entities.Where(c => DateTime.Now != myDatetime).ToListAsync();

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00.0000000Z' (DbType = DateTimeOffset)

SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE now() <> @__myDatetime_0");
        }

        [ConditionalFact]
        public virtual async Task Where_datetime_utcnow()
        {
            using var ctx = CreateContext();

            var myDatetime = new DateTime(2015, 4, 10);

            _ = await ctx.Entities.Where(c => DateTime.UtcNow != myDatetime).ToListAsync();

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00.0000000' (DbType = DateTime)

SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE now() AT TIME ZONE 'UTC' <> @__myDatetime_0");
        }

        #region Support

        private TimestampQueryContext CreateContext()
            => Fixture.CreateContext();

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class TimestampQueryContext : PoolableDbContext
        {
            public DbSet<Entity> Entities { get; set; }

            public TimestampQueryContext(DbContextOptions options) : base(options) {}
        }

        public class Entity
        {
            public int Id { get; set; }
            [Column(TypeName = "timestamp with time zone")]
            public DateTime TimestamptzDateTime { get; set; }
            public DateTime TimestampDateTime { get; set; }
            public DateTimeOffset TimestampDateTimeOffset { get; set; }
        }

        public class LegacyTimestampQueryFixture : SharedStoreFixtureBase<TimestampQueryContext>
        {
            protected override string StoreName => "TimestampQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            public LegacyTimestampQueryFixture()
                => NpgsqlTypeMappingSource.LegacyTimestampBehavior = true;

            public override void Dispose()
                => NpgsqlTypeMappingSource.LegacyTimestampBehavior = false;

            protected override void Seed(TimestampQueryContext context)
            {
                using var ctx = CreateContext();

                var utcDateTime1 = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc);
                var utcDateTime2 = new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Utc);

                ctx.AddRange(
                    new Entity
                    {
                        Id = 1,
                        TimestamptzDateTime = utcDateTime1,
                        TimestampDateTime = utcDateTime1.ToLocalTime(),
                        TimestampDateTimeOffset = new DateTimeOffset(utcDateTime1)
                    },
                    new Entity
                    {
                        Id = 2,
                        TimestamptzDateTime = utcDateTime2,
                        TimestampDateTime = DateTime.SpecifyKind(utcDateTime2.ToLocalTime(), DateTimeKind.Unspecified),
                        TimestampDateTimeOffset = new DateTimeOffset(utcDateTime2)
                    });
                ctx.SaveChanges();
            }
        }

        #endregion
    }

    [CollectionDefinition("LegacyTimestampQueryTest", DisableParallelization = true)]
    public class EventSourceTestCollection {}
}

#endif
