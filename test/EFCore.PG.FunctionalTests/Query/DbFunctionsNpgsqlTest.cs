using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DbFunctionsNpgsqlTest : DbFunctionsTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public DbFunctionsNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        public override void String_Like_Literal()
        {
            // PostgreSQL like is case-sensitive, while the EF Core "default" (i.e. SqlServer) is insensitive.
            // So we override and assert only 19 matches unlike the default's 34.
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

        [Fact]
        public void String_ILike_Literal()
        {
            using (var context = CreateContext())
            {
                var count = context.Customers.Count(c => EF.Functions.ILike(c.ContactName, "%M%"));

                Assert.Equal(34, count);
            }

            // For the useless = TRUE below see https://github.com/aspnet/EntityFramework/issues/9143
            Assert.Equal(
                @"SELECT COUNT(*)::INT4
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" ILIKE '%M%' = TRUE",
                Sql);
        }

        [Fact]
        public void String_ILike_Literal_With_Escape()
        {
            using (var context = CreateContext())
            {
                var count = context.Customers.Count(c => EF.Functions.ILike(c.ContactName, "!%", "!"));
                Assert.Equal(0, count);
            }

            // For the useless = TRUE below see https://github.com/aspnet/EntityFramework/issues/9143
            Assert.Equal(
                @"SELECT COUNT(*)::INT4
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" ILIKE '!%' ESCAPE '!' = TRUE",
                Sql);
        }

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;
    }
}
