using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class DbFunctionsNpgsqlTest : DbFunctionsTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public DbFunctionsNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        #region Like, ILike

        public override void Like_literal()
        {
            // PostgreSQL like is case-sensitive, while the EF Core "default" (i.e. SqlServer) is insensitive.
            // So we override and assert only 19 matches unlike the default's 34.
            using (var context = CreateContext())
            {
                var count = context.Customers.Count(c => EF.Functions.Like(c.ContactName, "%M%"));

                Assert.Equal(19, count);
            }

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" LIKE '%M%'");
        }

        public override void Like_identity()
        {
            base.Like_identity();

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" LIKE c.""ContactName"" ESCAPE ''");
        }

        public override void Like_literal_with_escape()
        {
            base.Like_literal_with_escape();

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" LIKE '!%' ESCAPE '!'");
        }

        [Fact]
        public void String_Like_Literal_With_Backslash()
        {
            using (var context = CreateContext())
            {
                var count = context.Customers.Count(c => EF.Functions.Like(c.ContactName, "\\"));

                Assert.Equal(0, count);
            }

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" LIKE '\' ESCAPE ''");
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
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" ILIKE '%M%' = TRUE");
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
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" ILIKE '!%' ESCAPE '!' = TRUE");
        }

        #endregion

        #region NullIf

        [Fact]
        public void NullIf_with_class()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.Customers.Select(c => EF.Functions.NullIf(c.Address, c.City)).ToArray();
                AssertContainsInSql("SELECT NULLIF(c.\"Address\", c.\"City\")");
            }
        }

        [Fact]
        public void NullIf_with_struct()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.OrderDetails.Select(od => EF.Functions.NullIf(od.Quantity, default(short?))).ToArray();
                AssertContainsInSql("SELECT NULLIF(od.\"Quantity\", NULL)");
            }
        }

        #endregion

        #region Helpers

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        #endregion
    }
}
