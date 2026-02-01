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
WHERE (n."Instant" + INTERVAL '1 00:00:00') - n."Instant" = INTERVAL '1 00:00:00'
""");
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
WHERE n."Instant"::timestamptz = TIMESTAMPTZ '2018-04-20T10:31:33.666Z'
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
