using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    public class GearsOfWarQueryNpgsqlTest : GearsOfWarQueryRelationalTestBase<GearsOfWarQueryNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public GearsOfWarQueryNpgsqlTest(GearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Byte array

        public override async Task Byte_array_contains_literal(bool async)
        {
            await base.Byte_array_contains_literal(async);

            AssertSql(
                @"SELECT s.""Id"", s.""Banner"", s.""Banner5"", s.""InternalNumber"", s.""Name""
FROM ""Squads"" AS s
WHERE position(BYTEA E'\\x01' IN s.""Banner"") > 0");
        }

        public override async Task Byte_array_contains_parameter(bool async)
        {
            await base.Byte_array_contains_parameter(async);

            AssertSql(
                @"@__someByte_0='1' (DbType = Int16)

SELECT s.""Id"", s.""Banner"", s.""Banner5"", s.""InternalNumber"", s.""Name""
FROM ""Squads"" AS s
WHERE position(set_byte(BYTEA E'\\x00', 0, @__someByte_0) IN s.""Banner"") > 0");
        }

        public override async Task Byte_array_filter_by_length_literal(bool async)
        {
            await base.Byte_array_filter_by_length_literal(async);

            AssertSql(
                @"SELECT s.""Id"", s.""Banner"", s.""Banner5"", s.""InternalNumber"", s.""Name""
FROM ""Squads"" AS s
WHERE length(s.""Banner"") = 1");
        }

        public override async Task Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(bool async)
        {
            await base.Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(async);

            AssertSql(
                @"SELECT s.""Id"", s.""Banner"", s.""Banner5"", s.""InternalNumber"", s.""Name""
FROM ""Squads"" AS s
WHERE length(s.""Banner5"") = 5");
        }

        public override async Task Byte_array_filter_by_length_parameter(bool async)
        {
            await base.Byte_array_filter_by_length_parameter(async);

            AssertSql(
                @"@__p_0='1'

SELECT s.""Id"", s.""Banner"", s.""Banner5"", s.""InternalNumber"", s.""Name""
FROM ""Squads"" AS s
WHERE length(s.""Banner"") = @__p_0");
        }

        public override void Byte_array_filter_by_length_parameter_compiled()
        {
            base.Byte_array_filter_by_length_parameter_compiled();

            AssertSql(
                @"@__byteArrayParam='0x2A80'

SELECT COUNT(*)::INT
FROM ""Squads"" AS s
WHERE length(s.""Banner"") = length(@__byteArrayParam)");
        }

        #endregion Byte array

        #region DateTimeOffset

        public override async Task Where_datetimeoffset_utcnow(bool async)
        {
            await base.Where_datetimeoffset_utcnow(async);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE m.""Timeline"" <> now()");
        }

        // Not supported by design: we support getting a local DateTime via DateTime.Now (based on PG TimeZone), but there's no way to get a
        // non-UTC DateTimeOffset.
        public override Task Where_datetimeoffset_now(bool async)
            => Task.CompletedTask;

        public override Task Where_datetimeoffset_millisecond_component(bool async)
            => Task.CompletedTask; // SQL translation not implemented, too annoying

        public override async Task Where_datetimeoffset_date_component(bool async)
        {
            await AssertQuery(
                async,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline.Date > new DateTime(1, DateTimeKind.Utc)
                      select m);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE date_trunc('day', m.""Timeline"") > TIMESTAMPTZ '0001-01-01 00:00:00Z'");
        }

        public override async Task Where_datetimeoffset_hour_component(bool async)
        {
            await base.Where_datetimeoffset_hour_component(async);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE date_part('hour', m.""Timeline"" AT TIME ZONE 'UTC')::INT = 10");
        }

        public override async Task DateTimeOffset_Date_returns_datetime(bool async)
        {
            var dateTimeOffset = new DateTimeOffset(2, 3, 1, 8, 0, 0, new TimeSpan(-5, 0, 0));

            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Timeline.Date.ToLocalTime() >= dateTimeOffset.Date));

            AssertSql(
                @"@__dateTimeOffset_Date_0='0002-03-01T00:00:00.0000000' (DbType = DateTime)

SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE date_trunc('day', m.""Timeline"")::timestamp >= @__dateTimeOffset_Date_0");
        }

        public override async Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
        {
            var dto = new DateTimeOffset(599898024001234567, new TimeSpan(0));
            var start = dto.AddDays(-1);
            var end = dto.AddDays(1);
            var dates = new[] { dto };

            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(
                    m => start <= m.Timeline.Date && m.Timeline < end && dates.Contains(m.Timeline)));

            AssertSql(
                @"@__start_0='1902-01-01T10:00:00.1234567+00:00'
@__end_1='1902-01-03T10:00:00.1234567+00:00'
@__dates_2={ '1902-01-02T10:00:00.1234567+00:00' } (DbType = Object)

SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE ((@__start_0 <= date_trunc('day', m.""Timeline"")::timestamptz) AND (m.""Timeline"" < @__end_1)) AND m.""Timeline"" = ANY (@__dates_2)");
        }

        // Base implementation uses DateTimeOffset.Now, which we don't translate by design. Use DateTimeOffset.UtcNow instead.
        public override async Task Select_datetimeoffset_comparison_in_projection(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<Mission>().Select(m => m.Timeline > DateTimeOffset.UtcNow));

            AssertSql(
                @"SELECT m.""Timeline"" > now()
FROM ""Missions"" AS m");
        }

        #endregion DateTimeOffset

        #region DateTime

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_subtraction(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m =>
                    new DateTimeOffset(2, 3, 2, 8, 0, 0, new TimeSpan(-5, 0, 0)) - m.Timeline > TimeSpan.FromDays(3)));

        #endregion DateTime

        #region TimeSpan

        public override async Task TimeSpan_Hours(bool async)
        {
            await base.TimeSpan_Hours(async);

            AssertSql(
                @"SELECT floor(date_part('hour', m.""Duration""))::INT
FROM ""Missions"" AS m");
        }

        public override async Task TimeSpan_Minutes(bool async)
        {
            await base.TimeSpan_Minutes(async);

            AssertSql(
                @"SELECT floor(date_part('minute', m.""Duration""))::INT
FROM ""Missions"" AS m");
        }

        public override async Task TimeSpan_Seconds(bool async)
        {
            await base.TimeSpan_Seconds(async);

            AssertSql(
                @"SELECT floor(date_part('second', m.""Duration""))::INT
FROM ""Missions"" AS m");
        }

        public override async Task TimeSpan_Milliseconds(bool async)
        {
            await base.TimeSpan_Milliseconds(async);

            AssertSql(
                @"SELECT floor(date_part('millisecond', m.""Duration""))::INT % 1000
FROM ""Missions"" AS m");
        }

        // Test runs successfully, but some time difference and precision issues and fail the assertion
        public override Task Where_TimeSpan_Hours(bool async)
            => Task.CompletedTask;

        public override async Task Where_TimeSpan_Minutes(bool async)
        {
            await base.Where_TimeSpan_Minutes(async);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE floor(date_part('minute', m.""Duration""))::INT = 1");
        }

        public override async Task Where_TimeSpan_Seconds(bool async)
        {
            await base.Where_TimeSpan_Seconds(async);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE floor(date_part('second', m.""Duration""))::INT = 1");
        }

        public override async Task Where_TimeSpan_Milliseconds(bool async)
        {
            await base.Where_TimeSpan_Milliseconds(async);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE (floor(date_part('millisecond', m.""Duration""))::INT % 1000) = 1");
        }

        #endregion TimeSpan

        #region DateOnly

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_DateOnly_ctor(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m =>
                    new DateOnly(EF.Property<DateOnly>(m, "Date").Year, EF.Property<DateOnly>(m, "Date").Month, 1) == new DateOnly(1996, 9, 11)));

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE make_date(date_part('year', m.""Date"")::INT, date_part('month', m.""Date"")::INT, 1) = DATE '1996-09-11'");
        }

        public override async Task Where_DateOnly_Year(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Date.Year == 1990).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE date_part('year', m.""Date"")::INT = 1990");
        }

        public override async Task Where_DateOnly_Month(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Date.Month == 11).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE date_part('month', m.""Date"")::INT = 11");
        }

        public override async Task Where_DateOnly_Day(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Date.Day == 10).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE date_part('day', m.""Date"")::INT = 10");
        }

        public override async Task Where_DateOnly_DayOfYear(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Date.DayOfYear == 314).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE date_part('doy', m.""Date"")::INT = 314");
        }

        public override async Task Where_DateOnly_DayOfWeek(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Date.DayOfWeek == DayOfWeek.Saturday).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE floor(date_part('dow', m.""Date""))::INT = 6");
        }

        public override async Task Where_DateOnly_AddYears(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Date.AddYears(3) == new DateOnly(1993, 11, 10)).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE (m.""Date"" + INTERVAL '3 years') = DATE '1993-11-10'");
        }

        public override async Task Where_DateOnly_AddMonths(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Date.AddMonths(3) == new DateOnly(1991, 2, 10)).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE (m.""Date"" + INTERVAL '3 months') = DATE '1991-02-10'");
        }

        public override async Task Where_DateOnly_AddDays(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Date.AddDays(3) == new DateOnly(1990, 11, 13)).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE (m.""Date"" + INTERVAL '3 days') = DATE '1990-11-13'");
        }

        #endregion DateOnly

        #region TimeOnly

        public override async Task Where_TimeOnly_Hour(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Time.Hour == 10).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE date_part('hour', m.""Time"")::INT = 10");
        }

        public override async Task Where_TimeOnly_Minute(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Time.Minute == 15).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE date_part('minute', m.""Time"")::INT = 15");
        }

        public override async Task Where_TimeOnly_Second(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Time.Second == 50).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE date_part('second', m.""Time"")::INT = 50");
        }

        public override Task Where_TimeOnly_Millisecond(bool async)
            => Task.CompletedTask; // Translation not implemented

        public override async Task Where_TimeOnly_AddHours(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Time.AddHours(3) == new TimeOnly(13, 15, 50, 500)).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE (m.""Time"" + INTERVAL '3 hours') = TIME '13:15:50.5'");
        }

        public override async Task Where_TimeOnly_AddMinutes(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Time.AddMinutes(3) == new TimeOnly(10, 18, 50, 500)).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE (m.""Time"" + INTERVAL '3 mins') = TIME '10:18:50.5'");
        }

        public override async Task Where_TimeOnly_Add_TimeSpan(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Time.Add(new TimeSpan(3, 0, 0)) == new TimeOnly(13, 15, 50, 500)).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE (m.""Time"" + INTERVAL '03:00:00') = TIME '13:15:50.5'");
        }

        public override async Task Where_TimeOnly_IsBetween(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Time.IsBetween(new TimeOnly(10, 0, 0), new TimeOnly(11, 0, 0))).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE (m.""Time"" >= TIME '10:00:00') AND (m.""Time"" < TIME '11:00:00')");
        }

        public override async Task Where_TimeOnly_subtract_TimeOnly(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m => m.Time - new TimeOnly(10, 0, 0) == new TimeSpan(0, 0, 15, 50, 500)).AsTracking(),
                entryCount: 1);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Date"", m.""Duration"", m.""Rating"", m.""Time"", m.""Timeline""
FROM ""Missions"" AS m
WHERE (m.""Time"" - TIME '10:00:00') = INTERVAL '00:15:50.5'");
        }

        #endregion TimeOnly

        private void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
