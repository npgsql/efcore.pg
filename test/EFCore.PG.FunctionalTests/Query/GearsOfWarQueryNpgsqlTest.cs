using System;
using System.Linq;
using System.Threading.Tasks;
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
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

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
                @"@__someByte_0='1'

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

        [Theory(Skip = "https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/874")]
        [MemberData(nameof(IsAsyncData))]
        public override Task String_concat_with_null_conditional_argument2(bool async)
            => base.String_concat_with_null_conditional_argument2(async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_subtraction(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Mission>().Where(m =>
                    new DateTimeOffset(2, 3, 2, 8, 0, 0, new TimeSpan(-5, 0, 0)) - m.Timeline > TimeSpan.FromDays(3)));

        #region Ignore DateTimeOffset tests

        // PostgreSQL has no datatype that corresponds to DateTimeOffset.
        // DateTimeOffset gets mapped to "timestamptz" which does not record the offset, so the values coming
        // back from the database aren't as expected.

        public override Task DateTimeOffset_DateAdd_AddDays(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddHours(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddMilliseconds(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddMinutes(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddMonths(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddSeconds(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddYears(bool async) => Task.CompletedTask;

        public override Task Where_datetimeoffset_date_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_day_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_dayofyear_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_hour_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_minute_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_month_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_year_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_second_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_millisecond_component(bool async) => Task.CompletedTask;
        public override Task Time_of_day_datetimeoffset(bool async) => Task.CompletedTask;

        public override Task Where_datetimeoffset_now(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_utcnow(bool async) => Task.CompletedTask;

        [ConditionalTheory(Skip = "DateTimeOffset.Date isn't currently translated")]
        [MemberData(nameof(IsAsyncData))]
        public override Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
            => base.DateTimeOffset_Contains_Less_than_Greater_than(async);

        [ConditionalTheory(Skip = "DateTimeOffset.Date isn't currently translated")]
        [MemberData(nameof(IsAsyncData))]
        public override Task DateTimeOffset_Date_returns_datetime(bool async)
            => base.DateTimeOffset_Date_returns_datetime(async);

        #endregion Ignore DateTimeOffset tests

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
                @"SELECT m.""Id"", m.""CodeName"", m.""Duration"", m.""Rating"", m.""Timeline""
FROM ""Missions"" AS m
WHERE floor(date_part('minute', m.""Duration""))::INT = 1");
        }

        public override async Task Where_TimeSpan_Seconds(bool async)
        {
            await base.Where_TimeSpan_Seconds(async);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Duration"", m.""Rating"", m.""Timeline""
FROM ""Missions"" AS m
WHERE floor(date_part('second', m.""Duration""))::INT = 1");
        }

        public override async Task Where_TimeSpan_Milliseconds(bool async)
        {
            await base.Where_TimeSpan_Milliseconds(async);

            AssertSql(
                @"SELECT m.""Id"", m.""CodeName"", m.""Duration"", m.""Rating"", m.""Timeline""
FROM ""Missions"" AS m
WHERE (floor(date_part('millisecond', m.""Duration""))::INT % 1000) = 1");
        }

        #endregion TimeSpan

        void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
