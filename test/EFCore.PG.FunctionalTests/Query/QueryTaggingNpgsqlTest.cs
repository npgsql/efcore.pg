using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

// ReSharper disable StringLiteralTypo
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    public class QueryTaggingNpgsqlTest : QueryTaggingTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public QueryTaggingNpgsqlTest(
            NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}

