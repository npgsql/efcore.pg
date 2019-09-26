using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    public class IncludeOneToOneNpgsqlTest : IncludeOneToOneTestBase<IncludeOneToOneNpgsqlTest.OneToOneQueryNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public IncludeOneToOneNpgsqlTest(OneToOneQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        public class OneToOneQueryNpgsqlFixture : OneToOneQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }
    }
}
