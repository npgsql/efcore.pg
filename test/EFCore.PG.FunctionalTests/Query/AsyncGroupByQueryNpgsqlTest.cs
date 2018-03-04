using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncGroupByQueryNpgsqlTest : AsyncGroupByQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public AsyncGroupByQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [Fact(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/11064")]
        public override Task OrderBy_Skip_GroupBy() => Task.CompletedTask;

        public override async Task GroupBy_Composite_Select_Average()
        {
            await base.GroupBy_Composite_Select_Average();

            AssertSql(
                @"SELECT AVG(CAST(o.""OrderID"" AS double precision))
FROM ""Orders"" AS o
GROUP BY o.""CustomerID"", o.""EmployeeID""");
        }

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
