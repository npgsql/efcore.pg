using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class LoadNpgsqlTest : LoadTestBase<LoadNpgsqlTest.LoadNpgsqlFixture>
    {
        public LoadNpgsqlTest(LoadNpgsqlFixture fixture)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        protected override void RecordLog() => Sql = Fixture.TestSqlLoggerFactory.Sql;

        private string Sql { get; set; }

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
