using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class AsyncQueryNpgsqlTest : AsyncQueryTestBase<NorthwindQueryNpgsqlFixture>
    {
        public AsyncQueryNpgsqlTest(NorthwindQueryNpgsqlFixture fixture)
            : base(fixture) {}

        #region Skipped tests

        [Fact(Skip = "Test commented out in EF7 (SqlServer/Sqlite)")]
        public override Task Projection_when_arithmetic_expressions() => null;

        [Fact(Skip = "Test commented out in EF7 (SqlServer/Sqlite)")]
        public override Task Projection_when_arithmetic_mixed() => null;

        [Fact(Skip = "Test commented out in EF7 (SqlServer/Sqlite)")]
        public override Task Projection_when_arithmetic_mixed_subqueries() => null;

        [Fact(Skip = "Fails on Npgsql < 3.1.3 (close in connecting state)")]
        public override Task Throws_on_concurrent_query_first() => null;

        #endregion
    }
}
