using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    // "Part 1 of the migration to 2.1-preview1"

    //    public class LoadNpgsqlTest
    //        : LoadTestBase<LoadNpgsqlTest.LoadNpgsqlFixture>
    //    {
    //        public LoadNpgsqlTest(LoadNpgsqlFixture fixture)
    //            : base(fixture)
    //        {
    //            fixture.TestSqlLoggerFactory.Clear();
    //        }

    //        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

    //        protected override void RecordLog() => Sql = Fixture.TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);

    //        private const string FileLineEnding = @"
    //";

    //        private string Sql { get; set; }

    //        public class LoadNpgsqlFixture : LoadFixtureBase
    //        {
    //            protected override string StoreName { get; } = "LoadTest";

    //            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
    //            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
    //        }
    //    }
}
