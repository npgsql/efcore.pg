using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class QueryTaggingNpgsqlTest : QueryTaggingTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public QueryTaggingNpgsqlTest(
            NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Single_query_tag()
        {
            base.Single_query_tag();

            AssertSql(
                @"-- EFCore: (#Yanni)
SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
ORDER BY c.""CustomerID""
LIMIT 1");
        }

        public override void Single_query_multiple_tags()
        {
            base.Single_query_multiple_tags();

            AssertSql(
                @"-- EFCore: (#Yanni, #Enya)
SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
ORDER BY c.""CustomerID""
LIMIT 1");
        }

        public override void Tags_on_subquery()
        {
            base.Tags_on_subquery();

            AssertSql(
                @"-- EFCore: (#Yanni, #Laurel)
SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
CROSS JOIN (
    SELECT o.*
    FROM ""Orders"" AS o
    ORDER BY o.""OrderID""
    LIMIT 5
) AS t
WHERE c.""CustomerID"" = 'ALFKI'");
        }

        public override void Duplicate_tags()
        {
            base.Duplicate_tags();

            AssertSql(
                @"-- EFCore: (#Yanni)
SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
ORDER BY c.""CustomerID""
LIMIT 1");
        }

        public override void Tag_on_include_query()
        {
            base.Tag_on_include_query();

            AssertSql(
                @"-- EFCore: (#Yanni)
SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
ORDER BY c.""CustomerID""
LIMIT 1",
                //
                @"-- EFCore: (#Yanni)
SELECT ""c.Orders"".""OrderID"", ""c.Orders"".""CustomerID"", ""c.Orders"".""EmployeeID"", ""c.Orders"".""OrderDate""
FROM ""Orders"" AS ""c.Orders""
INNER JOIN (
    SELECT c0.""CustomerID""
    FROM ""Customers"" AS c0
    ORDER BY c0.""CustomerID""
    LIMIT 1
) AS t ON ""c.Orders"".""CustomerID"" = t.""CustomerID""
ORDER BY t.""CustomerID""");
        }

        void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}

