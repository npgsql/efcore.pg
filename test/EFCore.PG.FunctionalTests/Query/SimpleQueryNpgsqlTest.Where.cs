using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public partial class SimpleQueryNpgsqlTest
    {
        public override async Task Where_datetime_now(bool isAsync)
        {
            await base.Where_datetime_now(isAsync);
            AssertContainsSqlFragment("WHERE NOW() <>");
        }

        public override async Task Where_datetime_utcnow(bool isAsync)
        {
            await base.Where_datetime_utcnow(isAsync);
            AssertContainsSqlFragment("WHERE NOW() AT TIME ZONE 'UTC' <>");
        }

        public override async Task Where_datetime_today(bool isAsync)
        {
            await base.Where_datetime_today(isAsync);
            AssertContainsSqlFragment("WHERE DATE_TRUNC('day', NOW()) ");
        }

        public override async Task Where_datetime_date_component(bool isAsync)
        {
            await base.Where_datetime_date_component(isAsync);
            AssertContainsSqlFragment("WHERE DATE_TRUNC('day', o.\"OrderDate\")");
        }

        public override async Task Where_datetime_year_component(bool isAsync)
        {
            await base.Where_datetime_year_component(isAsync);
            AssertContainsSqlFragment("DATE_PART('year', o.\"OrderDate\")");
        }

        public override async Task Where_datetime_month_component(bool isAsync)
        {
            await base.Where_datetime_month_component(isAsync);
            AssertContainsSqlFragment("DATE_PART('month', o.\"OrderDate\")");
        }

        public override async Task Where_datetime_dayOfYear_component(bool isAsync)
        {
            await base.Where_datetime_dayOfYear_component(isAsync);
            AssertContainsSqlFragment("DATE_PART('doy', o.\"OrderDate\")");
        }

        public override async Task Where_datetime_day_component(bool isAsync)
        {
            await base.Where_datetime_day_component(isAsync);
            AssertContainsSqlFragment("DATE_PART('day', o.\"OrderDate\")");
        }

        public override async Task Where_datetime_hour_component(bool isAsync)
        {
            await base.Where_datetime_hour_component(isAsync);
            AssertContainsSqlFragment("DATE_PART('hour', o.\"OrderDate\")");
        }

        public override async Task Where_datetime_minute_component(bool isAsync)
        {
            await base.Where_datetime_minute_component(isAsync);
            AssertContainsSqlFragment("DATE_PART('minute', o.\"OrderDate\")");
        }

        public override async Task Where_datetime_second_component(bool isAsync)
        {
            await base.Where_datetime_second_component(isAsync);
            AssertContainsSqlFragment("DATE_PART('second', o.\"OrderDate\")");
        }

        [Theory(Skip = "SQL translation not implemented, too annoying")]
        public override Task Where_datetime_millisecond_component(bool isAsync) => null;

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Where_datetime_dayOfWeek_component(bool isAsync)
        {
            await AssertQuery<Order>(
                isAsync,
                oc => oc.Where(o =>
                    o.OrderDate.Value.DayOfWeek == DayOfWeek.Tuesday),
                entryCount: 168);
            AssertContainsSqlFragment("WHERE CAST(FLOOR(DATE_PART('dow', o.\"OrderDate\")) AS int)");
        }

        public override async Task Where_string_indexof(bool isAsync)
        {
            await base.Where_string_indexof(isAsync);
            AssertContainsSqlFragment("WHERE ((STRPOS(c.\"City\", 'Sea') - 1) <> -1)");
        }

        [Theory(Skip = "Translation not implemented yet, #873")]
        public override Task Where_datetimeoffset_now_component(bool isAsync) => null;

        [Theory(Skip = "Translation not implemented yet, #873")]
        public override Task Where_datetimeoffset_utcnow_component(bool isAsync) => null;

        [Theory(Skip = "PostgreSQL only has log(x, base) over numeric, may be possible to cast back and forth though")]
        public override Task Where_math_log_new_base(bool isAsync) => null;

        [Theory(Skip = "Convert on DateTime not yet supported")]
        public override Task Convert_ToString(bool isAsync) => null;
    }
}
