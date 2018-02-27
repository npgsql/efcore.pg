using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsOwnedQueryNpgsqlTest : ComplexNavigationsOwnedQueryTestBase<ComplexNavigationsOwnedQueryNpgsqlFixture>
    {
        public ComplexNavigationsOwnedQueryNpgsqlTest(
            ComplexNavigationsOwnedQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
