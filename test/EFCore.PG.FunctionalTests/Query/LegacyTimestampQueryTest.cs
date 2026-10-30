using System.ComponentModel.DataAnnotations.Schema;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

#if DEBUG

namespace Microsoft.EntityFrameworkCore.Query
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
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
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
            await using var ctx = CreateContext();

            // Because we can't play around with the LegacyTimestampBehavior flag at the ADO level (different assembly already compile in
            // RELEASE), we need to make the ADO layer happy by sending a UTC DateTime - but it should be the same with non-UTC in legacy.
            var myDatetime = new DateTime(2015, 4, 10, 0, 0, 0, DateTimeKind.Utc);

            _ = await ctx.Entities.Where(c => DateTime.Now != myDatetime).ToListAsync();

            AssertSql(
                """
@myDatetime='2015-04-10T00:00:00.0000000Z' (DbType = DateTime)

SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeOffset", e."TimestamptzDateTime"
FROM "Entities" AS e
WHERE now() <> @myDatetime
""");
        }

        [ConditionalFact]
        public virtual async Task Where_datetime_utcnow()
        {
            await using var ctx = CreateContext();

            var myDatetime = new DateTime(2015, 4, 10);

            _ = await ctx.Entities.Where(c => DateTime.UtcNow != myDatetime).ToListAsync();

            AssertSql(
                """
@myDatetime='2015-04-10T00:00:00.0000000'

SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeOffset", e."TimestamptzDateTime"
FROM "Entities" AS e
WHERE now() AT TIME ZONE 'UTC' <> @myDatetime
""");
        }

        [ConditionalFact]
        public async Task Interval_arithmetic_on_timestamptz()
        {
            await using var ctx = CreateContext();

            // With the legacy timestamp behavior, a DateTime mapped to timestamptz is read as a local DateTime,
            // and e.g. AddDays(1) keeps the local time across a daylight saving transition; so does timestamptz + interval
            // in the session time zone (Europe/Berlin, which moves to daylight saving time on 2018-03-25).
            // So unlike without the legacy behavior, the arithmetic isn't performed in UTC.
            // DateTimeOffset elapsed-time arithmetic (e.g. AddDays) is still exact, so it's still performed in UTC
            // (see DateTimeOffset_AddMonths_keeps_the_local_offset for AddMonths/AddYears).
            var result = await ctx.Database.SqlQueryRaw<IntervalArithmetic>(
                    """
SELECT TIMESTAMPTZ '2018-03-24T11:00:00Z' AS "BeforeDst", TIMESTAMPTZ '2018-03-24T11:00:00Z' AS "BeforeDstOffset", TIMESTAMP '2018-03-24T12:00:00' AS "BeforeDstTimestamp", INTERVAL '1 day' AS "Interval"
""")
                .Select(
                    a => new
                    {
                        AddDays = a.BeforeDst.AddDays(1),
                        Add = a.BeforeDst + a.Interval,
                        OffsetAddDays = a.BeforeDstOffset.AddDays(1),
                        OffsetAdd = a.BeforeDstOffset + a.Interval,
                        TimestampAddDays = a.BeforeDstTimestamp.AddDays(1)
                    })
                .SingleAsync();

            // 12:00 in Europe/Berlin on both days, i.e. 23 hours later; values are read as UTC at the ADO.NET level (see above)
            Assert.Equal(new DateTime(2018, 3, 25, 10, 0, 0, DateTimeKind.Utc), result.AddDays);
            Assert.Equal(new DateTime(2018, 3, 25, 10, 0, 0, DateTimeKind.Utc), result.Add);
            Assert.Equal(new DateTimeOffset(2018, 3, 25, 11, 0, 0, TimeSpan.Zero), result.OffsetAddDays);
            Assert.Equal(new DateTimeOffset(2018, 3, 25, 11, 0, 0, TimeSpan.Zero), result.OffsetAdd);
            Assert.Equal(new DateTime(2018, 3, 25, 12, 0, 0), result.TimestampAddDays);

            AssertSql(
                """
SELECT m."BeforeDst" + INTERVAL '1 days' AS "AddDays", m."BeforeDst" + m."Interval" AS "Add", ((m."BeforeDstOffset" AT TIME ZONE 'UTC') + INTERVAL '1 days') AT TIME ZONE 'UTC' AS "OffsetAddDays", ((m."BeforeDstOffset" AT TIME ZONE 'UTC') + m."Interval") AT TIME ZONE 'UTC' AS "OffsetAdd", m."BeforeDstTimestamp" + INTERVAL '1 days' AS "TimestampAddDays"
FROM (
    SELECT TIMESTAMPTZ '2018-03-24T11:00:00Z' AS "BeforeDst", TIMESTAMPTZ '2018-03-24T11:00:00Z' AS "BeforeDstOffset", TIMESTAMP '2018-03-24T12:00:00' AS "BeforeDstTimestamp", INTERVAL '1 day' AS "Interval"
) AS m
LIMIT 2
""");
        }

        [ConditionalFact]
        public async Task DateTimeOffset_AddMonths_keeps_the_local_offset()
        {
            await using var ctx = CreateContext();

            // With the legacy timestamp behavior, a DateTimeOffset is read with the local offset, and AddMonths/AddYears add calendar
            // months in that offset, like timestamptz + interval in the session time zone (Europe/Berlin). E.g. 2018-01-31 00:30+01:00
            // plus one month is 2018-02-28 00:30+01:00, i.e. 2018-02-27 23:30Z, whereas in UTC it would be 2018-02-28 23:30Z.
            // AddDays adds an exact amount of time, which is the same in UTC.
            // Values are sent and read as UTC at the ADO.NET level (see above), i.e. 2018-01-30 23:30Z.
            var monthEnd = new DateTimeOffset(2018, 1, 30, 23, 30, 0, TimeSpan.Zero);

            var result = await ctx.Database.SqlQueryRaw<OffsetMonthEnd>(
                    """
SELECT TIMESTAMPTZ '2018-01-30T23:30:00Z' AS "MonthEnd"
""")
                .Select(
                    a => new
                    {
                        AddMonths = a.MonthEnd.AddMonths(1),
                        AddMonthsParameter = EF.Parameter(monthEnd).AddMonths(1),
                        AddDays = a.MonthEnd.AddDays(1)
                    })
                .SingleAsync();

            Assert.Equal(new DateTimeOffset(2018, 2, 27, 23, 30, 0, TimeSpan.Zero), result.AddMonths);
            Assert.Equal(new DateTimeOffset(2018, 2, 27, 23, 30, 0, TimeSpan.Zero), result.AddMonthsParameter);
            Assert.Equal(new DateTimeOffset(2018, 1, 31, 23, 30, 0, TimeSpan.Zero), result.AddDays);

            AssertSql(
                """
@monthEnd='2018-01-30T23:30:00.0000000+00:00' (DbType = DateTime)

SELECT m."MonthEnd" + INTERVAL '1 months' AS "AddMonths", @monthEnd + INTERVAL '1 months' AS "AddMonthsParameter", ((m."MonthEnd" AT TIME ZONE 'UTC') + INTERVAL '1 days') AT TIME ZONE 'UTC' AS "AddDays"
FROM (
    SELECT TIMESTAMPTZ '2018-01-30T23:30:00Z' AS "MonthEnd"
) AS m
LIMIT 2
""");
        }

        #region Support

        private class OffsetMonthEnd
        {
            public DateTimeOffset MonthEnd { get; set; }
        }

        private class IntervalArithmetic
        {
            [Column(TypeName = "timestamp with time zone")]
            public DateTime BeforeDst { get; set; }

            public DateTimeOffset BeforeDstOffset { get; set; }

            public DateTime BeforeDstTimestamp { get; set; }

            public TimeSpan Interval { get; set; }
        }

        private TimestampQueryContext CreateContext()
            => Fixture.CreateContext();

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class TimestampQueryContext(DbContextOptions options) : PoolableDbContext(options)
        {
            public DbSet<Entity> Entities { get; set; }
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
            protected override string StoreName
                => "TimestampQueryTest";

            // A session time zone with daylight saving time, see Interval_arithmetic_on_timestamptz
            protected override ITestStoreFactory TestStoreFactory
                => new NpgsqlTestStoreFactory(connectionStringOptions: "-c TimeZone=Europe/Berlin");

            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            public LegacyTimestampQueryFixture()
            {
                NpgsqlTypeMappingSource.LegacyTimestampBehavior = true;
            }

            public override ValueTask DisposeAsync()
            {
                NpgsqlTypeMappingSource.LegacyTimestampBehavior = false;
                return default;
            }

            protected override async Task SeedAsync(TimestampQueryContext context)
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

                await ctx.SaveChangesAsync();
            }
        }

        #endregion
    }

    [CollectionDefinition("LegacyTimestampQueryTest", DisableParallelization = true)]
    public class NodaTimeEventSourceTestCollection;
}

#endif
