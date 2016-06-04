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
    public class IncludeNpgsqlTest : IncludeTestBase<NorthwindQueryNpgsqlFixture>
    {
        public IncludeNpgsqlTest(NorthwindQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override void Include_collection()
        {
            base.Include_collection();

            // TODO: Assert on SQL
        }

        public override void Include_reference_and_collection()
        {
            base.Include_reference_and_collection();

            // TODO: Assert on SQL
        }

        public override void Include_references_multi_level()
        {
            base.Include_references_multi_level();

            Assert.Equal(
                @"SELECT ""od"".""OrderID"", ""od"".""ProductID"", ""od"".""Discount"", ""od"".""Quantity"", ""od"".""UnitPrice"", ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Order Details"" AS ""od""
INNER JOIN ""Orders"" AS ""o"" ON ""od"".""OrderID"" = ""o"".""OrderID""
LEFT JOIN ""Customers"" AS ""c"" ON ""o"".""CustomerID"" = ""c"".""CustomerID""",
                Sql);
        }

        public override void Include_multiple_references_multi_level()
        {
            base.Include_multiple_references_multi_level();

            Assert.Equal(
                @"SELECT ""od"".""OrderID"", ""od"".""ProductID"", ""od"".""Discount"", ""od"".""Quantity"", ""od"".""UnitPrice"", ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Order Details"" AS ""od""
INNER JOIN ""Orders"" AS ""o"" ON ""od"".""OrderID"" = ""o"".""OrderID""
LEFT JOIN ""Customers"" AS ""c"" ON ""o"".""CustomerID"" = ""c"".""CustomerID""
INNER JOIN ""Products"" AS ""p"" ON ""od"".""ProductID"" = ""p"".""ProductID""",
                Sql);
        }

        public override void Include_multiple_references_multi_level_reverse()
        {
            base.Include_multiple_references_multi_level_reverse();

            Assert.Equal(
                @"SELECT ""od"".""OrderID"", ""od"".""ProductID"", ""od"".""Discount"", ""od"".""Quantity"", ""od"".""UnitPrice"", ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Order Details"" AS ""od""
INNER JOIN ""Orders"" AS ""o"" ON ""od"".""OrderID"" = ""o"".""OrderID""
LEFT JOIN ""Customers"" AS ""c"" ON ""o"".""CustomerID"" = ""c"".""CustomerID""
INNER JOIN ""Products"" AS ""p"" ON ""od"".""ProductID"" = ""p"".""ProductID""",
                Sql);
        }

        public override void Include_references_and_collection_multi_level()
        {
            base.Include_references_and_collection_multi_level();

            // TODO: Assert on SQL
        }

        public override void Include_multi_level_reference_and_collection_predicate()
        {
            base.Include_multi_level_reference_and_collection_predicate();

            // TODO: Assert on SQL
        }

        public override void Include_multi_level_collection_and_then_include_reference_predicate()
        {
            base.Include_multi_level_collection_and_then_include_reference_predicate();

            // TODO: Assert on SQL
        }

        public override void Include_collection_alias_generation()
        {
            base.Include_collection_alias_generation();

            // TODO: Assert on SQL
        }

        public override void Include_collection_order_by_key()
        {
            base.Include_collection_order_by_key();

            // TODO: Assert on SQL
        }

        public override void Include_collection_order_by_non_key()
        {
            base.Include_collection_order_by_non_key();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
ORDER BY ""c"".""City"", ""c"".""CustomerID""

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
INNER JOIN (
    SELECT DISTINCT ""c"".""City"", ""c"".""CustomerID""
    FROM ""Customers"" AS ""c""
) AS ""c0"" ON ""o"".""CustomerID"" = ""c0"".""CustomerID""
ORDER BY ""c0"".""City"", ""c0"".""CustomerID""",
                Sql);
        }

        public override void Include_collection_as_no_tracking()
        {
            base.Include_collection_as_no_tracking();

            // TODO: Assert on SQL
        }

        public override void Include_collection_principal_already_tracked()
        {
            base.Include_collection_principal_already_tracked();

            // TODO: Assert on SQL
        }

        public override void Include_collection_principal_already_tracked_as_no_tracking()
        {
            base.Include_collection_principal_already_tracked_as_no_tracking();

            // TODO: Assert on SQL
        }

        public override void Include_collection_with_filter()
        {
            base.Include_collection_with_filter();

            // TODO: Assert on SQL
        }

        public override void Include_collection_with_filter_reordered()
        {
            base.Include_collection_with_filter_reordered();

            // TODO: Assert on SQL
        }

        public override void Include_collection_then_include_collection()
        {
            base.Include_collection_then_include_collection();

            // TODO: Assert on SQL
        }

        public override void Include_collection_when_projection()
        {
            base.Include_collection_when_projection();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Include_collection_on_join_clause_with_filter()
        {
            base.Include_collection_on_join_clause_with_filter();

            // TODO: Assert on SQL
        }

        public override void Include_collection_on_additional_from_clause_with_filter()
        {
            base.Include_collection_on_additional_from_clause_with_filter();

            // TODO: Assert on SQL
        }

        public override void Include_collection_on_additional_from_clause()
        {
            base.Include_collection_on_additional_from_clause();

            // TODO: Assert on SQL
        }

        public override void Include_duplicate_collection()
        {
            base.Include_duplicate_collection();

            // TODO: Assert on SQL
        }

        public override void Include_duplicate_collection_result_operator()
        {
            base.Include_duplicate_collection_result_operator();

            // TODO: Assert on SQL
        }

        public override void Include_collection_on_join_clause_with_order_by_and_filter()
        {
            base.Include_collection_on_join_clause_with_order_by_and_filter();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
INNER JOIN ""Orders"" AS ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
WHERE ""c"".""CustomerID"" = 'ALFKI'
ORDER BY ""c"".""City"", ""c"".""CustomerID""

SELECT ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate""
FROM ""Orders"" AS ""o0""
INNER JOIN (
    SELECT DISTINCT ""c"".""City"", ""c"".""CustomerID""
    FROM ""Customers"" AS ""c""
    INNER JOIN ""Orders"" AS ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
    WHERE ""c"".""CustomerID"" = 'ALFKI'
) AS ""c0"" ON ""o0"".""CustomerID"" = ""c0"".""CustomerID""
ORDER BY ""c0"".""City"", ""c0"".""CustomerID""",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause2()
        {
            base.Include_collection_on_additional_from_clause2();

            Assert.Equal(
                @"@__p_0: 5

SELECT ""t"".""CustomerID"", ""t"".""Address"", ""t"".""City"", ""t"".""CompanyName"", ""t"".""ContactName"", ""t"".""ContactTitle"", ""t"".""Country"", ""t"".""Fax"", ""t"".""Phone"", ""t"".""PostalCode"", ""t"".""Region""
FROM (
    SELECT ""c0"".""CustomerID"", ""c0"".""Address"", ""c0"".""City"", ""c0"".""CompanyName"", ""c0"".""ContactName"", ""c0"".""ContactTitle"", ""c0"".""Country"", ""c0"".""Fax"", ""c0"".""Phone"", ""c0"".""PostalCode"", ""c0"".""Region""
    FROM ""Customers"" AS ""c0""
    ORDER BY ""c0"".""CustomerID""
    LIMIT @__p_0
) AS ""t""
CROSS JOIN ""Customers"" AS ""c1""",
                Sql);
        }

        public override void Include_duplicate_collection_result_operator2()
        {
            base.Include_duplicate_collection_result_operator2();

            // TODO: Assert on SQL
        }

        public override void Include_reference()
        {
            base.Include_reference();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""c"" ON ""o"".""CustomerID"" = ""c"".""CustomerID""",
                Sql);
        }

        public override void Include_multiple_references()
        {
            base.Include_multiple_references();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""ProductID"", ""o"".""Discount"", ""o"".""Quantity"", ""o"".""UnitPrice"", ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate"", ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Order Details"" AS ""o""
INNER JOIN ""Orders"" AS ""o0"" ON ""o"".""OrderID"" = ""o0"".""OrderID""
INNER JOIN ""Products"" AS ""p"" ON ""o"".""ProductID"" = ""p"".""ProductID""",
                Sql);
        }

        public override void Include_reference_alias_generation()
        {
            base.Include_reference_alias_generation();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""ProductID"", ""o"".""Discount"", ""o"".""Quantity"", ""o"".""UnitPrice"", ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate""
FROM ""Order Details"" AS ""o""
INNER JOIN ""Orders"" AS ""o0"" ON ""o"".""OrderID"" = ""o0"".""OrderID""",
                Sql);
        }

        public override void Include_duplicate_reference()
        {
            base.Include_duplicate_reference();

            Assert.Equal(
                @"@__p_0: 2

SELECT ""t"".""OrderID"", ""t"".""CustomerID"", ""t"".""EmployeeID"", ""t"".""OrderDate"", ""t0"".""OrderID"", ""t0"".""CustomerID"", ""t0"".""EmployeeID"", ""t0"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""c0"".""CustomerID"", ""c0"".""Address"", ""c0"".""City"", ""c0"".""CompanyName"", ""c0"".""ContactName"", ""c0"".""ContactTitle"", ""c0"".""Country"", ""c0"".""Fax"", ""c0"".""Phone"", ""c0"".""PostalCode"", ""c0"".""Region""
FROM (
    SELECT ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate""
    FROM ""Orders"" AS ""o0""
    ORDER BY ""o0"".""CustomerID""
    LIMIT @__p_0
) AS ""t""
CROSS JOIN (
    SELECT ""o2"".""OrderID"", ""o2"".""CustomerID"", ""o2"".""EmployeeID"", ""o2"".""OrderDate""
    FROM ""Orders"" AS ""o2""
    ORDER BY ""o2"".""CustomerID""
    LIMIT 2 OFFSET 2
) AS ""t0""
LEFT JOIN ""Customers"" AS ""c"" ON ""t"".""CustomerID"" = ""c"".""CustomerID""
LEFT JOIN ""Customers"" AS ""c0"" ON ""t0"".""CustomerID"" = ""c0"".""CustomerID""",
                Sql);
        }

        public override void Include_duplicate_reference2()
        {
            base.Include_duplicate_reference2();

            Assert.Equal(
                @"@__p_0: 2

SELECT ""t"".""OrderID"", ""t"".""CustomerID"", ""t"".""EmployeeID"", ""t"".""OrderDate"", ""t0"".""OrderID"", ""t0"".""CustomerID"", ""t0"".""EmployeeID"", ""t0"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM (
    SELECT ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate""
    FROM ""Orders"" AS ""o0""
    ORDER BY ""o0"".""OrderID""
    LIMIT @__p_0
) AS ""t""
CROSS JOIN (
    SELECT ""o2"".""OrderID"", ""o2"".""CustomerID"", ""o2"".""EmployeeID"", ""o2"".""OrderDate""
    FROM ""Orders"" AS ""o2""
    ORDER BY ""o2"".""OrderID""
    LIMIT 2 OFFSET 2
) AS ""t0""
LEFT JOIN ""Customers"" AS ""c"" ON ""t"".""CustomerID"" = ""c"".""CustomerID""",
                Sql);
        }

        public override void Include_duplicate_reference3()
        {
            base.Include_duplicate_reference3();

            Assert.Equal(
                @"@__p_0: 2

SELECT ""t"".""OrderID"", ""t"".""CustomerID"", ""t"".""EmployeeID"", ""t"".""OrderDate"", ""t0"".""OrderID"", ""t0"".""CustomerID"", ""t0"".""EmployeeID"", ""t0"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM (
    SELECT ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate""
    FROM ""Orders"" AS ""o0""
    ORDER BY ""o0"".""OrderID""
    LIMIT @__p_0
) AS ""t""
CROSS JOIN (
    SELECT ""o2"".""OrderID"", ""o2"".""CustomerID"", ""o2"".""EmployeeID"", ""o2"".""OrderDate""
    FROM ""Orders"" AS ""o2""
    ORDER BY ""o2"".""OrderID""
    LIMIT 2 OFFSET 2
) AS ""t0""
LEFT JOIN ""Customers"" AS ""c"" ON ""t0"".""CustomerID"" = ""c"".""CustomerID""",
                Sql);
        }

        public override void Include_reference_when_projection()
        {
            base.Include_reference_when_projection();

            Assert.Equal(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Include_reference_with_filter_reordered()
        {
            base.Include_reference_with_filter_reordered();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""c"" ON ""o"".""CustomerID"" = ""c"".""CustomerID""
WHERE ""o"".""CustomerID"" = 'ALFKI'",
                Sql);
        }

        public override void Include_reference_with_filter()
        {
            base.Include_reference_with_filter();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""c"" ON ""o"".""CustomerID"" = ""c"".""CustomerID""
WHERE ""o"".""CustomerID"" = 'ALFKI'",
                Sql);
        }

        public override void Include_collection_dependent_already_tracked_as_no_tracking()
        {
            base.Include_collection_dependent_already_tracked_as_no_tracking();

            // TODO: Assert on SQL
        }

        public override void Include_collection_dependent_already_tracked()
        {
            base.Include_collection_dependent_already_tracked();

            // TODO: Assert on SQL
        }

        public override void Include_reference_dependent_already_tracked()
        {
            base.Include_reference_dependent_already_tracked();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE ""o"".""CustomerID"" = 'ALFKI'

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""c"" ON ""o"".""CustomerID"" = ""c"".""CustomerID""",
                Sql);
        }

        public override void Include_reference_as_no_tracking()
        {
            base.Include_reference_as_no_tracking();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" AS ""o""
LEFT JOIN ""Customers"" AS ""c"" ON ""o"".""CustomerID"" = ""c"".""CustomerID""",
                Sql);
        }

        public override void Include_collection_as_no_tracking2()
        {
            base.Include_collection_as_no_tracking2();

            // TODO: Assert on SQL
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
