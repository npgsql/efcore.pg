using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FiltersInheritanceNpgsqlTest : FiltersInheritanceTestBase<InheritanceNpgsqlFixture>
    {
        public FiltersInheritanceNpgsqlTest(InheritanceNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
