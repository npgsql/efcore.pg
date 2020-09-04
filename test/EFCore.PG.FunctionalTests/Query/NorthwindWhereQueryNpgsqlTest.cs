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
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

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
WHERE MAKE_DATE(DATE_PART('year', o.""OrderDate"")::INT, DATE_PART('month', o.""OrderDate"")::INT, 1) = TIMESTAMP '1996-09-11 00:00:00'");
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
WHERE MAKE_TIMESTAMP(DATE_PART('year', o.""OrderDate"")::INT, DATE_PART('month', o.""OrderDate"")::INT, 1, 0, 0, 0::double precision) = TIMESTAMP '1996-09-11 00:00:00'");
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

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
