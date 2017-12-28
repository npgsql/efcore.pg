using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class UpdatesNpgsqlTest : UpdatesRelationalTestBase<UpdatesNpgsqlFixture>
    {
        public UpdatesNpgsqlTest(UpdatesNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [Fact(Skip = "Not implemented")]
        public override void Identifiers_are_generated_correctly()
        {
            throw new System.NotImplementedException();
        }
    }
}
