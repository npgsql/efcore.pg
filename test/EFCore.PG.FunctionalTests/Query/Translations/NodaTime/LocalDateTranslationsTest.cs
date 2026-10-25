using Microsoft.EntityFrameworkCore.TestModels.NodaTime;
using NodaTime;

namespace Microsoft.EntityFrameworkCore.Query.Translations.NodaTime;

public class LocalDateTranslationsTest : QueryTestBase<NodaTimeQueryNpgsqlFixture>
{
    public LocalDateTranslationsTest(NodaTimeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Add_Period(bool async)
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
    public async Task Subtract_LocalDate_column(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate2 - t.LocalDate == Period.FromDays(1)));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE age(n."LocalDate2"::timestamp, n."LocalDate"::timestamp) = INTERVAL 'P1D'
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
@date='Friday, 20 April 2018' (DbType = Date)

SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE age(n."LocalDate2"::timestamp, @date::timestamp) = INTERVAL 'P1D'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LessThan(bool async)
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
WHERE age(n."LocalDate2"::timestamp, DATE '2018-04-20'::timestamp) = INTERVAL 'P1D'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Subtract_LocalDate_years_months_days(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>()
                .Select(t => t.LocalDate - new LocalDate(2017, 3, 19))
                .Where(p => p.Years == 1 && p.Months == 1 && p.Days == 1));

        AssertSql(
            """
SELECT age(n."LocalDate"::timestamp, DATE '2017-03-19'::timestamp)
FROM "NodaTimeTypes" AS n
WHERE date_part('year', age(n."LocalDate"::timestamp, DATE '2017-03-19'::timestamp))::int = 1 AND date_part('month', age(n."LocalDate"::timestamp, DATE '2017-03-19'::timestamp))::int = 1 AND date_part('day', age(n."LocalDate"::timestamp, DATE '2017-03-19'::timestamp))::int = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Subtract_LocalDate_negative(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Select(t => new LocalDate(2017, 3, 19) - t.LocalDate));

        AssertSql(
            """
SELECT age(DATE '2017-03-19'::timestamp, n."LocalDate"::timestamp)
FROM "NodaTimeTypes" AS n
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Subtract_LocalDate_month_end(bool async)
    {
        // PostgreSQL age() returns 30 days for March 31 to April 30, whereas NodaTime returns one month.
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Select(t => t.LocalDate + Period.FromDays(10) - new LocalDate(2018, 3, 31)),
            expectedQuery: ss => ss.Set<NodaTimeTypes>().Select(t => Period.FromDays(30)));

        AssertSql(
            """
SELECT age(CAST(n."LocalDate" + INTERVAL 'P10D' AS timestamp without time zone), DATE '2018-03-31'::timestamp)
FROM "NodaTimeTypes" AS n
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Year(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate.Year == 2018));

        AssertSql(
            """
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE trunc(date_part('year', n."LocalDate") - 0.5)::int + 1 = 2018
""");
    }

    [ConditionalTheory]
    [InlineData(false, -100)]
    [InlineData(true, -100)]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    public async Task Year_astronomical(bool async, int year)
    {
        await using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                var entity = await context.NodaTimeTypes.SingleAsync();
                entity.LocalDate = new LocalDate(year, 12, 31);
                await context.SaveChangesAsync();
                Fixture.TestSqlLoggerFactory.Clear();

                var query = context.NodaTimeTypes.Select(t => t.LocalDate.Year);
                Assert.Equal(year, async ? await query.SingleAsync() : query.Single());

                AssertSql(
                    """
SELECT trunc(date_part('year', n."LocalDate") - 0.5)::int + 1
FROM "NodaTimeTypes" AS n
LIMIT 2
""");
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Month(bool async)
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
    public async Task DayOrYear(bool async)
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
    public async Task Day(bool async)
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
    public async Task Distance()
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task At(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate.At(new LocalTime(12, 30, 0)) == new LocalDateTime(2018, 4, 20, 12, 30, 0)));

AssertSql(
"""
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."LocalDate" + TIME '12:30:00' = TIMESTAMP '2018-04-20T12:30:00'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task AtMidnight(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate.AtMidnight() == new LocalDateTime(2018, 4, 20, 0, 0, 0)));

AssertSql(
"""
SELECT n."Id", n."DateInterval", n."Duration", n."Instant", n."InstantRange", n."Interval", n."LocalDate", n."LocalDate2", n."LocalDateRange", n."LocalDateTime", n."LocalTime", n."Long", n."OffsetTime", n."Period", n."TimeZoneId", n."ZonedDateTime"
FROM "NodaTimeTypes" AS n
WHERE n."LocalDate" + TIME '00:00:00' = TIMESTAMP '2018-04-20T00:00:00'
""");
    }

    private NodaTimeContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
