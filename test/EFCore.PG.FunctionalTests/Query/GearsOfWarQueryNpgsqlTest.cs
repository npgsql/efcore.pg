using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQueryNpgsqlTest : GearsOfWarQueryTestBase<GearsOfWarQueryNpgsqlFixture>
    {
        public GearsOfWarQueryNpgsqlTest(GearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Where_datetimeoffset_minute_component()
        {
            // PostgreSQL has no datatype that corresponds to DateTimeOffset.
            // DateTimeOffset gets mapps to "timestamptz" which does not record the offset, so the values coming
            // back from the database aren't as expected.
        }
    }
}
