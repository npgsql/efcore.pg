using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
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
            => Fixture.TestSqlLoggerFactory.Clear();

        [Theory(Skip = "https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/874")]
        public override Task String_concat_with_null_conditional_argument2(bool isAsync) => Task.CompletedTask;

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

        #endregion Ignore DateTimeOffset tests
    }
}
