using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class WhereCaseInsensitiveNpgsqlTest : SimpleQueryTestBase<NorthwindCaseInsensitiveFixture<NoopModelCustomizer>>
    {
        public WhereCaseInsensitiveNpgsqlTest(NorthwindCaseInsensitiveFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        public override async Task Where_simple(bool isAsync)
        {
            await base.Where_simple(isAsync);

            AssertContainsSqlFragment("WHERE lower(c.\"City\") = lower('London')");
        }

        [Fact]
        public async Task Where_case_insensitive_results_should_be_equals()
        {
            using (var context = Fixture.CreateContext())
            {
                var small   = context.Customers.FirstOrDefault(x => x.City == "london");
                var medium  = context.Customers.FirstOrDefault(x => x.City == "loNDOn");
                var big     = context.Customers.FirstOrDefault(x => x.City == "LONDON");

                Assert.NotNull(small);
                Assert.NotNull(medium);
                Assert.NotNull(big);

                var id = small.CustomerID;

                Assert.All(new[] { small, medium, big }, customer => Assert.True(customer.CustomerID == id));
            }
        }

        void AssertContainsSqlFragment(string expectedFragment)
            => Assert.True(Fixture.TestSqlLoggerFactory.SqlStatements.Any(s => s.Contains(expectedFragment)));
    }
}
