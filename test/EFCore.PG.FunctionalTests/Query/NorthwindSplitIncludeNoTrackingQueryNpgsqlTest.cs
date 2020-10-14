using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindSplitIncludeNoTrackingQueryNpgsqlTest : NorthwindSplitIncludeNoTrackingQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public NorthwindSplitIncludeNoTrackingQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override Task Include_collection_with_last_no_orderby(bool async)
            => AssertTranslationFailedWithDetails(
                () => AssertLast(
                    async,
                    ss => ss.Set<Customer>().Include(c => c.Orders),
                    entryCount: 8
                ), RelationalStrings.MissingOrderingInSelectExpression);
    }
}
