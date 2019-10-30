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
    public class GroupByQueryNpgsqlTest : GroupByQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public GroupByQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory]  // https://github.com/aspnet/EntityFrameworkCore/pull/18675
        [MemberData(nameof(IsAsyncData))]
        public override Task GroupBy_aggregate_projecting_conditional_expression(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().GroupBy(o => o.OrderDate).Select(
                    g =>
                        new { g.Key, SomeValue = g.Count() == 0 ? 1 : g.Sum(o => o.OrderID % 2 == 0 ? 1 : 0) / g.Count() }),
                e => (e.Key, e.SomeValue));
        }

        [ConditionalTheory]  // https://github.com/aspnet/EntityFrameworkCore/pull/18675
        [MemberData(nameof(IsAsyncData))]
        public override Task GroupBy_with_order_by_skip_and_another_order_by(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Order>()
                    .OrderBy(o => o.CustomerID)
                    .ThenBy(o => o.OrderID)
                    .Skip(80)
                    .OrderBy(o => o.CustomerID)
                    .ThenBy(o => o.OrderID)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => g.Sum(o => o.OrderID))
            );
        }

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
