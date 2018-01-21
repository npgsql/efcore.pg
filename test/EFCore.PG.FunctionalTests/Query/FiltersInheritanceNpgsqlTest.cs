using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FiltersInheritanceNpgsqlTest : FiltersInheritanceTestBase<FiltersInheritanceNpgsqlFixture>
    {
        public FiltersInheritanceNpgsqlTest(FiltersInheritanceNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
