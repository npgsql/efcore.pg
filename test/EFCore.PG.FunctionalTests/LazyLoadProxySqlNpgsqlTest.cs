using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    // ReSharper disable once UnusedMember.Global
    public class LazyLoadProxyNpgsqlTest : LazyLoadProxyTestBase<LazyLoadProxyNpgsqlTest.LoadNpgsqlFixture>
    {
        public LazyLoadProxyNpgsqlTest(LoadNpgsqlFixture fixture)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        [ConditionalFact]  // Requires MARS
        public override void Top_level_projection_track_entities_before_passing_to_client_method() {}

        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        protected override void RecordLog() => Sql = Fixture.TestSqlLoggerFactory.Sql;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        string Sql { get; set; }

        public class LoadNpgsqlFixture : LoadFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
