using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindWhereQueryNpgsqlTest : NorthwindWhereQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public NorthwindWhereQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Date and time

        public override async Task Where_datetime_now(bool async)
        {
            await base.Where_datetime_now(async);

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00.0000000' (DbType = DateTime)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE now() <> @__myDatetime_0");
        }

        public override async Task Where_datetime_utcnow(bool async)
        {
            await base.Where_datetime_utcnow(async);

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00.0000000' (DbType = DateTimeOffset)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE now() AT TIME ZONE 'UTC' <> @__myDatetime_0");
        }

        public override async Task Where_datetime_today(bool async)
        {
            await base.Where_datetime_today(async);

            AssertSql(
                @"SELECT e.""EmployeeID"", e.""City"", e.""Country"", e.""FirstName"", e.""ReportsTo"", e.""Title""
FROM ""Employees"" AS e
WHERE date_trunc('day', now()) = date_trunc('day', now())");
        }

        public override async Task Where_datetime_date_component(bool async)
        {
            await base.Where_datetime_date_component(async);

            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00.0000000' (DbType = DateTime)

SELECT o.""OrderID"", o.""CustomerID"", o.""EmployeeID"", o.""OrderDate""
FROM ""Orders"" AS o
WHERE date_trunc('day', o.""OrderDate"") = @__myDatetime_0");
        }

        public override async Task Where_date_add_year_constant_component(bool async)
        {
            await base.Where_date_add_year_constant_component(async);

            AssertSql(
                @"SELECT o.""OrderID"", o.""CustomerID"", o.""EmployeeID"", o.""OrderDate""
FROM ""Orders"" AS o
WHERE date_part('year', o.""OrderDate"" + INTERVAL '-1 years')::INT = 1997");
        }

        public override async Task Where_datetime_year_component(bool async)
        {
            await base.Where_datetime_year_component(async);

            AssertSql(
                @"SELECT o.""OrderID"", o.""CustomerID"", o.""EmployeeID"", o.""OrderDate""
FROM ""Orders"" AS o
WHERE date_part('year', o.""OrderDate"")::INT = 1998");
        }

        public override async Task Where_datetime_month_component(bool async)
        {
            await base.Where_datetime_month_component(async);

            AssertSql(
                @"SELECT o.""OrderID"", o.""CustomerID"", o.""EmployeeID"", o.""OrderDate""
FROM ""Orders"" AS o
WHERE date_part('month', o.""OrderDate"")::INT = 4");
        }

        public override async Task Where_datetime_dayOfYear_component(bool async)
        {
            await base.Where_datetime_dayOfYear_component(async);

            AssertSql(
                @"SELECT o.""OrderID"", o.""CustomerID"", o.""EmployeeID"", o.""OrderDate""
FROM ""Orders"" AS o
WHERE date_part('doy', o.""OrderDate"")::INT = 68");
        }

        public override async Task Where_datetime_day_component(bool async)
        {
            await base.Where_datetime_day_component(async);

            AssertSql(
                @"SELECT o.""OrderID"", o.""CustomerID"", o.""EmployeeID"", o.""OrderDate""
FROM ""Orders"" AS o
WHERE date_part('day', o.""OrderDate"")::INT = 4");
        }

        public override async Task Where_datetime_hour_component(bool async)
        {
            await base.Where_datetime_hour_component(async);

            AssertSql(
                @"SELECT o.""OrderID"", o.""CustomerID"", o.""EmployeeID"", o.""OrderDate""
FROM ""Orders"" AS o
WHERE date_part('hour', o.""OrderDate"")::INT = 14");
        }

        public override async Task Where_datetime_minute_component(bool async)
        {
            await base.Where_datetime_minute_component(async);

            AssertSql(
                @"SELECT o.""OrderID"", o.""CustomerID"", o.""EmployeeID"", o.""OrderDate""
FROM ""Orders"" AS o
WHERE date_part('minute', o.""OrderDate"")::INT = 23");
        }

        public override async Task Where_datetime_second_component(bool async)
        {
            await base.Where_datetime_second_component(async);

            AssertSql(
                @"SELECT o.""OrderID"", o.""CustomerID"", o.""EmployeeID"", o.""OrderDate""
FROM ""Orders"" AS o
WHERE date_part('second', o.""OrderDate"")::INT = 44");
        }

        [Theory(Skip = "SQL translation not implemented, too annoying")]
        public override Task Where_datetime_millisecond_component(bool async)
            => base.Where_datetime_millisecond_component(async);

        [Theory(Skip = "Translation not implemented yet, #873")]
        public override Task Where_datetimeoffset_now_component(bool async)
            => base.Where_datetimeoffset_now_component(async);

        [Theory(Skip = "Translation not implemented yet, #873")]
        public override Task Where_datetimeoffset_utcnow_component(bool async)
            => base.Where_datetimeoffset_utcnow_component(async);

        #endregion Date and time

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_datetime_ctor1(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o =>
                    new DateTime(o.OrderDate.Value.Year, o.OrderDate.Value.Month, 1) == new DateTime(1996, 9, 11)));

            AssertSql(
                @"SELECT o.""OrderID"", o.""CustomerID"", o.""EmployeeID"", o.""OrderDate""
FROM ""Orders"" AS o
WHERE make_date(date_part('year', o.""OrderDate"")::INT, date_part('month', o.""OrderDate"")::INT, 1) = TIMESTAMP '1996-09-11 00:00:00'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_datetime_ctor2(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o =>
                    new DateTime(o.OrderDate.Value.Year, o.OrderDate.Value.Month, 1, 0, 0, 0) == new DateTime(1996, 9, 11)));

            AssertSql(
                @"SELECT o.""OrderID"", o.""CustomerID"", o.""EmployeeID"", o.""OrderDate""
FROM ""Orders"" AS o
WHERE make_timestamp(date_part('year', o.""OrderDate"")::INT, date_part('month', o.""OrderDate"")::INT, 1, 0, 0, 0::double precision) = TIMESTAMP '1996-09-11 00:00:00'");
        }

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
