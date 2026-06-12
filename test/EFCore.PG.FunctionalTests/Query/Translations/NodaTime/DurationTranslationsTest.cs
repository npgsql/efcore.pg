using Microsoft.EntityFrameworkCore.TestModels.NodaTime;
using NodaTime;

namespace Microsoft.EntityFrameworkCore.Query.Translations.NodaTime;

public class DurationTranslationsTest : QueryTestBase<NodaTimeQueryNpgsqlFixture>
{
    public DurationTranslationsTest(NodaTimeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TotalDays(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.TotalDays > 27));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('epoch', n."Duration") / 86400.0 > 27.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TotalHours(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.TotalHours < 700));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('epoch', n."Duration") / 3600.0 < 700.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TotalMinutes(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.TotalMinutes < 40000));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('epoch', n."Duration") / 60.0 < 40000.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TotalSeconds(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.TotalSeconds == 2365448.02));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('epoch', n."Duration") = 2365448.02
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TotalMilliseconds(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.TotalMilliseconds == 2365448020));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('epoch', n."Duration") / 0.001 = 2365448020.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Days(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.Days == 27));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('day', n."Duration")::int = 27
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Hours(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.Hours == 9));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('hour', n."Duration")::int = 9
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Minutes(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.Minutes == 4));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('minute', n."Duration")::int = 4
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Seconds(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.Seconds == 8));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE floor(date_part('second', n."Duration"))::int = 8
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task GroupBy_Property_Select_Sum_over_Duration(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<NodaTimeTypes>()
                .GroupBy(o => o.Id)
                .Select(g => EF.Functions.Sum(g.Select(o => o.Duration))),
            expectedQuery: ss => ss.Set<NodaTimeTypes>()
                .GroupBy(o => o.Id)
                .Select(g => (Duration?)Duration.FromTicks(g.Sum(o => o.Duration.TotalTicks))));

        AssertSql(
            """
SELECT sum(n."Duration")
FROM "NodaTimeTypes" AS n
GROUP BY n."Id"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task GroupBy_Property_Select_Average_over_Duration(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<NodaTimeTypes>()
                .GroupBy(o => o.Id)
                .Select(g => EF.Functions.Average(g.Select(o => o.Duration))),
            expectedQuery: ss => ss.Set<NodaTimeTypes>()
                .GroupBy(o => o.Id)
                .Select(g => (Duration?)Duration.FromTicks((long)g.Average(o => o.Duration.TotalTicks))));

        AssertSql(
            """
SELECT avg(n."Duration")
FROM "NodaTimeTypes" AS n
GROUP BY n."Id"
""");
    }

    private NodaTimeContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
