using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        public override async Task Select_expression_date_add_year(bool isAsync)
        {
            await base.Select_expression_date_add_year(isAsync);

            AssertSql(
                @"SELECT (o.""OrderDate"" + MAKE_INTERVAL(years => 1)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Select_expression_date_add_year_param(bool isAsync)
        {
            var years = 2;

            await AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddYears(years)
                        }),
                e => e.OrderDate);

            AssertSql(
                @"@__years_0='2'

SELECT (o.""OrderDate"" + MAKE_INTERVAL(years => @__years_0)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_month(bool isAsync)
        {
            await base.Select_expression_datetime_add_month(isAsync);

            AssertSql(
                @"SELECT (o.""OrderDate"" + MAKE_INTERVAL(months => 1)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_hour(bool isAsync)
        {
            await base.Select_expression_datetime_add_hour(isAsync);

            AssertSql(
                @"SELECT (o.""OrderDate"" + MAKE_INTERVAL(hours => 1)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_minute(bool isAsync)
        {
            await base.Select_expression_datetime_add_minute(isAsync);

            AssertSql(
                @"SELECT (o.""OrderDate"" + MAKE_INTERVAL(mins => 1)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_second(bool isAsync)
        {
            await base.Select_expression_datetime_add_second(isAsync);

            AssertSql(
                @"SELECT (o.""OrderDate"" + MAKE_INTERVAL(secs => 1)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
