using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeOneToOneNpgsqlTest : IncludeOneToOneTestBase<IncludeOneToOneNpgsqlTest.OneToOneQueryNpgsqlFixture>
    {
        public IncludeOneToOneNpgsqlTest(OneToOneQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        private string Sql => Fixture.TestSqlLoggerFactory.SqlStatements.Last();

        public class OneToOneQueryNpgsqlFixture : OneToOneQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }
    }
}
