using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    public class GearsOfWarQueryNpgsqlTest : GearsOfWarQueryTestBase<GearsOfWarQueryNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public GearsOfWarQueryNpgsqlTest(GearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        // PostgreSQL has no datatype that corresponds to DateTimeOffset.
        // DateTimeOffset gets mapped to "timestamptz" which does not record the offset, so the values coming
        // back from the database aren't as expected.
        public override Task Where_datetimeoffset_hour_component(bool isAsync) => Task.CompletedTask;

        // PostgreSQL has no datatype that corresponds to DateTimeOffset.
        // DateTimeOffset gets mapped to "timestamptz" which does not record the offset, so the values coming
        // back from the database aren't as expected.
        public override Task Where_datetimeoffset_minute_component(bool isAsync) => Task.CompletedTask;

        // PostgreSQL has no datatype that corresponds to DateTimeOffset.
        // DateTimeOffset gets mapped to "timestamptz" which does not record the offset, so the values coming
        // back from the database aren't as expected.
        public override Task Time_of_day_datetimeoffset(bool isAsync) => Task.CompletedTask;
    }
}
