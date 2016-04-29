using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class QueryNavigationsNpgsqlTest : QueryNavigationsTestBase<NorthwindQueryNpgsqlFixture>
    {
        public override void Select_Where_Navigation()
        {
            base.Select_Where_Navigation();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o.Customer"".""CustomerID"", ""o.Customer"".""Address"", ""o.Customer"".""City"", ""o.Customer"".""CompanyName"", ""o.Customer"".""ContactName"", ""o.Customer"".""ContactTitle"", ""o.Customer"".""Country"", ""o.Customer"".""Fax"", ""o.Customer"".""Phone"", ""o.Customer"".""PostalCode"", ""o.Customer"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""
ORDER BY ""o"".""CustomerID""",
                Sql);
        }

        public override void Select_Where_Navigation_Deep()
        {
            base.Select_Where_Navigation_Deep();

            Assert.StartsWith(
                @"SELECT ""od"".""OrderID"", ""od"".""ProductID"", ""od"".""Discount"", ""od"".""Quantity"", ""od"".""UnitPrice"", ""od.Order"".""OrderID"", ""od.Order"".""CustomerID"", ""od.Order"".""EmployeeID"", ""od.Order"".""OrderDate"", ""od.Order.Customer"".""CustomerID"", ""od.Order.Customer"".""Address"", ""od.Order.Customer"".""City"", ""od.Order.Customer"".""CompanyName"", ""od.Order.Customer"".""ContactName"", ""od.Order.Customer"".""ContactTitle"", ""od.Order.Customer"".""Country"", ""od.Order.Customer"".""Fax"", ""od.Order.Customer"".""Phone"", ""od.Order.Customer"".""PostalCode"", ""od.Order.Customer"".""Region""
FROM ""Order Details"" AS ""od""
INNER JOIN ""Orders"" AS ""od.Order"" ON ""od"".""OrderID"" = ""od.Order"".""OrderID""
LEFT JOIN ""Customers"" AS ""od.Order.Customer"" ON ""od.Order"".""CustomerID"" = ""od.Order.Customer"".""CustomerID""
ORDER BY ""od.Order"".""CustomerID""",
                Sql);
        }

        public override void Select_Where_Navigation_Included()
        {
            base.Select_Where_Navigation_Included();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o.Customer"".""CustomerID"", ""o.Customer"".""Address"", ""o.Customer"".""City"", ""o.Customer"".""CompanyName"", ""o.Customer"".""ContactName"", ""o.Customer"".""ContactTitle"", ""o.Customer"".""Country"", ""o.Customer"".""Fax"", ""o.Customer"".""Phone"", ""o.Customer"".""PostalCode"", ""o.Customer"".""Region"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""
LEFT JOIN ""Customers"" AS ""c"" ON ""o"".""CustomerID"" = ""c"".""CustomerID""
ORDER BY ""o"".""CustomerID""",
                Sql);
        }

        public override void Select_Navigation()
        {
            base.Select_Navigation();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o.Customer"".""CustomerID"", ""o.Customer"".""Address"", ""o.Customer"".""City"", ""o.Customer"".""CompanyName"", ""o.Customer"".""ContactName"", ""o.Customer"".""ContactTitle"", ""o.Customer"".""Country"", ""o.Customer"".""Fax"", ""o.Customer"".""Phone"", ""o.Customer"".""PostalCode"", ""o.Customer"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""
ORDER BY ""o"".""CustomerID""",
                Sql);
        }

        public override void Select_Navigations()
        {
            base.Select_Navigations();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o.Customer"".""CustomerID"", ""o.Customer"".""Address"", ""o.Customer"".""City"", ""o.Customer"".""CompanyName"", ""o.Customer"".""ContactName"", ""o.Customer"".""ContactTitle"", ""o.Customer"".""Country"", ""o.Customer"".""Fax"", ""o.Customer"".""Phone"", ""o.Customer"".""PostalCode"", ""o.Customer"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""
ORDER BY ""o"".""CustomerID""",
                Sql);
        }

        public override void Select_Where_Navigations()
        {
            base.Select_Where_Navigations();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o.Customer"".""CustomerID"", ""o.Customer"".""Address"", ""o.Customer"".""City"", ""o.Customer"".""CompanyName"", ""o.Customer"".""ContactName"", ""o.Customer"".""ContactTitle"", ""o.Customer"".""Country"", ""o.Customer"".""Fax"", ""o.Customer"".""Phone"", ""o.Customer"".""PostalCode"", ""o.Customer"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""
ORDER BY ""o"".""CustomerID""",
                Sql);
        }

        public override void Select_Where_Navigation_Multiple_Access()
        {
            base.Select_Where_Navigation_Multiple_Access();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o.Customer"".""CustomerID"", ""o.Customer"".""Address"", ""o.Customer"".""City"", ""o.Customer"".""CompanyName"", ""o.Customer"".""ContactName"", ""o.Customer"".""ContactTitle"", ""o.Customer"".""Country"", ""o.Customer"".""Fax"", ""o.Customer"".""Phone"", ""o.Customer"".""PostalCode"", ""o.Customer"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""
ORDER BY ""o"".""CustomerID""",
                Sql);
        }

        public override void Select_Navigations_Where_Navigations()
        {
            base.Select_Navigations_Where_Navigations();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o.Customer"".""CustomerID"", ""o.Customer"".""Address"", ""o.Customer"".""City"", ""o.Customer"".""CompanyName"", ""o.Customer"".""ContactName"", ""o.Customer"".""ContactTitle"", ""o.Customer"".""Country"", ""o.Customer"".""Fax"", ""o.Customer"".""Phone"", ""o.Customer"".""PostalCode"", ""o.Customer"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""
ORDER BY ""o"".""CustomerID""",
                Sql);
        }

        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o.Customer"".""CustomerID"", ""o.Customer"".""Address"", ""o.Customer"".""City"", ""o.Customer"".""CompanyName"", ""o.Customer"".""ContactName"", ""o.Customer"".""ContactTitle"", ""o.Customer"".""Country"", ""o.Customer"".""Fax"", ""o.Customer"".""Phone"", ""o.Customer"".""PostalCode"", ""o.Customer"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""
