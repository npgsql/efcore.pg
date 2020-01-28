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
    public class GearsOfWarQueryNpgsqlTest : GearsOfWarQueryTestBase<GearsOfWarQueryNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public GearsOfWarQueryNpgsqlTest(GearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory(Skip = "#1225")]
        public override Task Byte_array_contains_literal(bool async)
            => base.Byte_array_contains_literal(async);

        [ConditionalTheory(Skip = "#1225")]
        public override Task Byte_array_contains_parameter(bool async)
            => base.Byte_array_contains_parameter(async);

        public override async Task Byte_array_filter_by_length_literal(bool async)
        {
            await base.Byte_array_filter_by_length_literal(async);

            AssertSql(
                @"SELECT s.""Id"", s.""Banner"", s.""Banner5"", s.""InternalNumber"", s.""Name""
FROM ""Squads"" AS s
WHERE LENGTH(s.""Banner"") = 1");
        }

        public override async Task Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(bool async)
        {
            await base.Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(async);

            AssertSql(
                @"SELECT s.""Id"", s.""Banner"", s.""Banner5"", s.""InternalNumber"", s.""Name""
FROM ""Squads"" AS s
WHERE LENGTH(s.""Banner5"") = 5");
        }

        public override async Task Byte_array_filter_by_length_parameter(bool async)
        {
            await base.Byte_array_filter_by_length_parameter(async);

            AssertSql(
                @"@__p_0='1'

SELECT s.""Id"", s.""Banner"", s.""Banner5"", s.""InternalNumber"", s.""Name""
FROM ""Squads"" AS s
WHERE LENGTH(s.""Banner"") = @__p_0");
        }

        public override void Byte_array_filter_by_length_parameter_compiled()
        {
            base.Byte_array_filter_by_length_parameter_compiled();

            AssertSql(
                @"@__byteArrayParam='0x2A80'

SELECT COUNT(*)::INT
FROM ""Squads"" AS s
WHERE LENGTH(s.""Banner"") = LENGTH(@__byteArrayParam)");
        }

        [Theory(Skip = "https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/874")]
        [MemberData(nameof(IsAsyncData))]
        public override Task String_concat_with_null_conditional_argument2(bool isAsync)
            => base.String_concat_with_null_conditional_argument2(isAsync);

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

        public override Task DateTimeOffset_DateAdd_AddDays(bool isAsync) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddHours(bool isAsync) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddMilliseconds(bool isAsync) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddMinutes(bool isAsync) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddMonths(bool isAsync) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddSeconds(bool isAsync) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddYears(bool isAsync) => Task.CompletedTask;

        public override Task Where_datetimeoffset_date_component(bool isAsync) => Task.CompletedTask;
        public override Task Where_datetimeoffset_day_component(bool isAsync) => Task.CompletedTask;
        public override Task Where_datetimeoffset_dayofyear_component(bool isAsync) => Task.CompletedTask;
        public override Task Where_datetimeoffset_hour_component(bool isAsync) => Task.CompletedTask;
        public override Task Where_datetimeoffset_minute_component(bool isAsync) => Task.CompletedTask;
        public override Task Where_datetimeoffset_month_component(bool isAsync) => Task.CompletedTask;
        public override Task Where_datetimeoffset_year_component(bool isAsync) => Task.CompletedTask;
        public override Task Where_datetimeoffset_second_component(bool isAsync) => Task.CompletedTask;
        public override Task Where_datetimeoffset_millisecond_component(bool isAsync) => Task.CompletedTask;
        public override Task Time_of_day_datetimeoffset(bool isAsync) => Task.CompletedTask;

        public override Task Where_datetimeoffset_now(bool isAsync) => Task.CompletedTask;
        public override Task Where_datetimeoffset_utcnow(bool isAsync) => Task.CompletedTask;

        [ConditionalTheory(Skip = "DateTimeOffset.Date isn't currently translated")]
        [MemberData(nameof(IsAsyncData))]
        public override Task DateTimeOffset_Contains_Less_than_Greater_than(bool isAsync)
            => base.DateTimeOffset_Contains_Less_than_Greater_than(isAsync);

        [ConditionalTheory(Skip = "DateTimeOffset.Date isn't currently translated")]
        [MemberData(nameof(IsAsyncData))]
        public override Task DateTimeOffset_Date_returns_datetime(bool async)
            => base.DateTimeOffset_Date_returns_datetime(async);

        #endregion Ignore DateTimeOffset tests

        void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
