using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindSelectQueryNpgsqlTest : NorthwindSelectQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public NorthwindSelectQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // Remove after https://github.com/dotnet/efcore/pull/26285
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override Task Projection_AsEnumerable_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .OrderBy(c => c.CustomerID)
                    .Select(c => ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).OrderBy(o => o.OrderID).AsEnumerable())
                    .Where(e => e.Where(o => o.OrderID < 11000).Count() > 0)
                    .Select(e => e.Where(o => o.OrderID < 10750)),
                assertOrder: true,
                entryCount: 18);
        }

        public override async Task Select_datetime_DayOfWeek_component(bool async)
        {
            await base.Select_datetime_DayOfWeek_component(async);

            AssertSql(
                @"SELECT floor(date_part('dow', o.""OrderDate""))::INT
FROM ""Orders"" AS o");
        }

        public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
            => AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