ORDER BY ""o"".""CustomerID""",
                Sql);
        }

        public override void Select_Singleton_Navigation_With_Member_Access()
        {
            base.Select_Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o.Customer"".""CustomerID"", ""o.Customer"".""Address"", ""o.Customer"".""City"", ""o.Customer"".""CompanyName"", ""o.Customer"".""ContactName"", ""o.Customer"".""ContactTitle"", ""o.Customer"".""Country"", ""o.Customer"".""Fax"", ""o.Customer"".""Phone"", ""o.Customer"".""PostalCode"", ""o.Customer"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""
ORDER BY ""o"".""CustomerID""",
                Sql);
        }

        public override void Singleton_Navigation_With_Member_Access()
        {
            base.Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o.Customer"".""CustomerID"", ""o.Customer"".""Address"", ""o.Customer"".""City"", ""o.Customer"".""CompanyName"", ""o.Customer"".""ContactName"", ""o.Customer"".""ContactTitle"", ""o.Customer"".""Country"", ""o.Customer"".""Fax"", ""o.Customer"".""Phone"", ""o.Customer"".""PostalCode"", ""o.Customer"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""
ORDER BY ""o"".""CustomerID""",
                Sql);
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();

            Assert.Equal(
                @"SELECT ""o1"".""OrderID"", ""o1"".""CustomerID"", ""o1"".""EmployeeID"", ""o1"".""OrderDate"", ""o2"".""OrderID"", ""o2"".""CustomerID"", ""o2"".""EmployeeID"", ""o2"".""OrderDate""
FROM ""Orders"" AS ""o1""
INNER JOIN ""Customers"" AS ""o1.Customer"" ON ""o1"".""CustomerID"" = ""o1.Customer"".""CustomerID""
CROSS JOIN ""Orders"" AS ""o2""
INNER JOIN ""Customers"" AS ""o2.Customer"" ON ""o2"".""CustomerID"" = ""o2.Customer"".""CustomerID""
WHERE (""o1.Customer"".""City"" = ""o2.Customer"".""City"") OR (""o1.Customer"".""City"" IS NULL AND ""o2.Customer"".""City"" IS NULL)",
                Sql);
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();

            Assert.Equal(
                @"SELECT ""o1"".""CustomerID"", ""o2"".""CustomerID""
FROM ""Orders"" AS ""o1""
INNER JOIN ""Customers"" AS ""o1.Customer"" ON ""o1"".""CustomerID"" = ""o1.Customer"".""CustomerID""
CROSS JOIN ""Orders"" AS ""o2""
INNER JOIN ""Customers"" AS ""o2.Customer"" ON ""o2"".""CustomerID"" = ""o2.Customer"".""CustomerID""
WHERE (""o1.Customer"".""City"" = ""o2.Customer"".""City"") OR (""o1.Customer"".""City"" IS NULL AND ""o2.Customer"".""City"" IS NULL)",
                Sql);
        }

        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();

            Assert.Equal(
                @"SELECT ""o1"".""OrderID"", ""o1"".""CustomerID"", ""o1"".""EmployeeID"", ""o1"".""OrderDate"", ""o2"".""OrderID"", ""o2"".""CustomerID"", ""o2"".""EmployeeID"", ""o2"".""OrderDate""
FROM ""Orders"" AS ""o1""
CROSS JOIN ""Orders"" AS ""o2""
WHERE (""o1"".""CustomerID"" = ""o2"".""CustomerID"") OR (""o1"".""CustomerID"" IS NULL AND ""o2"".""CustomerID"" IS NULL)",
                Sql);
        }

        public override void Select_Where_Navigation_Null()
        {
            base.Select_Where_Navigation_Null();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" IS NULL",
                Sql);
        }

        public override void Select_Where_Navigation_Null_Deep()
        {
            base.Select_Where_Navigation_Null_Deep();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title"", ""e.Manager"".""EmployeeID"", ""e.Manager"".""City"", ""e.Manager"".""Country"", ""e.Manager"".""FirstName"", ""e.Manager"".""ReportsTo"", ""e.Manager"".""Title""
FROM ""Employees"" AS ""e""
LEFT JOIN ""Employees"" AS ""e.Manager"" ON ""e"".""ReportsTo"" = ""e.Manager"".""EmployeeID""
ORDER BY ""e"".""ReportsTo""",
                Sql);
        }

        public override void Select_Where_Navigation_Null_Reverse()
        {
            base.Select_Where_Navigation_Null_Reverse();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" IS NULL",
                Sql);
        }

        public override void Collection_select_nav_prop_any()
        {
            base.Collection_select_nav_prop_any();

            Assert.Equal(
                @"SELECT (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM ""Orders"" AS ""o0""
            WHERE ""c"".""CustomerID"" = ""o0"".""CustomerID"")
        THEN TRUE::bool ELSE FALSE::bool
    END
)
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Collection_where_nav_prop_any()
        {
            base.Collection_where_nav_prop_any();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE EXISTS (
    SELECT 1
    FROM ""Orders"" AS ""o""
    WHERE ""c"".""CustomerID"" = ""o"".""CustomerID"")",
                Sql);
        }

        public override void Collection_where_nav_prop_any_predicate()
        {
            base.Collection_where_nav_prop_any_predicate();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE EXISTS (
    SELECT 1
    FROM ""Orders"" AS ""o""
    WHERE (""o"".""OrderID"" > 0) AND (""c"".""CustomerID"" = ""o"".""CustomerID""))",
                Sql);
        }

        public override void Collection_select_nav_prop_all()
        {
            base.Collection_select_nav_prop_all();

            Assert.Equal(
                @"SELECT (
    SELECT CASE
        WHEN NOT EXISTS (
            SELECT 1
            FROM ""Orders"" AS ""o0""
            WHERE ((""c"".""CustomerID"" = ""o0"".""CustomerID"") AND ""o0"".""CustomerID"" IS NOT NULL) AND NOT ((""o0"".""CustomerID"" = 'ALFKI') AND ""o0"".""CustomerID"" IS NOT NULL))
        THEN TRUE::bool ELSE FALSE::bool
    END
)
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Collection_select_nav_prop_all_client()
        {
            base.Collection_select_nav_prop_all_client();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" AS ""c""

SELECT ""o1"".""OrderID"", ""o1"".""CustomerID"", ""o1"".""EmployeeID"", ""o1"".""OrderDate""
FROM ""Orders"" AS ""o1""",
                Sql);
        }

        public override void Collection_where_nav_prop_all()
        {
            base.Collection_where_nav_prop_all();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOT EXISTS (
    SELECT 1
    FROM ""Orders"" AS ""o""
    WHERE ((""c"".""CustomerID"" = ""o"".""CustomerID"") AND ""o"".""CustomerID"" IS NOT NULL) AND NOT ((""o"".""CustomerID"" = 'ALFKI') AND ""o"".""CustomerID"" IS NOT NULL))",
                Sql);
        }

        public override void Collection_where_nav_prop_all_client()
        {
            base.Collection_where_nav_prop_all_client();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Collection_select_nav_prop_count()
        {
            base.Collection_select_nav_prop_count();

            Assert.Equal(
                @"SELECT (
    SELECT COUNT(*)::INT4
    FROM ""Orders"" AS ""o0""
    WHERE ""c"".""CustomerID"" = ""o0"".""CustomerID""
)
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Collection_where_nav_prop_count()
        {
            base.Collection_where_nav_prop_count();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (
    SELECT COUNT(*)::INT4
    FROM ""Orders"" AS ""o""
    WHERE ""c"".""CustomerID"" = ""o"".""CustomerID""
) > 5",
                Sql);
        }

        public override void Collection_where_nav_prop_count_reverse()
        {
            base.Collection_where_nav_prop_count_reverse();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE 5 < (
    SELECT COUNT(*)::INT4
    FROM ""Orders"" AS ""o""
    WHERE ""c"".""CustomerID"" = ""o"".""CustomerID""
)",
                Sql);
        }

        public override void Collection_orderby_nav_prop_count()
        {
            base.Collection_orderby_nav_prop_count();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
ORDER BY (
    SELECT COUNT(*)::INT4
    FROM ""Orders"" AS ""o""
    WHERE ""c"".""CustomerID"" = ""o"".""CustomerID""
)",
                Sql);
        }

        public override void Collection_select_nav_prop_long_count()
        {
            base.Collection_select_nav_prop_long_count();

            Assert.Equal(
                @"SELECT (
    SELECT COUNT(*)
    FROM ""Orders"" AS ""o0""
    WHERE ""c"".""CustomerID"" = ""o0"".""CustomerID""
)
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Collection_select_nav_prop_sum()
        {
            base.Collection_select_nav_prop_sum();

            Assert.Equal(
                @"SELECT (
    SELECT SUM(""o0"".""OrderID"")::INT4
    FROM ""Orders"" AS ""o0""
    WHERE ""c"".""CustomerID"" = ""o0"".""CustomerID""
)
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Collection_where_nav_prop_sum()
        {
            base.Collection_where_nav_prop_sum();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (
    SELECT SUM(""o"".""OrderID"")::INT4
    FROM ""Orders"" AS ""o""
    WHERE ""c"".""CustomerID"" = ""o"".""CustomerID""
) > 1000",
                Sql);
        }

        public override void Collection_select_nav_prop_first_or_default()
        {
            base.Collection_select_nav_prop_first_or_default();

            // TODO: Projection sub-query lifting
            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" AS ""c""

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Collection_select_nav_prop_first_or_default_then_nav_prop()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop();

            // TODO: Projection sub-query lifting
            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" AS ""c""

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        private static string Sql => TestSqlLoggerFactory.Sql;

        public QueryNavigationsNpgsqlTest(NorthwindQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }
    }
}
