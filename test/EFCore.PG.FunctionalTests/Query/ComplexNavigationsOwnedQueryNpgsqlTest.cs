using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
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
