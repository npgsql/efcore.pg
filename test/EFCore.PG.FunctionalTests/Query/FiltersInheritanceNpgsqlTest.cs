using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class FiltersInheritanceNpgsqlTest : FiltersInheritanceTestBase<FiltersInheritanceNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public FiltersInheritanceNpgsqlTest(FiltersInheritanceNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
