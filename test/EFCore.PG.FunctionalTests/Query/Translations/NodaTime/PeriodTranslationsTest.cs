using Microsoft.EntityFrameworkCore.TestModels.NodaTime;
using NodaTime;

namespace Microsoft.EntityFrameworkCore.Query.Translations.NodaTime;

public class PeriodTranslationsTest : QueryTestBase<NodaTimeQueryNpgsqlFixture>
{
    public PeriodTranslationsTest(NodaTimeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Years(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Years == 2018));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('year', n."Period")::int = 2018
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Months(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Months == 4));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('month', n."Period")::int = 4
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Days(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Days == 20));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('day', n."Period")::int = 20
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Hours(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Hours == 10));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('hour', n."Period")::int = 10
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Minutes(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Minutes == 31));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('minute', n."Period")::int = 31
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Seconds(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Seconds == 23));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE floor(date_part('second', n."Period"))::int = 23
""");
    }

    // PostgreSQL does not support extracting weeks from intervals
    [ConditionalFact]
    public Task Weeks_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => t.Period.Weeks == 0).ToListAsync());
    }

    [ConditionalFact]
    public Task Milliseconds_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => t.Period.Nanoseconds == 0).ToListAsync());
    }

    [ConditionalFact]
    public Task Nanoseconds_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => t.Period.Nanoseconds == 0).ToListAsync());
    }

    [ConditionalFact]
    public Task Ticks_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => t.Period.Ticks == 0).ToListAsync());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromYears(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromYears(t.Id).Years == 1));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('year', make_interval(years => n."Id"))::int = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromMonths(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromMonths(t.Id).Months == 1));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('month', make_interval(months => n."Id"))::int = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromWeeks(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromWeeks(t.Id).Days == 7),
            ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromWeeks(t.Id).Normalize().Days == 7));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('day', make_interval(weeks => n."Id"))::int = 7
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromDays(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromDays(t.Id).Days == 1));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('day', make_interval(days => n."Id"))::int = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromHours_int(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromHours(t.Id).Hours == 1));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('hour', make_interval(hours => n."Id"))::int = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromHours_long(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromHours(t.Long).Hours == 1));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('hour', make_interval(hours => n."Long"::int))::int = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromMinutes_int(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromMinutes(t.Id).Minutes == 1));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('minute', make_interval(mins => n."Id"))::int = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromMinutes_long(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromMinutes(t.Long).Minutes == 1));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('minute', make_interval(mins => n."Long"::int))::int = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromSeconds_int(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromSeconds(t.Id).Seconds == 1));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE floor(date_part('second', make_interval(secs => n."Id"::bigint::double precision)))::int = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromSeconds_long(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromSeconds(t.Long).Seconds == 1));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE floor(date_part('second', make_interval(secs => n."Long"::double precision)))::int = 1
""");
    }

    [ConditionalFact]
    public Task FromMilliseconds_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => Period.FromMilliseconds(t.Id).Seconds == 1).ToListAsync());
    }

    [ConditionalFact]
    public Task FromNanoseconds_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => Period.FromNanoseconds(t.Id).Seconds == 1).ToListAsync());
    }

    [ConditionalFact]
    public Task FromTicks_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => Period.FromNanoseconds(t.Id).Seconds == 1).ToListAsync());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task GroupBy_Property_Select_Sum_over_Period(bool async)
    {
        await using var ctx = CreateContext();

        // Note: Unlike Duration, Period can't be converted to total ticks (because its absolute time varies).
        var query = ctx.Set<NodaTimeTypes>()
            .GroupBy(o => o.Id)
            .Select(g => EF.Functions.Sum(g.Select(o => o.Period)));

        _ = async
            ? await query.ToListAsync()
            : query.ToList();

        AssertSql(
            """
SELECT sum(n."Period")
FROM "NodaTimeTypes" AS n
GROUP BY n."Id"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task GroupBy_Property_Select_Average_over_Period(bool async)
    {
        await using var ctx = CreateContext();

        // Note: Unlike Duration, Period can't be converted to total ticks (because its absolute time varies).
        var query = ctx.Set<NodaTimeTypes>()
            .GroupBy(o => o.Id)
            .Select(g => EF.Functions.Average(g.Select(o => o.Period)));

        _ = async
            ? await query.ToListAsync()
            : query.ToList();

        AssertSql(
            """
SELECT avg(n."Period")
FROM "NodaTimeTypes" AS n
GROUP BY n."Id"
""");
    }

    private NodaTimeContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
