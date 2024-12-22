using Microsoft.EntityFrameworkCore.TestModels.NodaTime;
using NodaTime;

namespace Microsoft.EntityFrameworkCore.Query.Translations.NodaTime;

public class IntervalTranslationsTest : QueryTestBase<NodaTimeQueryNpgsqlFixture>
{
    public IntervalTranslationsTest(NodaTimeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_Start(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>()
                .Where(t => t.Interval.Start == new LocalDateTime(2018, 4, 20, 10, 31, 33, 666).InUtc().ToInstant()));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE lower(n."Interval") = TIMESTAMPTZ '2018-04-20T10:31:33.666Z'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_End(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Interval.End == new LocalDateTime(2018, 4, 25, 10, 31, 33, 666).InUtc().ToInstant()));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE upper(n."Interval") = TIMESTAMPTZ '2018-04-25T10:31:33.666Z'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_HasStart(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Interval.HasStart));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE NOT (lower_inf(n."Interval"))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_HasEnd(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Interval.HasEnd));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE NOT (upper_inf(n."Interval"))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_Duration(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Interval.Duration == Duration.FromDays(5)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE upper(n."Interval") - lower(n."Interval") = INTERVAL '5 00:00:00'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_Contains_Instant(bool async)
    {
        var interval = new Interval(
            new LocalDateTime(2018, 01, 01, 0, 0, 0).InUtc().ToInstant(),
            new LocalDateTime(2020, 12, 25, 0, 0, 0).InUtc().ToInstant());

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => interval.Contains(t.Instant)));

        AssertSql(
            """
@interval='2018-01-01T00:00:00Z/2020-12-25T00:00:00Z' (DbType = Object)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE @interval @> n."Instant"
""");
    }

    [ConditionalTheory]
    [MinimumPostgresVersion(14, 0)] // Multiranges were introduced in PostgreSQL 14
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_RangeAgg(bool async)
    {
        await using var context = CreateContext();

        var query = context.NodaTimeTypes
            .GroupBy(x => true)
            .Select(g => EF.Functions.RangeAgg(g.Select(x => x.Interval)));

        var union = async
            ? await query.SingleAsync()
            : query.Single();

        var start = Instant.FromUtc(2018, 4, 20, 10, 31, 33).Plus(Duration.FromMilliseconds(666));
        Assert.Equal([new(start, start + Duration.FromDays(5))], union);

        AssertSql(
            """
SELECT range_agg(n0."Interval")
FROM (
    SELECT n."Interval", TRUE AS "Key"
    FROM "NodaTimeTypes" AS n
) AS n0
GROUP BY n0."Key"
LIMIT 2
""");
    }

    [ConditionalTheory]
    [MinimumPostgresVersion(14, 0)] // range_intersect_agg was introduced in PostgreSQL 14
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_Intersect_aggregate(bool async)
    {
        await using var context = CreateContext();

        var query = context.NodaTimeTypes
            .GroupBy(x => true)
            .Select(g => EF.Functions.RangeIntersectAgg(g.Select(x => x.Interval)));

        var intersection = async
            ? await query.SingleAsync()
            : query.Single();

        var start = Instant.FromUtc(2018, 4, 20, 10, 31, 33).Plus(Duration.FromMilliseconds(666));
        Assert.Equal(new Interval(start, start + Duration.FromDays(5)), intersection);

        AssertSql(
            """
SELECT range_intersect_agg(n0."Interval")
FROM (
    SELECT n."Interval", TRUE AS "Key"
    FROM "NodaTimeTypes" AS n
) AS n0
GROUP BY n0."Key"
LIMIT 2
""");
    }

    private NodaTimeContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
