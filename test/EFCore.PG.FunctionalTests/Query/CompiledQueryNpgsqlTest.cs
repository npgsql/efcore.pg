using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class CompiledQueryNpgsqlTest : CompiledQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public CompiledQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        // EF Core expects the following tests to fail, but Npgsql actually supports arrays so they pass
        public override void Query_with_array_parameter() {}
        public override Task Query_with_array_parameter_async() => Task.CompletedTask;
    }
}
