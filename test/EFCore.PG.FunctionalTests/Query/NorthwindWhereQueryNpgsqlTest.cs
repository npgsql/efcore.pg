using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindWhereQueryNpgsqlTest : NorthwindWhereQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public NorthwindWhereQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [Theory(Skip = "SQL translation not implemented, too annoying")]
        public override Task Where_datetime_millisecond_component(bool async)
            => base.Where_datetime_millisecond_component(async);

        [Theory(Skip = "Translation not implemented yet, #873")]
        public override Task Where_datetimeoffset_now_component(bool async)
            => base.Where_datetimeoffset_now_component(async);

        [Theory(Skip = "Translation not implemented yet, #873")]
        public override Task Where_datetimeoffset_utcnow_component(bool async)
            => base.Where_datetimeoffset_utcnow_component(async);

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
