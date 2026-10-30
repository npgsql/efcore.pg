using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.TestModels.NodaTime;
using NodaTime;

namespace Microsoft.EntityFrameworkCore.Query.Translations.NodaTime;

public class InstantTranslationsTest : QueryTestBase<NodaTimeQueryNpgsqlFixture>
{
    public InstantTranslationsTest(NodaTimeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Subtract_Duration(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Instant + Duration.FromDays(1) - t.Instant == Duration.FromDays(1)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE (((n."Instant" AT TIME ZONE 'UTC') + INTERVAL '1 00:00:00') AT TIME ZONE 'UTC') - n."Instant" = INTERVAL '1 00:00:00'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Duration_arithmetic_on_timestamptz_with_precision_or_alias(bool async)
    {
        await using var ctx = CreateContext();

        // Europe/Berlin moves to daylight saving time on 2018-03-25, so arithmetic in the session time zone would be an hour off
        // NodaTime has no Duration + Instant operator, so the Duration is always on the right of the arithmetic.
        var duration = Duration.FromDays(1);
        var instant = Instant.FromUtc(2018, 3, 24, 12, 0);
        var query = ctx.Database.SqlQueryRaw<DurationArithmetic>(
                """
SELECT TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "Instant", TIMESTAMPTZ '2018-03-25T12:00:00Z' AS "ZonedDateTime", INTERVAL '1 day' AS "Duration"
""")
            .Select(
                a => new
                {
                    Instant = a.Instant + a.Duration,
                    ZonedDateTime = a.ZonedDateTime - a.Duration,
                    InstantParameter = a.Instant + duration,
                    ZonedDateTimeParameter = a.ZonedDateTime - duration,
                    // The parameter's mapping is inferred from the column on the other side of the comparison
                    InstantComparison = instant + a.Duration == a.ZonedDateTime.ToInstant()
                        && a.ZonedDateTime.ToInstant() == instant + a.Duration
                });

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal(Instant.FromUtc(2018, 3, 25, 12, 0), result.Instant);
        Assert.Equal(Instant.FromUtc(2018, 3, 24, 12, 0).InUtc(), result.ZonedDateTime);
        Assert.Equal(Instant.FromUtc(2018, 3, 25, 12, 0), result.InstantParameter);
        Assert.Equal(Instant.FromUtc(2018, 3, 24, 12, 0).InUtc(), result.ZonedDateTimeParameter);
        Assert.True(result.InstantComparison);

        AssertSql(
            """
@duration='1:00:00:00' (DbType = Object)
@instant='2018-03-24T12:00:00Z' (DbType = DateTime)

SELECT ((m."Instant" AT TIME ZONE 'UTC') + m."Duration") AT TIME ZONE 'UTC' AS "Instant", ((m."ZonedDateTime" AT TIME ZONE 'UTC') - m."Duration") AT TIME ZONE 'UTC' AS "ZonedDateTime", ((m."Instant" AT TIME ZONE 'UTC') + @duration) AT TIME ZONE 'UTC' AS "InstantParameter", ((m."ZonedDateTime" AT TIME ZONE 'UTC') - @duration) AT TIME ZONE 'UTC' AS "ZonedDateTimeParameter", ((@instant AT TIME ZONE 'UTC') + m."Duration") AT TIME ZONE 'UTC' = m."ZonedDateTime" AND m."ZonedDateTime" = ((@instant AT TIME ZONE 'UTC') + m."Duration") AT TIME ZONE 'UTC' AS "InstantComparison"
FROM (
    SELECT TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "Instant", TIMESTAMPTZ '2018-03-25T12:00:00Z' AS "ZonedDateTime", INTERVAL '1 day' AS "Duration"
) AS m
LIMIT 2
""");
    }

    private class DurationArithmetic
    {
        [Precision(3)]
        public Instant Instant { get; set; }

        [Column(TypeName = "timestamptz(3)")]
        public ZonedDateTime ZonedDateTime { get; set; }

        public Duration Duration { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task InUtc(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.Instant.InUtc()
                    == new ZonedDateTime(new LocalDateTime(2018, 4, 20, 10, 31, 33, 666), DateTimeZone.Utc, Offset.Zero)));

        AssertSql(
            """
@p='2018-04-20T10:31:33 UTC (+00)' (DbType = DateTime)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."Instant" = @p
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task InZone_constant_LocalDateTime(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.Instant.InZone(DateTimeZoneProviders.Tzdb["Europe/Berlin"]).LocalDateTime
                    == new LocalDateTime(2018, 4, 20, 12, 31, 33, 666)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."Instant" AT TIME ZONE 'Europe/Berlin' = TIMESTAMP '2018-04-20T12:31:33.666'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task InZone_constant_Date(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.Instant.InZone(DateTimeZoneProviders.Tzdb["Europe/Berlin"]).Date == new LocalDate(2018, 4, 20)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE CAST(n."Instant" AT TIME ZONE 'Europe/Berlin' AS date) = DATE '2018-04-20'
""");
    }

    [ConditionalFact] // #3831
    [MinimumPostgresVersion(12, 0)]
    public async Task DateTrunc_with_timezone()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Set<NodaTimeTypes>().Select(t => EF.Functions.DateTrunc("day", t.Instant, "Europe/Berlin")).ToListAsync();

        AssertSql(
            """
SELECT date_trunc('day', n."Instant", 'Europe/Berlin')
FROM "NodaTimeTypes" AS n
""");
    }

    [ConditionalFact] // #3831
    public async Task DateTrunc_without_timezone()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Set<NodaTimeTypes>().Select(t => EF.Functions.DateTrunc("day", t.Instant)).ToListAsync();

        AssertSql(
            """
SELECT date_trunc('day', n."Instant")
FROM "NodaTimeTypes" AS n
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task InZone_parameter_LocalDateTime(bool async)
    {
        var timeZone = DateTimeZoneProviders.Tzdb["Europe/Berlin"];

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.Instant.InZone(timeZone).LocalDateTime == new LocalDateTime(2018, 4, 20, 12, 31, 33, 666)));

        AssertSql(
            """
@timeZone='Europe/Berlin'

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."Instant" AT TIME ZONE @timeZone = TIMESTAMP '2018-04-20T12:31:33.666'
""");
    }

    [ConditionalFact]
    public async Task InZone_without_LocalDateTime_fails()
    {
        await using var ctx = CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.Set<NodaTimeTypes>().Where(t => t.Instant.InZone(DateTimeZoneProviders.Tzdb["Europe/Berlin"]) == default)
                .ToListAsync());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ToDateTimeUtc(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>()
                .Where(t => t.Instant.ToDateTimeUtc() == new DateTime(2018, 4, 20, 10, 31, 33, 666, DateTimeKind.Utc)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."Instant" = TIMESTAMPTZ '2018-04-20T10:31:33.666Z'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task GetCurrentInstant_from_Instance(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Instant < SystemClock.Instance.GetCurrentInstant()));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."Instant" < NOW()
""");
    }

    [ConditionalFact]
    public async Task Distance()
    {
        await using var context = CreateContext();
        var closest = await context.NodaTimeTypes
            .OrderBy(t => EF.Functions.Distance(t.Instant, new LocalDateTime(2018, 4, 1, 0, 0, 0).InUtc().ToInstant())).FirstAsync();

        Assert.Equal(1, closest.Id);

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
ORDER BY n."Instant" <-> TIMESTAMPTZ '2018-04-01T00:00:00Z' NULLS FIRST
LIMIT 1
""");
    }

    private NodaTimeContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
