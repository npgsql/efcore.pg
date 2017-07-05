using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CompiledQueryNpgsqlTest : CompiledQueryTestBase<NorthwindQueryNpgsqlFixture>
    {
        public CompiledQueryNpgsqlTest(NorthwindQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
