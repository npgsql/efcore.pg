using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public partial class SimpleQueryNpgsqlTest : SimpleQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public SimpleQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Select_expression_date_add_year()
        {
            base.Select_expression_date_add_year();

            AssertSql(
                @"SELECT o.""OrderDate"" + MAKE_INTERVAL(years => 1) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        [Fact]
        public void Select_expression_date_add_year_param()
        {
            var years = 2;

            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddYears(years)
                        }),
                e => e.OrderDate);

            AssertSql(
                @"@__years_0='2'

SELECT o.""OrderDate"" + MAKE_INTERVAL(years => @__years_0) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        // TODO: these have been added in the base class as part of
        // https://github.com/aspnet/EntityFrameworkCore/issues/10522
        [Fact]
        public void Select_expression_datetime_add_month()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddMonths(1)
                        }),
                e => e.OrderDate);

            AssertSql(
                @"SELECT o.""OrderDate"" + MAKE_INTERVAL(months => 1) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        [Fact]
        public void Select_expression_datetime_add_hour()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddHours(1)
                        }),
                e => e.OrderDate);

            AssertSql(
                @"SELECT o.""OrderDate"" + MAKE_INTERVAL(hours => 1) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        [Fact]
        public void Select_expression_datetime_add_minute()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddMinutes(1)
                        }),
                e => e.OrderDate);

            AssertSql(
                @"SELECT o.""OrderDate"" + MAKE_INTERVAL(mins => 1) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        [Fact]
        public void Select_expression_datetime_add_second()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddSeconds(1)
                        }),
                e => e.OrderDate);

            AssertSql(
                @"SELECT o.""OrderDate"" + MAKE_INTERVAL(secs => 1) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
