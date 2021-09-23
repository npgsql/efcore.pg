using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindWhereQueryNpgsqlTest : NorthwindWhereQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public NorthwindWhereQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Date and time

        public override Task Where_datetime_now(bool async)
            => Task.CompletedTask; // See TimestampQueryTest

        public override Task Where_datetime_utcnow(bool async)
            => Task.CompletedTask; // See TimestampQueryTest

        public override async Task Where_datetime_today(bool async)
        {
            await base.Where_datetime_today(async);

            AssertSql(
                @"SELECT e.""EmployeeID"", e.""City"", e.""Country"", e.""FirstName"", e.""ReportsTo"", e.""Title""
FROM ""Employees"" AS e
WHERE date_trunc('day', now()::timestamp) = date_trunc('day', now()::timestamp)");
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

        public override Task Where_datetime_millisecond_component(bool async)
            => Task.CompletedTask; // SQL translation not implemented, too annoying

        public override Task Where_datetimeoffset_now_component(bool async)
            => Task.CompletedTask; // https://github.com/npgsql/efcore.pg/issues/873

        public override Task Where_datetimeoffset_utcnow_component(bool async)
            => Task.CompletedTask; // https://github.com/npgsql/efcore.pg/issues/873

        #endregion Date and time

        [ConditionalTheory(Skip = "#873")]
        public override Task Where_datetimeoffset_utcnow(bool async)
            => base.Where_datetimeoffset_utcnow(async);

        public override Task Where_collection_navigation_ToList_Contains(bool async)
        {
            var order = new Order { OrderID = 10248 };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).ToList())
                    .Where(e => e.Contains(order)),
                entryCount: 5);
        }

        public override Task Where_collection_navigation_ToArray_Contains(bool async)
        {
            var order = new Order { OrderID = 10248 };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(c => c.Orders.AsEnumerable().OrderBy(o => o.OrderID).ToArray())
                    .Where(e => e.Contains(order)),
                entryCount: 5);
        }

        public override Task Where_collection_navigation_AsEnumerable_Contains(bool async)
        {
            var order = new Order { OrderID = 10248 };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).AsEnumerable())
                    .Where(e => e.Contains(order)),
                entryCount: 5);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
