using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlTypes;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NodaTimeTest : QueryTestBase<NodaTimeTest.NodaTimeFixture>
    {
        public NodaTimeTest(NodaTimeFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Operator(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate < new LocalDate(2018, 4, 21)),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE n.""LocalDate"" < DATE '2018-04-21'");
        }

        #region Addition and subtraction

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Add_LocalDate_Period(bool async)
        {
            // Note: requires some special type inference logic because we're adding things of different types
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate + Period.FromMonths(1) > t.LocalDate),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE (n.""LocalDate"" + INTERVAL 'P1M') > n.""LocalDate""");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Subtract_Instant(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Instant + Duration.FromDays(1) - t.Instant == Duration.FromDays(1)),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE ((n.""Instant"" + INTERVAL '1 00:00:00') - n.""Instant"") = INTERVAL '1 00:00:00'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Subtract_LocalDateTime(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime + Period.FromDays(1) - t.LocalDateTime == Period.FromDays(1)),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE ((n.""LocalDateTime"" + INTERVAL 'P1D') - n.""LocalDateTime"") = INTERVAL 'P1D'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Subtract_ZonedDateTime(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime + Duration.FromDays(1) - t.ZonedDateTime == Duration.FromDays(1)),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE ((n.""ZonedDateTime"" + INTERVAL '1 00:00:00') - n.""ZonedDateTime"") = INTERVAL '1 00:00:00'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Subtract_LocalDate(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate2 - t.LocalDate == Period.FromDays(1)),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE MAKE_INTERVAL(days => n.""LocalDate2"" - n.""LocalDate"") = INTERVAL 'P1D'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Subtract_LocalDate_parameter(bool async)
        {
            using var ctx = CreateContext();

            var date = new LocalDate(2018, 4, 20);
            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate2 - date == Period.FromDays(1)),
                entryCount: 1);

            AssertSql(
                @"@__date_0='Friday, 20 April 2018' (DbType = Date)

SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE MAKE_INTERVAL(days => n.""LocalDate2"" - @__date_0) = INTERVAL 'P1D'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Subtract_LocalDate_constant(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate2 - new LocalDate(2018, 4, 20) == Period.FromDays(1)),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE MAKE_INTERVAL(days => n.""LocalDate2"" - DATE '2018-04-20') = INTERVAL 'P1D'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Subtract_LocalTime(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalTime + Period.FromHours(1) - t.LocalTime == Period.FromHours(1)),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE ((n.""LocalTime"" + INTERVAL 'PT1H') - n.""LocalTime"") = INTERVAL 'PT1H'");
        }

        #endregion

        #region LocalDateTime members

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDateTime_Year(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Year == 2018),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('year', n.""LocalDateTime"")::INT = 2018");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDateTime_Month(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Month == 4),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('month', n.""LocalDateTime"")::INT = 4");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDateTime_DayOfYear(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.DayOfYear == 110),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('doy', n.""LocalDateTime"")::INT = 110");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDateTime_Day(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Day == 20),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('day', n.""LocalDateTime"")::INT = 20");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDateTime_Hour(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Hour == 10),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('hour', n.""LocalDateTime"")::INT = 10");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDateTime_Minute(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Minute == 31),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('minute', n.""LocalDateTime"")::INT = 31");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDateTime_Second(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Second == 33),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE FLOOR(DATE_PART('second', n.""LocalDateTime""))::INT = 33");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDateTime_Date(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.Date == new LocalDate(2018, 4, 20)),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_TRUNC('day', n.""LocalDateTime"") = DATE '2018-04-20'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDateTime_DayOfWeek(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDateTime.DayOfWeek == IsoDayOfWeek.Friday),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE CASE
    WHEN FLOOR(DATE_PART('dow', n.""LocalDateTime""))::INT = 0 THEN 7
    ELSE FLOOR(DATE_PART('dow', n.""LocalDateTime""))::INT
END = 5");
        }

        #endregion LocalDateTime members

        #region LocalDate members

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDate_Year(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate.Year == 2018),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('year', n.""LocalDate"")::INT = 2018");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDate_Month(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate.Month == 4),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('month', n.""LocalDate"")::INT = 4");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDate_DayOrYear(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate.DayOfYear == 110),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('doy', n.""LocalDate"")::INT = 110");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalDate_Day(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalDate.Day == 20),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('day', n.""LocalDate"")::INT = 20");
        }

        #endregion LocalDate members

        #region LocalTime members

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalTime_Hour(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalTime.Hour == 10),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('hour', n.""LocalTime"")::INT = 10");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalTime_Minute(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalTime.Minute == 31),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('minute', n.""LocalTime"")::INT = 31");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task LocalTime_Second(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.LocalTime.Second == 33),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE FLOOR(DATE_PART('second', n.""LocalTime""))::INT = 33");
        }

        #endregion LocalTime members

        #region Period

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_Years(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Years == 2018),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('year', n.""Period"")::INT = 2018");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_Months(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Months == 4),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('month', n.""Period"")::INT = 4");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_Days(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Days == 20),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('day', n.""Period"")::INT = 20");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_Hours(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Hours == 10),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('hour', n.""Period"")::INT = 10");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_Minutes(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Minutes == 31),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('minute', n.""Period"")::INT = 31");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_Seconds(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Period.Seconds == 23),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE FLOOR(DATE_PART('second', n.""Period""))::INT = 23");
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
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromYears(t.Id).Years == 1),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('year', MAKE_INTERVAL(years => n.""Id""))::INT = 1");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_FromMonths(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromMonths(t.Id).Months == 1),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('month', MAKE_INTERVAL(months => n.""Id""))::INT = 1");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_FromWeeks(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromWeeks(t.Id).Days == 7),
                ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromWeeks(t.Id).Normalize().Days == 7),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('day', MAKE_INTERVAL(weeks => n.""Id""))::INT = 7");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_FromDays(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromDays(t.Id).Days == 1),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('day', MAKE_INTERVAL(days => n.""Id""))::INT = 1");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_FromHours_int(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromHours(t.Id).Hours == 1),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('hour', MAKE_INTERVAL(hours => n.""Id""))::INT = 1");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_FromHours_long(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromHours(t.Long).Hours == 1),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('hour', MAKE_INTERVAL(hours => n.""Long""::INT))::INT = 1");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_FromMinutes_int(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromMinutes(t.Id).Minutes == 1),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('minute', MAKE_INTERVAL(mins => n.""Id""))::INT = 1");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_FromMinutes_long(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromMinutes(t.Long).Minutes == 1),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('minute', MAKE_INTERVAL(mins => n.""Long""::INT))::INT = 1");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_FromSeconds_int(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromSeconds(t.Id).Seconds == 1),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE FLOOR(DATE_PART('second', MAKE_INTERVAL(secs => n.""Id""::bigint::double precision)))::INT = 1");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Period_FromSeconds_long(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => Period.FromSeconds(t.Long).Seconds == 1),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE FLOOR(DATE_PART('second', MAKE_INTERVAL(secs => n.""Long""::double precision)))::INT = 1");
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

        #endregion Period

        #region Duration

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Duration_TotalDays(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.TotalDays > 27),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE (DATE_PART('epoch', n.""Duration"") / 86400.0) > 27.0");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Duration_TotalHours(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.TotalHours < 700),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE (DATE_PART('epoch', n.""Duration"") / 3600.0) < 700.0");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Duration_TotalMinutes(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.Minutes < 40000),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('minute', n.""Duration"")::INT < 40000");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Duration_TotalSeconds(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.TotalSeconds == 2365448.02),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE (DATE_PART('epoch', n.""Duration"") / 1.0) = 2365448.02");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Duration_TotalMilliseconds(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.TotalMilliseconds == 2365448020),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE (DATE_PART('epoch', n.""Duration"") / 0.001) = 2365448020.0");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Duration_Days(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.Days == 27),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('day', n.""Duration"")::INT = 27");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Duration_Hours(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.Hours == 9),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('hour', n.""Duration"")::INT = 9");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Duration_Minutes(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.Minutes == 4),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('minute', n.""Duration"")::INT = 4");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Duration_Seconds(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Duration.Seconds == 8),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE FLOOR(DATE_PART('second', n.""Duration""))::INT = 8");
        }

        #endregion

        #region Range

        [ConditionalFact]
        public void DateRange_Contains()
        {
            using var ctx = CreateContext();

            var _ = ctx.NodaTimeTypes.Single(t => t.DateRange.Contains(new LocalDate(2018, 4, 21)));
            Assert.Contains(@"n.""DateRange"" @> DATE '2018-04-21'", Sql);
        }

        #endregion Range

        #region Instant

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Instance_InUtc(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Instant.InUtc() == new ZonedDateTime(new LocalDateTime(2018, 4, 20, 10, 31, 33, 666), DateTimeZone.Utc, Offset.Zero)),
                entryCount: 1);

            AssertSql(
                @"@__p_0='2018-04-20T10:31:33 UTC (+00)' (DbType = DateTimeOffset)

SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE n.""Instant"" = @__p_0");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task GetCurrentInstant_from_Instance(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Instant < SystemClock.Instance.GetCurrentInstant()),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE n.""Instant"" < NOW()");
        }

        #endregion

        #region ZonedDateTime

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task ZonedDateTime_Year(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Year == 2018),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('year', n.""ZonedDateTime"" AT TIME ZONE 'UTC')::INT = 2018");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task ZonedDateTime_Month(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Month == 4),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('month', n.""ZonedDateTime"" AT TIME ZONE 'UTC')::INT = 4");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task ZonedDateTime_DayOfYear(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.DayOfYear == 110),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('doy', n.""ZonedDateTime"" AT TIME ZONE 'UTC')::INT = 110");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task ZonedDateTime_Day(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Day == 20),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('day', n.""ZonedDateTime"" AT TIME ZONE 'UTC')::INT = 20");

        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task ZonedDateTime_Hour(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Hour == 10),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('hour', n.""ZonedDateTime"" AT TIME ZONE 'UTC')::INT = 10");

        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task ZonedDateTime_Minute(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Minute == 31),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_PART('minute', n.""ZonedDateTime"" AT TIME ZONE 'UTC')::INT = 31");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task ZonedDateTime_Second(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Second == 33),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE FLOOR(DATE_PART('second', n.""ZonedDateTime"" AT TIME ZONE 'UTC'))::INT = 33");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task ZonedDateTime_Date(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.Date == new LocalDate(2018, 4, 20)),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE DATE_TRUNC('day', n.""ZonedDateTime"" AT TIME ZONE 'UTC') = DATE '2018-04-20'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task ZonedDateTime_DayOfWeek(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.ZonedDateTime.DayOfWeek == IsoDayOfWeek.Friday),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE CASE
    WHEN FLOOR(DATE_PART('dow', n.""ZonedDateTime"" AT TIME ZONE 'UTC'))::INT = 0 THEN 7
    ELSE FLOOR(DATE_PART('dow', n.""ZonedDateTime"" AT TIME ZONE 'UTC'))::INT
END = 5");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task ZonedDateTime_LocalDateTime(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<NodaTimeTypes>().Where(t => t.Instant.InUtc().LocalDateTime == new LocalDateTime(2018, 4, 20, 10, 31, 33, 666)),
                entryCount: 1);

            AssertSql(
                @"SELECT n.""Id"", n.""DateRange"", n.""Duration"", n.""Instant"", n.""LocalDate"", n.""LocalDate2"", n.""LocalDateTime"", n.""LocalTime"", n.""Long"", n.""OffsetTime"", n.""Period"", n.""ZonedDateTime""
FROM ""NodaTimeTypes"" AS n
WHERE n.""Instant"" AT TIME ZONE 'UTC' = TIMESTAMP '2018-04-20T10:31:33.666'");
        }

        #endregion ZonedDateTime

        #region Support

        private NodaTimeContext CreateContext()
        {
            var ctx = Fixture.CreateContext();

            // Set the PostgreSQL TimeZone parameter to something local, to ensure that operations which take TimeZone into account don't
            // depend on the database's time zone, and also that operations which shouldn't take TimeZone into account indeed don't.
            ctx.Database.BeginTransaction();
            ctx.Database.ExecuteSqlRaw("SET TimeZone='Europe/Berlin'");
            Fixture.TestSqlLoggerFactory.Clear();

            return ctx;
        }

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;

        private static Period _defaultPeriod = Period.FromYears(2018) + Period.FromMonths(4) + Period.FromDays(20) +
            Period.FromHours(10) + Period.FromMinutes(31) + Period.FromSeconds(23) +
            Period.FromMilliseconds(666);

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class NodaTimeContext : PoolableDbContext
        {
            public NodaTimeContext(DbContextOptions<NodaTimeContext> options) : base(options) {}

            // ReSharper disable once MemberHidesStaticFromOuterClass
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public DbSet<NodaTimeTypes> NodaTimeTypes { get; set; }

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
            public NpgsqlRange<LocalDate> DateRange { get; set; }
            public long Long { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Global
        }

        public class NodaTimeFixture : SharedStoreFixtureBase<NodaTimeContext>, IQueryFixtureBase
        {
            protected override string StoreName => "NodaTimeTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

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

            public IReadOnlyDictionary<Type, object> GetEntitySorters()
                => new Dictionary<Type, Func<object, object>> { { typeof(NodaTimeTypes), e => ((NodaTimeTypes)e)?.Id } }
                    .ToDictionary(e => e.Key, e => (object)e.Value);

            public IReadOnlyDictionary<Type, object> GetEntityAsserters()
                => new Dictionary<Type, Action<object, object>>
                {
                    {
                        typeof(NodaTimeTypes), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
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
                                // Assert.Equal(ee.DateRange, aa.DateRange);
                                Assert.Equal(ee.Long, aa.Long);
                            }
                        }
                    }
                }.ToDictionary(e => e.Key, e => (object)e.Value);
        }

        protected class NodaTimeData : ISetSource
        {
            public IReadOnlyList<NodaTimeTypes> NodaTimeTypes { get; }

            public NodaTimeData()
                => NodaTimeTypes = CreateNodaTimeTypes();

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
                        DateRange = new NpgsqlRange<LocalDate>(localDateTime.Date, localDateTime.Date.PlusDays(5)),
                        Duration = duration,
                        Long = 1
                    }
                };
            }
        }

        #endregion Support
    }
}
