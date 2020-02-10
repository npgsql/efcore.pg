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
    public class NorthwindGroupByQueryNpgsqlTest : NorthwindGroupByQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public NorthwindGroupByQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // https://github.com/dotnet/efcore/pull/19855
        public override Task GroupBy_scalar_aggregate_in_set_operation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.CustomerID.StartsWith("F"))
                    .Select(c => new { c.CustomerID, Sequence = 0 })
                    .Union(ss.Set<Order>()
                        .GroupBy(o => o.CustomerID)
                        .Select(g => new
                        {
                            CustomerID = g.Key,
                            Sequence = 1
                        })),
                elementSorter: e => e.CustomerID + "/" + e.Sequence);
        }

        public override Task GroupBy_Property_Select_Count_with_predicate(bool async)
            => Assert.ThrowsAsync<InvalidOperationException>(
                () => base.GroupBy_Property_Select_Count_with_predicate(async));

        public override Task GroupBy_Property_Select_LongCount_with_predicate(bool async)
            => Assert.ThrowsAsync<InvalidOperationException>(
                () => base.GroupBy_Property_Select_LongCount_with_predicate(async));
    }
}
