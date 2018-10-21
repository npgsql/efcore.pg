using System.Linq;
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
            : base(fixture) => Fixture.TestSqlLoggerFactory.Clear();

        #region Overrides

        public override async Task Select_expression_date_add_year(bool isAsync)
        {
            await base.Select_expression_date_add_year(isAsync);

            AssertSql(
                @"SELECT (o.""OrderDate"" + INTERVAL '1 years') AS ""OrderDate""
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

SELECT (o.""OrderDate"" + CAST((@__years_0 || ' years') AS interval)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_month(bool isAsync)
        {
            await base.Select_expression_datetime_add_month(isAsync);

            AssertSql(
                @"SELECT (o.""OrderDate"" + INTERVAL '1 months') AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_hour(bool isAsync)
        {
            await base.Select_expression_datetime_add_hour(isAsync);

            AssertSql(
                @"SELECT (o.""OrderDate"" + INTERVAL '1 hours') AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_minute(bool isAsync)
        {
            await base.Select_expression_datetime_add_minute(isAsync);

            AssertSql(
                @"SELECT (o.""OrderDate"" + INTERVAL '1 mins') AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_datetime_add_second(bool isAsync)
        {
            await base.Select_expression_datetime_add_second(isAsync);

            AssertSql(
                @"SELECT (o.""OrderDate"" + INTERVAL '1 secs') AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");
        }

        #endregion

        #region PadLeft, PadRight

        [Fact]
        public void PadLeft_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.Customers.Select(x => x.Address.PadLeft(2)).ToArray();
                AssertContainsInSql("SELECT lpad(x.\"Address\", 2)");
            }
        }

        [Fact]
        public void PadLeft_char_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.Customers.Select(x => x.Address.PadLeft(2, 'a')).ToArray();
                AssertContainsInSql("SELECT lpad(x.\"Address\", 2, 'a')");
            }
        }

        [Fact]
        public void PadLeft_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var length = 2;
                var _ = ctx.Customers.Select(x => x.Address.PadLeft(length)).ToArray();
                AssertContainsInSql("SELECT lpad(x.\"Address\", @__length_0)");
            }
        }

        [Fact]
        public void PadLeft_char_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var length = 2;
                // ReSharper disable once ConvertToConstant.Local
                var character = 'a';
                var _ = ctx.Customers.Select(x => x.Address.PadLeft(length, character)).ToArray();
                AssertContainsInSql("SELECT lpad(x.\"Address\", @__length_0, @__character_1)");
            }
        }

        [Fact]
        public void PadRight_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.Customers.Select(x => x.Address.PadRight(2)).ToArray();
                AssertContainsInSql("SELECT rpad(x.\"Address\", 2)");
            }
        }

        [Fact]
        public void PadRight_char_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.Customers.Select(x => x.Address.PadRight(2, 'a')).ToArray();
                AssertContainsInSql("SELECT rpad(x.\"Address\", 2, 'a')");
            }
        }

        [Fact]
        public void PadRight_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var length = 2;
                var _ = ctx.Customers.Select(x => x.Address.PadRight(length)).ToArray();
                AssertContainsInSql("SELECT rpad(x.\"Address\", @__length_0)");
            }
        }

        [Fact]
        public void PadRight_char_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var length = 2;
                // ReSharper disable once ConvertToConstant.Local
                var character = 'a';
                var _ = ctx.Customers.Select(x => x.Address.PadRight(length, character)).ToArray();
                AssertContainsInSql("SELECT rpad(x.\"Address\", @__length_0, @__character_1)");
            }
        }

        #endregion

        #region Helpers

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        #endregion
    }
}
