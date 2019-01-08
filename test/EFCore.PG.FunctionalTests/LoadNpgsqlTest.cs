using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    // ReSharper disable once UnusedMember.Global
    public class LoadNpgsqlTest : LoadTestBase<LoadNpgsqlTest.LoadNpgsqlFixture>
    {
        public LoadNpgsqlTest(LoadNpgsqlFixture fixture)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        protected override void RecordLog() => Sql = Fixture.TestSqlLoggerFactory.Sql;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        string Sql { get; set; }

        public class LoadNpgsqlFixture : LoadFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(
                    c => c
                        .Log(RelationalEventId.QueryClientEvaluationWarning));
        }
    }
}
