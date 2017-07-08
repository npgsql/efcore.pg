using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DbFunctionsNpgsqlTest : DbFunctionsTestBase<NorthwindQueryNpgsqlFixture>
    {
        public DbFunctionsNpgsqlTest(NorthwindQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void String_Like_Literal()
        {
            // The superclass implementation of this asserts 34, since SqlServer does case-insensitive matching.
            using (var context = CreateContext())
            {
                var count = context.Customers.Count(c => EF.Functions.Like(c.ContactName, "%M%"));

                Assert.Equal(19, count);
            }

            Assert.Equal(
                @"SELECT COUNT(*)::INT4
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '%M%'",
                Sql);
        }

        public override void String_Like_Identity()
        {
            base.String_Like_Identity();

            Assert.Equal(
                @"SELECT COUNT(*)::INT4
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE ""c"".""ContactName"" ESCAPE ''",
                Sql);
        }

        public override void String_Like_Literal_With_Escape()
        {
            base.String_Like_Literal_With_Escape();

            Assert.Equal(
                @"SELECT COUNT(*)::INT4
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '!%' ESCAPE '!'",
                Sql);
        }

        [Fact]
        public void String_Like_Literal_With_Backslash()
        {
            using (var context = CreateContext())
            {
                var count = context.Customers.Count(c => EF.Functions.Like(c.ContactName, "\\"));

                Assert.Equal(0, count);
            }

            Assert.Equal(
                @"SELECT COUNT(*)::INT4
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\' ESCAPE ''",
                Sql);
        }

        private const string FileLineEnding = @"
";

        private string Sql => Fixture.TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
