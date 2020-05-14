using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindCompiledQueryNpgsqlTest : NorthwindCompiledQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public NorthwindCompiledQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // EF Core expects the following tests to fail, but Npgsql actually supports arrays so they pass
        public override void Query_with_array_parameter() {}
        public override Task Query_with_array_parameter_async() => Task.CompletedTask;

        // This one fails in a different way than what EF expects, since we do support array indexing,
        // but not over object arrays.
        public override void MakeBinary_does_not_throw_for_unsupported_operator() {}
    }
}
