using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

// ReSharper disable StringLiteralTypo
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    public class QueryTaggingNpgsqlTest : QueryTaggingTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public QueryTaggingNpgsqlTest(
            NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        public override void Single_query_tag()
        {
            base.Single_query_tag();

            AssertSql(
                @"-- Yanni

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
ORDER BY c.""CustomerID""
LIMIT 1");
        }

        public override void Single_query_multiple_tags()
        {
            base.Single_query_multiple_tags();

            AssertSql(
                @"-- Yanni

-- Enya

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
ORDER BY c.""CustomerID""
LIMIT 1");
        }

        public override void Tags_on_subquery()
        {
            base.Tags_on_subquery();

            AssertSql(
                @"-- Yanni

-- Laurel

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
                @"-- Yanni

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
ORDER BY c.""CustomerID""
LIMIT 1");
        }

        public override void Tag_on_include_query()
        {
            base.Tag_on_include_query();

            AssertSql(
                @"-- Yanni

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
ORDER BY c.""CustomerID""
LIMIT 1",
                //
                @"-- Yanni

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

        public override void Tag_on_scalar_query()
        {
            base.Tag_on_scalar_query();

            AssertSql(
                @"-- Yanni

SELECT o.""OrderDate""
FROM ""Orders"" AS o
ORDER BY o.""OrderID""
LIMIT 1");
        }

        public override void Single_query_multiline_tag()
        {
            base.Single_query_multiline_tag();

            AssertSql(
                @"-- Yanni
-- AND
-- Laurel

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
ORDER BY c.""CustomerID""
LIMIT 1");
        }

        public override void Single_query_multiple_multiline_tag()
        {
            base.Single_query_multiple_multiline_tag();

            AssertSql(
                @"-- Yanni
-- AND
-- Laurel

-- Yet
-- Another
-- Multiline
-- Tag

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
ORDER BY c.""CustomerID""
LIMIT 1");
        }

        public override void Single_query_multiline_tag_with_empty_lines()
        {
            base.Single_query_multiline_tag_with_empty_lines();

            AssertSql(
                @"-- Yanni
-- 
-- AND
-- 
-- Laurel

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
ORDER BY c.""CustomerID""
LIMIT 1");
        }

        void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}

