using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class NodaTimeQueryNpgsqlTest : QueryTestBase<NodaTimeQueryNpgsqlTest.NodaTimeQueryNpgsqlFixture>
{
    public NodaTimeQueryNpgsqlTest(NodaTimeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Operator(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate < new LocalDate(2018, 4, 21)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."LocalDate" < DATE '2018-04-21'
""");
    }

    #region Addition and subtraction

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Add_LocalDate_Period(bool async)
    {
        // Note: requires some special type inference logic because we're adding things of different types
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate + Period.FromMonths(1) > t.LocalDate));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."LocalDate" + INTERVAL 'P1M' > n."LocalDate"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Subtract_Instant(bool async)
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
    public async Task Subtract_LocalDateTime(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime + Period.FromDays(1) - t.LocalDateTime == Period.FromDays(1)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE (n."LocalDateTime" + INTERVAL 'P1D') - n."LocalDateTime" = INTERVAL 'P1D'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Subtract_ZonedDateTime(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime + Duration.FromDays(1) - t.ZonedDateTime == Duration.FromDays(1)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE (n."ZonedDateTime" + INTERVAL '1 00:00:00') - n."ZonedDateTime" = INTERVAL '1 00:00:00'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Subtract_LocalDate(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate2 - t.LocalDate == Period.FromDays(1)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE make_interval(days => n."LocalDate2" - n."LocalDate") = INTERVAL 'P1D'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Subtract_LocalDate_parameter(bool async)
    {
        var date = new LocalDate(2018, 4, 20);
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate2 - date == Period.FromDays(1)));

        AssertSql(
            """
@__date_0='Friday, 20 April 2018' (DbType = Date)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE make_interval(days => n."LocalDate2" - @__date_0) = INTERVAL 'P1D'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Subtract_LocalDate_constant(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate2 - new LocalDate(2018, 4, 20) == Period.FromDays(1)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE make_interval(days => n."LocalDate2" - DATE '2018-04-20') = INTERVAL 'P1D'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Subtract_LocalTime(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalTime + Period.FromHours(1) - t.LocalTime == Period.FromHours(1)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE (n."LocalTime" + INTERVAL 'PT1H') - n."LocalTime" = INTERVAL 'PT1H'
""");
    }

    #endregion

    #region LocalDateTime

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_Year(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Year == 2018));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('year', n."LocalDateTime")::int = 2018
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_Month(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Month == 4));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('month', n."LocalDateTime")::int = 4
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_DayOfYear(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.DayOfYear == 110));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('doy', n."LocalDateTime")::int = 110
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_Day(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Day == 20));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('day', n."LocalDateTime")::int = 20
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_Hour(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Hour == 10));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('hour', n."LocalDateTime")::int = 10
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_Minute(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Minute == 31));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('minute', n."LocalDateTime")::int = 31
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_Second(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Second == 33));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE floor(date_part('second', n."LocalDateTime"))::int = 33
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_Date(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Date == new LocalDate(2018, 4, 20)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."LocalDateTime"::date = DATE '2018-04-20'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_Time(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.TimeOfDay == new LocalTime(10, 31, 33, 666)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."LocalDateTime"::time = TIME '10:31:33.666'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_DayOfWeek(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.DayOfWeek == IsoDayOfWeek.Friday));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE CASE floor(date_part('dow', n."LocalDateTime"))::int
    WHEN 0 THEN 7
    ELSE floor(date_part('dow', n."LocalDateTime"))::int
END = 5
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_InZoneLeniently_ToInstant(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.LocalDateTime.InZoneLeniently(DateTimeZoneProviders.Tzdb["Europe/Berlin"]).ToInstant()
                    == new ZonedDateTime(new LocalDateTime(2018, 4, 20, 8, 31, 33, 666), DateTimeZone.Utc, Offset.Zero).ToInstant()));

        AssertSql(
            """
@__ToInstant_0='2018-04-20T08:31:33Z' (DbType = DateTime)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."LocalDateTime" AT TIME ZONE 'Europe/Berlin' = @__ToInstant_0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDateTime_InZoneLeniently_ToInstant_with_column_time_zone(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.LocalDateTime.InZoneLeniently(DateTimeZoneProviders.Tzdb[t.TimeZoneId]).ToInstant()
                    == new ZonedDateTime(new LocalDateTime(2018, 4, 20, 8, 31, 33, 666), DateTimeZone.Utc, Offset.Zero).ToInstant()));

        AssertSql(
            """
@__ToInstant_0='2018-04-20T08:31:33Z' (DbType = DateTime)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."LocalDateTime" AT TIME ZONE n."TimeZoneId" = @__ToInstant_0
""");
    }

    [ConditionalFact]
    public async Task LocalDateTime_Distance()
    {
        await using var context = CreateContext();
        var closest = await context.NodaTimeTypes
            .OrderBy(t => EF.Functions.Distance(t.LocalDateTime, new LocalDateTime(2018, 4, 1, 0, 0, 0))).FirstAsync();

        Assert.Equal(1, closest.Id);

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
ORDER BY n."LocalDateTime" <-> TIMESTAMP '2018-04-01T00:00:00' NULLS FIRST
LIMIT 1
""");
    }

    #endregion LocalDateTime

    #region LocalDate

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDate_Year(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate.Year == 2018));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('year', n."LocalDate")::int = 2018
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDate_Month(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate.Month == 4));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('month', n."LocalDate")::int = 4
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDate_DayOrYear(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate.DayOfYear == 110));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('doy', n."LocalDate")::int = 110
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalDate_Day(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate.Day == 20));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('day', n."LocalDate")::int = 20
""");
    }

    [ConditionalFact]
    public async Task LocalDate_Distance()
    {
        await using var context = CreateContext();
        var closest = await context.NodaTimeTypes.OrderBy(t => EF.Functions.Distance(t.LocalDate, new LocalDate(2018, 4, 1))).FirstAsync();

        Assert.Equal(1, closest.Id);

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
ORDER BY n."LocalDate" <-> DATE '2018-04-01' NULLS FIRST
LIMIT 1
""");
    }

    #endregion LocalDate

    #region LocalTime

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalTime_Hour(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalTime.Hour == 10));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('hour', n."LocalTime")::int = 10
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalTime_Minute(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalTime.Minute == 31));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('minute', n."LocalTime")::int = 31
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LocalTime_Second(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalTime.Second == 33));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE floor(date_part('second', n."LocalTime"))::int = 33
""");
    }

    #endregion LocalTime

    #region Period

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Period_Years(bool async)
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
    public async Task Period_Months(bool async)
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
    public async Task Period_Days(bool async)
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
    public async Task Period_Hours(bool async)
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
    public async Task Period_Minutes(bool async)
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
    public async Task Period_Seconds(bool async)
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
    public Task Period_Weeks_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => t.Period.Weeks == 0).ToListAsync());
    }

    [ConditionalFact]
    public Task Period_Milliseconds_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => t.Period.Nanoseconds == 0).ToListAsync());
    }

    [ConditionalFact]
    public Task Period_Nanoseconds_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => t.Period.Nanoseconds == 0).ToListAsync());
    }

    [ConditionalFact]
    public Task Period_Ticks_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => t.Period.Ticks == 0).ToListAsync());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Period_FromYears(bool async)
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
    public async Task Period_FromMonths(bool async)
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
    public async Task Period_FromWeeks(bool async)
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
    public async Task Period_FromDays(bool async)
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
    public async Task Period_FromHours_int(bool async)
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
    public async Task Period_FromHours_long(bool async)
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
    public async Task Period_FromMinutes_int(bool async)
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
    public async Task Period_FromMinutes_long(bool async)
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
    public async Task Period_FromSeconds_int(bool async)
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
    public async Task Period_FromSeconds_long(bool async)
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
    public Task Period_FromMilliseconds_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => Period.FromMilliseconds(t.Id).Seconds == 1).ToListAsync());
    }

    [ConditionalFact]
    public Task Period_FromNanoseconds_is_not_translated()
    {
        using var ctx = CreateContext();

        return AssertTranslationFailed(
            () => ctx.Set<NodaTimeTypes>().Where(t => Period.FromNanoseconds(t.Id).Seconds == 1).ToListAsync());
    }

    [ConditionalFact]
    public Task Period_FromTicks_is_not_translated()
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

    #endregion Period

    #region Duration

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Duration_TotalDays(bool async)
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
    public async Task Duration_TotalHours(bool async)
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
    public async Task Duration_TotalMinutes(bool async)
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
    public async Task Duration_TotalSeconds(bool async)
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
    public async Task Duration_TotalMilliseconds(bool async)
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
    public async Task Duration_Days(bool async)
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
    public async Task Duration_Hours(bool async)
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
    public async Task Duration_Minutes(bool async)
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
    public async Task Duration_Seconds(bool async)
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

    #endregion

    #region Interval

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
@__interval_0='2018-01-01T00:00:00Z/2020-12-25T00:00:00Z' (DbType = Object)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE @__interval_0 @> n."Instant"
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
        Assert.Equal(new Interval[] { new(start, start + Duration.FromDays(5)) }, union);

        AssertSql(
            """
SELECT range_agg(t."Interval")
FROM (
    SELECT n."Interval", TRUE AS "Key"
    FROM "NodaTimeTypes" AS n
) AS t
GROUP BY t."Key"
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
SELECT range_intersect_agg(t."Interval")
FROM (
    SELECT n."Interval", TRUE AS "Key"
    FROM "NodaTimeTypes" AS n
) AS t
GROUP BY t."Key"
LIMIT 2
""");
    }

    #endregion Interval

    #region DateInterval

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateInterval_Length(bool async)
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
    public async Task DateInterval_Start(bool async)
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
    public async Task DateInterval_End(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.DateInterval.End == new LocalDate(2018, 4, 24)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE upper(n."DateInterval") - INTERVAL 'P1D' = DATE '2018-04-24'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateInterval_Contains_LocalDate(bool async)
    {
        var dateInterval = new DateInterval(new LocalDate(2018, 01, 01), new LocalDate(2020, 12, 25));

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => dateInterval.Contains(t.LocalDate)));

        AssertSql(
            """
@__dateInterval_0='[2018-01-01, 2020-12-25]' (DbType = Object)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE @__dateInterval_0 @> n."LocalDate"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateInterval_Contains_DateInterval(bool async)
    {
        var dateInterval = new DateInterval(new LocalDate(2018, 4, 22), new LocalDate(2018, 4, 24));

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.DateInterval.Contains(dateInterval)));

        AssertSql(
            """
@__dateInterval_0='[2018-04-22, 2018-04-24]' (DbType = Object)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."DateInterval" @> @__dateInterval_0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateInterval_Intersection(bool async)
    {
        var dateInterval = new DateInterval(new LocalDate(2018, 4, 22), new LocalDate(2018, 4, 26));

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.DateInterval.Intersection(dateInterval) == new DateInterval(new LocalDate(2018, 4, 22), new LocalDate(2018, 4, 24))));

        AssertSql(
            """
@__dateInterval_0='[2018-04-22, 2018-04-26]' (DbType = Object)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."DateInterval" * @__dateInterval_0 = '[2018-04-22,2018-04-24]'::daterange
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateInterval_Union(bool async)
    {
        var dateInterval = new DateInterval(new LocalDate(2018, 4, 22), new LocalDate(2018, 4, 26));

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.DateInterval.Union(dateInterval) == new DateInterval(new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 26))));

        AssertSql(
            """
@__dateInterval_0='[2018-04-22, 2018-04-26]' (DbType = Object)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."DateInterval" + @__dateInterval_0 = '[2018-04-20,2018-04-26]'::daterange
""");
    }

    [ConditionalTheory]
    [MinimumPostgresVersion(14, 0)] // Multiranges were introduced in PostgreSQL 14
    [MemberData(nameof(IsAsyncData))]
    public async Task DateInterval_RangeAgg(bool async)
    {
        await using var context = CreateContext();

        var query = context.NodaTimeTypes
            .GroupBy(x => true)
            .Select(g => EF.Functions.RangeAgg(g.Select(x => x.DateInterval)));

        var union = async
            ? await query.SingleAsync()
            : query.Single();

        Assert.Equal(new DateInterval[] { new(new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 24)) }, union);

        AssertSql(
            """
SELECT range_agg(t."DateInterval")
FROM (
    SELECT n."DateInterval", TRUE AS "Key"
    FROM "NodaTimeTypes" AS n
) AS t
GROUP BY t."Key"
LIMIT 2
""");
    }

    [ConditionalTheory]
    [MinimumPostgresVersion(14, 0)] // range_intersect_agg was introduced in PostgreSQL 14
    [MemberData(nameof(IsAsyncData))]
    public async Task DateInterval_Intersect_aggregate(bool async)
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
SELECT range_intersect_agg(t."DateInterval")
FROM (
    SELECT n."DateInterval", TRUE AS "Key"
    FROM "NodaTimeTypes" AS n
) AS t
GROUP BY t."Key"
LIMIT 2
""");
    }

    #endregion DateInterval

    #region Range

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateRange_Contains(bool async)
    {
        var dateRange = new DateInterval(new LocalDate(2018, 01, 01), new LocalDate(2020, 12, 26));

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => dateRange.Contains(t.LocalDate)));

        AssertSql(
            """
@__dateRange_0='[2018-01-01, 2020-12-26]' (DbType = Object)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE @__dateRange_0 @> n."LocalDate"
""");
    }

    #endregion Range

    #region Instant

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Instance_InUtc(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.Instant.InUtc()
                    == new ZonedDateTime(new LocalDateTime(2018, 4, 20, 10, 31, 33, 666), DateTimeZone.Utc, Offset.Zero)));

        AssertSql(
            """
@__p_0='2018-04-20T10:31:33 UTC (+00)' (DbType = DateTime)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."Instant" = @__p_0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Instance_InZone_constant_LocalDateTime(bool async)
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
    public async Task Instance_InZone_constant_Date(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.Instant.InZone(DateTimeZoneProviders.Tzdb["Europe/Berlin"]).Date
                    == new LocalDate(2018, 4, 20)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE CAST(n."Instant" AT TIME ZONE 'Europe/Berlin' AS date) = DATE '2018-04-20'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Instance_InZone_parameter_LocalDateTime(bool async)
    {
        var timeZone = DateTimeZoneProviders.Tzdb["Europe/Berlin"];

        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.Instant.InZone(timeZone).LocalDateTime
                    == new LocalDateTime(2018, 4, 20, 12, 31, 33, 666)));

        AssertSql(
            """
@__timeZone_0='Europe/Berlin'

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."Instant" AT TIME ZONE @__timeZone_0 = TIMESTAMP '2018-04-20T12:31:33.666'
""");
    }

    [ConditionalFact]
    public async Task Instance_InZone_without_LocalDateTime_fails()
    {
        await using var ctx = CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.Set<NodaTimeTypes>().Where(t => t.Instant.InZone(DateTimeZoneProviders.Tzdb["Europe/Berlin"]) == default)
                .ToListAsync());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Instance_ToDateTimeUtc(bool async)
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
    public async Task Instant_Distance()
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

    #endregion

    #region ZonedDateTime

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ZonedDateTime_Year(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Year == 2018));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('year', n."ZonedDateTime" AT TIME ZONE 'UTC')::int = 2018
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ZonedDateTime_Month(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Month == 4));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('month', n."ZonedDateTime" AT TIME ZONE 'UTC')::int = 4
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ZonedDateTime_DayOfYear(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.DayOfYear == 110));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('doy', n."ZonedDateTime" AT TIME ZONE 'UTC')::int = 110
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ZonedDateTime_Day(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Day == 20));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('day', n."ZonedDateTime" AT TIME ZONE 'UTC')::int = 20
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ZonedDateTime_Hour(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Hour == 10));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('hour', n."ZonedDateTime" AT TIME ZONE 'UTC')::int = 10
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ZonedDateTime_Minute(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Minute == 31));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE date_part('minute', n."ZonedDateTime" AT TIME ZONE 'UTC')::int = 31
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ZonedDateTime_Second(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Second == 33));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE floor(date_part('second', n."ZonedDateTime" AT TIME ZONE 'UTC'))::int = 33
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ZonedDateTime_Date(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Date == new LocalDate(2018, 4, 20)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE CAST(n."ZonedDateTime" AT TIME ZONE 'UTC' AS date) = DATE '2018-04-20'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ZonedDateTime_DayOfWeek(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.DayOfWeek == IsoDayOfWeek.Friday));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE CASE floor(date_part('dow', n."ZonedDateTime" AT TIME ZONE 'UTC'))::int
    WHEN 0 THEN 7
    ELSE floor(date_part('dow', n."ZonedDateTime" AT TIME ZONE 'UTC'))::int
END = 5
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ZonedDateTime_LocalDateTime(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.Instant.InUtc().LocalDateTime == new LocalDateTime(2018, 4, 20, 10, 31, 33, 666)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."Instant" AT TIME ZONE 'UTC' = TIMESTAMP '2018-04-20T10:31:33.666'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ZonedDateTime_ToInstant(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(
                t => t.ZonedDateTime.ToInstant()
                    == new ZonedDateTime(new LocalDateTime(2018, 4, 20, 10, 31, 33, 666), DateTimeZone.Utc, Offset.Zero).ToInstant()));

        AssertSql(
            """
@__ToInstant_0='2018-04-20T10:31:33Z' (DbType = DateTime)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."ZonedDateTime" = @__ToInstant_0
""");
    }

    [ConditionalFact]
    public async Task ZonedDateTime_Distance()
    {
        await using var context = CreateContext();
        var closest = await context.NodaTimeTypes
            .OrderBy(
                t => EF.Functions.Distance(
                    t.ZonedDateTime,
                    new ZonedDateTime(new LocalDateTime(2018, 4, 1, 0, 0, 0), DateTimeZone.Utc, Offset.Zero))).FirstAsync();

        Assert.Equal(1, closest.Id);

        AssertSql(
            """
@__p_1='2018-04-01T00:00:00 UTC (+00)' (DbType = DateTime)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
ORDER BY n."ZonedDateTime" <-> @__p_1 NULLS FIRST
LIMIT 1
""");
    }

    #endregion ZonedDateTime

    #region Support

    private NodaTimeContext CreateContext()
        => Fixture.CreateContext();

    private static readonly Period _defaultPeriod = Period.FromYears(2018)
        + Period.FromMonths(4)
        + Period.FromDays(20)
        + Period.FromHours(10)
        + Period.FromMinutes(31)
        + Period.FromSeconds(23)
        + Period.FromMilliseconds(666);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class NodaTimeContext : PoolableDbContext
    {
        public NodaTimeContext(DbContextOptions<NodaTimeContext> options)
            : base(options)
        {
        }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DbSet<NodaTimeTypes> NodaTimeTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("btree_gist");
        }

        public static void Seed(NodaTimeContext context)
        {
            context.AddRange(NodaTimeData.CreateNodaTimeTypes());
            context.SaveChanges();
        }
    }

    public class NodaTimeTypes
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public int Id { get; set; }
        public Instant Instant { get; set; }
        public LocalDateTime LocalDateTime { get; set; }
        public ZonedDateTime ZonedDateTime { get; set; }
        public LocalDate LocalDate { get; set; }
        public LocalDate LocalDate2 { get; set; }
        public LocalTime LocalTime { get; set; }
        public OffsetTime OffsetTime { get; set; }
        public Period Period { get; set; }
        public Duration Duration { get; set; }
        public DateInterval DateInterval { get; set; }
        public NpgsqlRange<LocalDate> LocalDateRange { get; set; }
        public Interval Interval { get; set; }
        public NpgsqlRange<Instant> InstantRange { get; set; }
        public long Long { get; set; }

        public string TimeZoneId { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public class NodaTimeQueryNpgsqlFixture : SharedStoreFixtureBase<NodaTimeContext>, IQueryFixtureBase
    {
        protected override string StoreName
            => "NodaTimeTest";

#pragma warning disable CS0618 // GlobalTypeMapper is obsolete
        public NodaTimeQueryNpgsqlFixture()
        {
            NpgsqlConnection.GlobalTypeMapper.UseNodaTime();
        }
#pragma warning restore CS0618

        // Set the PostgreSQL TimeZone parameter to something local, to ensure that operations which take TimeZone into account
        // don't depend on the database's time zone, and also that operations which shouldn't take TimeZone into account indeed
        // don't.
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.WithConnectionStringOptions("-c TimeZone=Europe/Berlin");

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        private NodaTimeData _expectedData;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection).AddEntityFrameworkNpgsqlNodaTime();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);
            new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseNodaTime();

            return optionsBuilder;
        }

        protected override void Seed(NodaTimeContext context)
            => NodaTimeContext.Seed(context);

        public Func<DbContext> GetContextCreator()
            => CreateContext;

        public ISetSource GetExpectedData()
            => _expectedData ??= new NodaTimeData();

        public IReadOnlyDictionary<Type, object> EntitySorters
            => new Dictionary<Type, Func<object, object>> { { typeof(NodaTimeTypes), e => ((NodaTimeTypes)e)?.Id } }
                .ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> EntityAsserters
            => new Dictionary<Type, Action<object, object>>
            {
                {
                    typeof(NodaTimeTypes), (e, a) =>
                    {
                        Assert.Equal(e is null, a is null);
                        if (a is not null)
                        {
                            var ee = (NodaTimeTypes)e;
                            var aa = (NodaTimeTypes)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.LocalDateTime, aa.LocalDateTime);
                            Assert.Equal(ee.ZonedDateTime, aa.ZonedDateTime);
                            Assert.Equal(ee.Instant, aa.Instant);
                            Assert.Equal(ee.LocalDate, aa.LocalDate);
                            Assert.Equal(ee.LocalDate2, aa.LocalDate2);
                            Assert.Equal(ee.LocalTime, aa.LocalTime);
                            Assert.Equal(ee.OffsetTime, aa.OffsetTime);
                            Assert.Equal(ee.Period, aa.Period);
                            Assert.Equal(ee.Duration, aa.Duration);
                            Assert.Equal(ee.DateInterval, aa.DateInterval);
                            // Assert.Equal(ee.DateRange, aa.DateRange);
                            Assert.Equal(ee.Long, aa.Long);
                            Assert.Equal(ee.TimeZoneId, aa.TimeZoneId);
                        }
                    }
                }
            }.ToDictionary(e => e.Key, e => (object)e.Value);
    }

    private class NodaTimeData : ISetSource
    {
        private IReadOnlyList<NodaTimeTypes> NodaTimeTypes { get; }

        public NodaTimeData()
        {
            NodaTimeTypes = CreateNodaTimeTypes();
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(NodaTimeTypes))
            {
                return (IQueryable<TEntity>)NodaTimeTypes.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        public static IReadOnlyList<NodaTimeTypes> CreateNodaTimeTypes()
        {
            var localDateTime = new LocalDateTime(2018, 4, 20, 10, 31, 33, 666);
            var zonedDateTime = localDateTime.InUtc();
            var instant = zonedDateTime.ToInstant();
            var duration = Duration.FromMilliseconds(20)
                .Plus(Duration.FromSeconds(8))
                .Plus(Duration.FromMinutes(4))
                .Plus(Duration.FromHours(9))
                .Plus(Duration.FromDays(27));

            return new List<NodaTimeTypes>
            {
                new()
                {
                    Id = 1,
                    LocalDateTime = localDateTime,
                    ZonedDateTime = zonedDateTime,
                    Instant = instant,
                    LocalDate = localDateTime.Date,
                    LocalDate2 = localDateTime.Date + Period.FromDays(1),
                    LocalTime = localDateTime.TimeOfDay,
                    OffsetTime = new OffsetTime(new LocalTime(10, 31, 33, 666), Offset.Zero),
                    Period = _defaultPeriod,
                    Duration = duration,
                    DateInterval = new DateInterval(localDateTime.Date, localDateTime.Date.PlusDays(4)), // inclusive
                    LocalDateRange = new NpgsqlRange<LocalDate>(localDateTime.Date, localDateTime.Date.PlusDays(5)), // exclusive
                    Interval = new Interval(instant, instant + Duration.FromDays(5)),
                    InstantRange = new NpgsqlRange<Instant>(instant, true, instant + Duration.FromDays(5), false),
                    Long = 1,
                    TimeZoneId = "Europe/Berlin"
                }
            };
        }
    }

    #endregion Support
}
