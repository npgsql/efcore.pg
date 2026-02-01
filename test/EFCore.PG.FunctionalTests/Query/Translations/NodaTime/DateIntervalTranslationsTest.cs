using Microsoft.EntityFrameworkCore.TestModels.NodaTime;
using NodaTime;

namespace Microsoft.EntityFrameworkCore.Query.Translations.NodaTime;

public class DateIntervalTranslationsTest : QueryTestBase<NodaTimeQueryNpgsqlFixture>
{
    public DateIntervalTranslationsTest(NodaTimeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Length(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.DateInterval.Length == 5));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE upper(n."DateInterval") - lower(n."DateInterval") = 5
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Start(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.DateInterval.Start == new LocalDate(2018, 4, 20)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE lower(n."DateInterval") = DATE '2018-04-20'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task End(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.DateInterval.End == new LocalDate(2018, 4, 24)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE CAST(upper(n."DateInterval") - INTERVAL 'P1D' AS date) = DATE '2018-04-24'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task End_Select(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Select(t => t.DateInterval.End));

        AssertSql(
            """
SELECT CAST(upper(n."DateInterval") - INTERVAL 'P1D' AS date)
FROM "NodaTimeTypes" AS n
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Contains_LocalDate(bool async)
    {
        var dateInterval = new DateInterval(new LocalDate(2018, 01, 01), new LocalDate(2020, 12, 25));

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => dateInterval.Contains(t.LocalDate)));

        AssertSql(
            """
@dateInterval='[2018-01-01, 2020-12-25]' (DbType = Object)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE @dateInterval @> n."LocalDate"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Contains_DateInterval(bool async)
    {
        var dateInterval = new DateInterval(new LocalDate(2018, 4, 22), new LocalDate(2018, 4, 24));

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.DateInterval.Contains(dateInterval)));

        AssertSql(
            """
@dateInterval='[2018-04-22, 2018-04-24]' (DbType = Object)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."DateInterval" @> @dateInterval
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Intersection(bool async)
    {
        var dateInterval = new DateInterval(new LocalDate(2018, 4, 22), new LocalDate(2018, 4, 26));

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.DateInterval.Intersection(dateInterval) == new DateInterval(new LocalDate(2018, 4, 22), new LocalDate(2018, 4, 24))));

        AssertSql(
            """
@dateInterval='[2018-04-22, 2018-04-26]' (DbType = Object)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."DateInterval" * @dateInterval = '[2018-04-22,2018-04-24]'::daterange
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Union(bool async)
    {
        var dateInterval = new DateInterval(new LocalDate(2018, 4, 22), new LocalDate(2018, 4, 26));

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.DateInterval.Union(dateInterval) == new DateInterval(new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 26))));

        AssertSql(
            """
@dateInterval='[2018-04-22, 2018-04-26]' (DbType = Object)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."DateInterval" + @dateInterval = '[2018-04-20,2018-04-26]'::daterange
""");
    }

    [ConditionalTheory]
    [MinimumPostgresVersion(14, 0)] // Multiranges were introduced in PostgreSQL 14
    [MemberData(nameof(IsAsyncData))]
    public async Task RangeAgg(bool async)
    {
        await using var context = CreateContext();

        var query = context.NodaTimeTypes
            .GroupBy(x => true)
            .Select(g => EF.Functions.RangeAgg(g.Select(x => x.DateInterval)));

        var union = async
            ? await query.SingleAsync()
            : query.Single();

        Assert.Equal([new(new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 24))], union);

        AssertSql(
            """
SELECT range_agg(n0."DateInterval")
FROM (
    SELECT n."DateInterval", TRUE AS "Key"
    FROM "NodaTimeTypes" AS n
) AS n0
GROUP BY n0."Key"
LIMIT 2
""");
    }

    [ConditionalTheory]
    [MinimumPostgresVersion(14, 0)] // range_intersect_agg was introduced in PostgreSQL 14
    [MemberData(nameof(IsAsyncData))]
    public async Task Intersect_aggregate(bool async)
    {
        await using var context = CreateContext();

        var query = context.NodaTimeTypes
            .GroupBy(x => true)
            .Select(g => EF.Functions.RangeIntersectAgg(g.Select(x => x.DateInterval)));

        var intersection = async
            ? await query.SingleAsync()
            : query.Single();

        Assert.Equal(new DateInterval(new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 24)), intersection);

        AssertSql(
            """
SELECT range_intersect_agg(n0."DateInterval")
FROM (
    SELECT n."DateInterval", TRUE AS "Key"
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
