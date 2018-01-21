using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FiltersNpgsqlTest : FiltersTestBase<NorthwindQueryNpgsqlFixture<NorthwindFiltersCustomizer>>
    {
        public FiltersNpgsqlTest(NorthwindQueryNpgsqlFixture<NorthwindFiltersCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
