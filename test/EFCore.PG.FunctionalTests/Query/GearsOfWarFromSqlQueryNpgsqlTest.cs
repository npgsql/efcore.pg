using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    public class GearsOfWarFromSqlQueryNpgsqlTest : GearsOfWarFromSqlQueryTestBase<GearsOfWarQueryNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public GearsOfWarFromSqlQueryNpgsqlTest(GearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        public override void From_sql_queryable_simple_columns_out_of_order()
        {
            base.From_sql_queryable_simple_columns_out_of_order();

            Assert.Equal(
                @"SELECT ""Id"", ""Name"", ""IsAutomatic"", ""AmmunitionType"", ""OwnerFullName"", ""SynergyWithId"" FROM ""Weapons"" ORDER BY ""Name""",
                Sql);
        }

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();

        string Sql => Fixture.TestSqlLoggerFactory.Sql;
    }
}
