using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindAggregateOperatorsQueryNpgsqlTest : NorthwindAggregateOperatorsQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public NorthwindAggregateOperatorsQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Contains_with_local_uint_array_closure(bool async)
        {
            await base.Contains_with_local_uint_array_closure(async);

            // Note: PostgreSQL doesn't support uint, but value converters make this into bigint
            AssertSql(
                @"@__ids_0='System.Int32[]' (DbType = Object)

SELECT e.""EmployeeID"", e.""City"", e.""Country"", e.""FirstName"", e.""ReportsTo"", e.""Title""
FROM ""Employees"" AS e
WHERE e.""EmployeeID"" = ANY (@__ids_0)",
                //
                @"@__ids_0='System.Int32[]' (DbType = Object)

SELECT e.""EmployeeID"", e.""City"", e.""Country"", e.""FirstName"", e.""ReportsTo"", e.""Title""
FROM ""Employees"" AS e
WHERE e.""EmployeeID"" = ANY (@__ids_0)");
        }

        public override async Task Contains_with_local_nullable_uint_array_closure(bool async)
        {
            await base.Contains_with_local_nullable_uint_array_closure(async);

            // Note: PostgreSQL doesn't support uint, but value converters make this into bigint

            AssertSql(
                @"SELECT e.""EmployeeID"", e.""City"", e.""Country"", e.""FirstName"", e.""ReportsTo"", e.""Title""
FROM ""Employees"" AS e
WHERE e.""EmployeeID"" IN (0, 1)",
                //
                @"SELECT e.""EmployeeID"", e.""City"", e.""Country"", e.""FirstName"", e.""ReportsTo"", e.""Title""
FROM ""Employees"" AS e
WHERE e.""EmployeeID"" = 0");
        }

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
