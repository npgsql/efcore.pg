using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class GearsOfWarFromSqlQueryNpgsqlTest : GearsOfWarFromSqlQueryTestBase<NpgsqlTestStore, GearsOfWarQueryNpgsqlFixture>
    {
        public GearsOfWarFromSqlQueryNpgsqlTest(GearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void From_sql_queryable_simple_columns_out_of_order()
        {
            base.From_sql_queryable_simple_columns_out_of_order();

            Assert.Equal(
                @"SELECT ""Id"", ""Name"", ""IsAutomatic"", ""AmmunitionType"", ""OwnerFullName"", ""SynergyWithId"" FROM ""Weapon"" ORDER BY ""Name""",
                Sql);
        }

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;
    }
}
