using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class UdfDbFunctionNpgsqlTests : UdfDbFunctionTestBase<UdfDbFunctionNpgsqlTests.Npgsql>
    {
        // ReSharper disable once UnusedParameter.Local
        public UdfDbFunctionNpgsqlTests(Npgsql fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Scalar Tests

        #region Static

        [Fact]
        public override void Scalar_Function_Extension_Method_Static()
        {
            base.Scalar_Function_Extension_Method_Static();

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE (""IsDate""(c.""FirstName"") = FALSE) AND (""IsDate""(c.""FirstName"") IS NOT NULL)");
        }

        [Fact]
        public override void Scalar_Function_With_Translator_Translates_Static()
        {
            base.Scalar_Function_With_Translator_Translates_Static();

            AssertSql(
                @"@__customerId_0='3'

SELECT length(c.""LastName"")
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_0
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Static()
            => base.Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Static();

        [Fact]
        public override void Scalar_Function_Constant_Parameter_Static()
        {
            base.Scalar_Function_Constant_Parameter_Static();

            AssertSql(
                @"@__customerId_0='1'

SELECT ""CustomerOrderCount""(@__customerId_0)
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Correlated_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Correlated_Static();

            AssertSql(
                @"SELECT c.""LastName"", ""CustomerOrderCount""(c.""Id"") AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 1
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static();

            AssertSql(
                @"SELECT c.""LastName"", ""CustomerOrderCount""(1) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 1
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Parameter_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Parameter_Static();

            AssertSql(
                @"@__customerId_0='1'

SELECT c.""LastName"", ""CustomerOrderCount""(@__customerId_0) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_0
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Nested_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Nested_Static();

            AssertSql(
                @"@__starCount_1='3'
@__customerId_0='3'

SELECT c.""LastName"", ""StarValue""(@__starCount_1, ""CustomerOrderCount""(@__customerId_0)) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_0
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Where_Correlated_Static()
        {
            base.Scalar_Function_Where_Correlated_Static();

            AssertSql(
                @"SELECT LOWER(CAST(c.""Id"" AS text))
FROM ""Customers"" AS c
WHERE ""IsTopCustomer""(c.""Id"")");
        }

        [Fact]
        public override void Scalar_Function_Where_Not_Correlated_Static()
        {
            base.Scalar_Function_Where_Not_Correlated_Static();

            AssertSql(
                @"@__startDate_0='2000-04-01T00:00:00' (Nullable = true) (DbType = DateTime)

SELECT c.""Id""
FROM ""Customers"" AS c
WHERE (""GetCustomerWithMostOrdersAfterDate""(@__startDate_0) = c.""Id"") AND (""GetCustomerWithMostOrdersAfterDate""(@__startDate_0) IS NOT NULL)
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Where_Parameter_Static()
        {
            base.Scalar_Function_Where_Parameter_Static();

            AssertSql(
                @"@__period_0='0'

SELECT c.""Id""
FROM ""Customers"" AS c
WHERE (c.""Id"" = ""GetCustomerWithMostOrdersAfterDate""(""GetReportingPeriodStartDate""(@__period_0))) AND (""GetCustomerWithMostOrdersAfterDate""(""GetReportingPeriodStartDate""(@__period_0)) IS NOT NULL)
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Where_Nested_Static()
        {
            base.Scalar_Function_Where_Nested_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c
WHERE c.""Id"" = ""GetCustomerWithMostOrdersAfterDate""(""GetReportingPeriodStartDate""(0))
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Correlated_Static()
        {
            base.Scalar_Function_Let_Correlated_Static();

            AssertSql(
                @"SELECT c.""LastName"", ""CustomerOrderCount""(c.""Id"") AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 2
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Not_Correlated_Static()
        {
            base.Scalar_Function_Let_Not_Correlated_Static();

            AssertSql(
                @"SELECT c.""LastName"", ""CustomerOrderCount""(2) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 2
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Not_Parameter_Static()
        {
            base.Scalar_Function_Let_Not_Parameter_Static();

            AssertSql(
                @"@__customerId_0='2'

SELECT c.""LastName"", ""CustomerOrderCount""(@__customerId_0) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_0
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Nested_Static()
        {
            base.Scalar_Function_Let_Nested_Static();

            AssertSql(
                @"@__starCount_0='3'
@__customerId_1='1'

SELECT c.""LastName"", ""StarValue""(@__starCount_0, ""CustomerOrderCount""(@__customerId_1)) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_1
LIMIT 2");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == AddOneStatic([c].Id))'")]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_Where_Static()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Where_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'orderby AddOneStatic([c].Id) asc'")]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Static()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_Select_Static()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Select_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c
ORDER BY c.""Id""");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == AddOneStatic(Abs(CustomerOrderCountWithClientStatic([c].Id))))'")]
        public override void Scalar_Nested_Function_Client_BCL_UDF_Static()
        {
            base.Scalar_Nested_Function_Client_BCL_UDF_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == AddOneStatic(CustomerOrderCountWithClientStatic(Abs([c].Id))))'")]
        public override void Scalar_Nested_Function_Client_UDF_BCL_Static()
        {
            base.Scalar_Nested_Function_Client_UDF_BCL_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == Abs(AddOneStatic(CustomerOrderCountWithClientStatic([c].Id))))'")]
        public override void Scalar_Nested_Function_BCL_Client_UDF_Static()
        {
            base.Scalar_Nested_Function_BCL_Client_UDF_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (1 == Abs(CustomerOrderCountWithClientStatic(AddOneStatic([c].Id))))'")]
        public override void Scalar_Nested_Function_BCL_UDF_Client_Static()
        {
            base.Scalar_Nested_Function_BCL_UDF_Client_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (1 == CustomerOrderCountWithClientStatic(Abs(AddOneStatic([c].Id))))'")]
        public override void Scalar_Nested_Function_UDF_BCL_Client_Static()
        {
            base.Scalar_Nested_Function_UDF_BCL_Client_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (1 == CustomerOrderCountWithClientStatic(AddOneStatic(Abs([c].Id))))'")]
        public override void Scalar_Nested_Function_UDF_Client_BCL_Static()
        {
            base.Scalar_Nested_Function_UDF_Client_BCL_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (3 == AddOneStatic(Abs([c].Id)))'")]
        public override void Scalar_Nested_Function_Client_BCL_Static()
        {
            base.Scalar_Nested_Function_Client_BCL_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == AddOneStatic(CustomerOrderCountWithClientStatic([c].Id)))'")]
        public override void Scalar_Nested_Function_Client_UDF_Static()
        {
            base.Scalar_Nested_Function_Client_UDF_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (3 == Abs(AddOneStatic([c].Id)))'")]
        public override void Scalar_Nested_Function_BCL_Client_Static()
        {
            base.Scalar_Nested_Function_BCL_Client_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_UDF_Static()
        {
            base.Scalar_Nested_Function_BCL_UDF_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c
WHERE 3 = ABS(""CustomerOrderCount""(c.""Id""))
LIMIT 2");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == CustomerOrderCountWithClientStatic(AddOneStatic([c].Id)))'")]
        public override void Scalar_Nested_Function_UDF_Client_Static()
        {
            base.Scalar_Nested_Function_UDF_Client_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_BCL_Static()
        {
            base.Scalar_Nested_Function_UDF_BCL_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c
WHERE 3 = ""CustomerOrderCount""(ABS(c.""Id""))
LIMIT 2");
        }

        [Fact]
        public override void Nullable_navigation_property_access_preserves_schema_for_sql_function()
        {
            base.Nullable_navigation_property_access_preserves_schema_for_sql_function();

            AssertSql(
                @"SELECT dbo.""IdentityString""(c.""FirstName"")
FROM ""Orders"" AS o
LEFT JOIN ""Customers"" AS c ON o.""CustomerId"" = c.""Id""
ORDER BY o.""Id""
LIMIT 1");
        }

        #endregion

        #region Instance

        [Fact]
        public override void Scalar_Function_Non_Static()
        {
            base.Scalar_Function_Non_Static();

            AssertSql(
                @"SELECT ""StarValue""(4, c.""Id"") AS ""Id"", ""DollarValue""(2, c.""LastName"") AS ""LastName""
FROM ""Customers"" AS c
WHERE c.""Id"" = 1
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Extension_Method_Instance()
        {
            base.Scalar_Function_Extension_Method_Instance();

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE (""IsDate""(c.""FirstName"") = FALSE) AND (""IsDate""(c.""FirstName"") IS NOT NULL)");
        }

        [Fact]
        public override void Scalar_Function_With_Translator_Translates_Instance()
        {
            base.Scalar_Function_With_Translator_Translates_Instance();

            AssertSql(
                @"@__customerId_0='3'

SELECT length(c.""LastName"")
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_0
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Constant_Parameter_Instance()
        {
            base.Scalar_Function_Constant_Parameter_Instance();

            AssertSql(
                @"@__customerId_1='1'

SELECT ""CustomerOrderCount""(@__customerId_1)
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Correlated_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Correlated_Instance();

            AssertSql(
                @"SELECT c.""LastName"", ""CustomerOrderCount""(c.""Id"") AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 1
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance();

            AssertSql(
                @"SELECT c.""LastName"", ""CustomerOrderCount""(1) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 1
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Parameter_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Parameter_Instance();

            AssertSql(
                @"@__customerId_0='1'

SELECT c.""LastName"", ""CustomerOrderCount""(@__customerId_0) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_0
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Nested_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Nested_Instance();

            AssertSql(
                @"@__starCount_2='3'
@__customerId_0='3'

SELECT c.""LastName"", ""StarValue""(@__starCount_2, ""CustomerOrderCount""(@__customerId_0)) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_0
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Where_Correlated_Instance()
        {
            base.Scalar_Function_Where_Correlated_Instance();

            AssertSql(
                @"SELECT LOWER(CAST(c.""Id"" AS text))
FROM ""Customers"" AS c
WHERE ""IsTopCustomer""(c.""Id"")");
        }

        [Fact]
        public override void Scalar_Function_Where_Not_Correlated_Instance()
        {
            base.Scalar_Function_Where_Not_Correlated_Instance();

            AssertSql(
                @"@__startDate_1='2000-04-01T00:00:00' (Nullable = true) (DbType = DateTime)

SELECT c.""Id""
FROM ""Customers"" AS c
WHERE (""GetCustomerWithMostOrdersAfterDate""(@__startDate_1) = c.""Id"") AND (""GetCustomerWithMostOrdersAfterDate""(@__startDate_1) IS NOT NULL)
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Where_Parameter_Instance()
        {
            base.Scalar_Function_Where_Parameter_Instance();

            AssertSql(
                @"@__period_1='0'

SELECT c.""Id""
FROM ""Customers"" AS c
WHERE (c.""Id"" = ""GetCustomerWithMostOrdersAfterDate""(""GetReportingPeriodStartDate""(@__period_1))) AND (""GetCustomerWithMostOrdersAfterDate""(""GetReportingPeriodStartDate""(@__period_1)) IS NOT NULL)
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Where_Nested_Instance()
        {
            base.Scalar_Function_Where_Nested_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c
WHERE c.""Id"" = ""GetCustomerWithMostOrdersAfterDate""(""GetReportingPeriodStartDate""(0))
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Correlated_Instance()
        {
            base.Scalar_Function_Let_Correlated_Instance();

            AssertSql(
                @"SELECT c.""LastName"", ""CustomerOrderCount""(c.""Id"") AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 2
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Not_Correlated_Instance()
        {
            base.Scalar_Function_Let_Not_Correlated_Instance();

            AssertSql(
                @"SELECT c.""LastName"", ""CustomerOrderCount""(2) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 2
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Not_Parameter_Instance()
        {
            base.Scalar_Function_Let_Not_Parameter_Instance();

            AssertSql(
                @"@__customerId_1='2'

SELECT c.""LastName"", ""CustomerOrderCount""(@__customerId_1) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_1
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Nested_Instance()
        {
            base.Scalar_Function_Let_Nested_Instance();

            AssertSql(
                @"@__starCount_1='3'
@__customerId_2='1'

SELECT c.""LastName"", ""StarValue""(@__starCount_1, ""CustomerOrderCount""(@__customerId_2)) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_2
LIMIT 2");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == __context_0.AddOneInstance([c].Id))'")]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_Where_Instance()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Where_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'orderby __context_0.AddOneInstance([c].Id) asc'")]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Instance()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c
ORDER BY c.""Id""");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == __context_0.AddOneInstance(Abs(__context_0.CustomerOrderCountWithClientInstance([c].Id))))'")]
        public override void Scalar_Nested_Function_Client_BCL_UDF_Instance()
        {
            base.Scalar_Nested_Function_Client_BCL_UDF_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == __context_0.AddOneInstance(__context_0.CustomerOrderCountWithClientInstance(Abs([c].Id))))'")]
        public override void Scalar_Nested_Function_Client_UDF_BCL_Instance()
        {
            base.Scalar_Nested_Function_Client_UDF_BCL_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == Abs(__context_0.AddOneInstance(__context_0.CustomerOrderCountWithClientInstance([c].Id))))'")]
        public override void Scalar_Nested_Function_BCL_Client_UDF_Instance()
        {
            base.Scalar_Nested_Function_BCL_Client_UDF_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (1 == Abs(__context_0.CustomerOrderCountWithClientInstance(__context_0.AddOneInstance([c].Id))))'")]
        public override void Scalar_Nested_Function_BCL_UDF_Client_Instance()
        {
            base.Scalar_Nested_Function_BCL_UDF_Client_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (1 == __context_0.CustomerOrderCountWithClientInstance(Abs(__context_0.AddOneInstance([c].Id))))'")]
        public override void Scalar_Nested_Function_UDF_BCL_Client_Instance()
        {
            base.Scalar_Nested_Function_UDF_BCL_Client_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (1 == __context_0.CustomerOrderCountWithClientInstance(__context_0.AddOneInstance(Abs([c].Id))))'")]
        public override void Scalar_Nested_Function_UDF_Client_BCL_Instance()
        {
            base.Scalar_Nested_Function_UDF_Client_BCL_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (3 == __context_0.AddOneInstance(Abs([c].Id)))'")]
        public override void Scalar_Nested_Function_Client_BCL_Instance()
        {
            base.Scalar_Nested_Function_Client_BCL_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == __context_0.AddOneInstance(__context_0.CustomerOrderCountWithClientInstance([c].Id)))'")]
        public override void Scalar_Nested_Function_Client_UDF_Instance()
        {
            base.Scalar_Nested_Function_Client_UDF_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (3 == Abs(__context_0.AddOneInstance([c].Id)))'")]
        public override void Scalar_Nested_Function_BCL_Client_Instance()
        {
            base.Scalar_Nested_Function_BCL_Client_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_UDF_Instance()
        {
            base.Scalar_Nested_Function_BCL_UDF_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c
WHERE 3 = ABS(""CustomerOrderCount""(c.""Id""))
LIMIT 2");
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'where (2 == __context_0.CustomerOrderCountWithClientInstance(__context_0.AddOneInstance([c].Id)))'")]
        public override void Scalar_Nested_Function_UDF_Client_Instance()
        {
            base.Scalar_Nested_Function_UDF_Client_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_BCL_Instance()
        {
            base.Scalar_Nested_Function_UDF_BCL_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c
WHERE 3 = ""CustomerOrderCount""(ABS(c.""Id""))
LIMIT 2");
        }

        #endregion

        #endregion

        protected class NpgsqlUDFSqlContext : UDFSqlContext
        {
            public NpgsqlUDFSqlContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // IsDate is a built-in SQL Server function, that in the base class is mapped as built-in, which means we
                // don't get any quotes. We remap it as non-built-in by including a (null) schema.
                var isDateMethodInfo = typeof(UDFSqlContext).GetMethod(nameof(IsDateStatic));
                modelBuilder.HasDbFunction(isDateMethodInfo)
                    .HasTranslation(args => SqlFunctionExpression.Create((string)null, "IsDate", args, isDateMethodInfo.ReturnType, null));
                var isDateMethodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(IsDateInstance));
                modelBuilder.HasDbFunction(isDateMethodInfo2)
                    .HasTranslation(args => SqlFunctionExpression.Create((string)null, "IsDate", args, isDateMethodInfo2.ReturnType, null));

                // Base class maps to len(), but in PostgreSQL it's called length()
                var methodInfo = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthStatic));
                modelBuilder.HasDbFunction(methodInfo)
                    .HasTranslation(args => SqlFunctionExpression.Create("length", args, methodInfo.ReturnType, null));
                var methodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthInstance));
                modelBuilder.HasDbFunction(methodInfo2)
                    .HasTranslation(args => SqlFunctionExpression.Create("length", args, methodInfo2.ReturnType, null));
            }
        }

        public class Npgsql : UdfFixtureBase
        {
            protected override string StoreName { get; } = "UDFDbFunctionNpgsqlTests";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            protected override Type ContextType { get; } = typeof(NpgsqlUDFSqlContext);

            protected override void Seed(DbContext context)
            {
                base.Seed(context);

                context.Database.ExecuteSqlRaw(
                    @"CREATE FUNCTION ""CustomerOrderCount"" (""customerId"" INTEGER)
                                                    RETURNS INTEGER
                                                    AS $$ SELECT COUNT(""Id"")::INTEGER FROM ""Orders"" WHERE ""CustomerId"" = $1 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlRaw(
                    @"CREATE FUNCTION ""StarValue"" (""starCount"" INTEGER, value TEXT)
                                                    RETURNS TEXT
                                                    AS $$ SELECT repeat('*', $1) || $2 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlRaw(
                    @"CREATE FUNCTION ""StarValue"" (""starCount"" INTEGER, value INTEGER)
                                                    RETURNS TEXT
                                                    AS $$ SELECT repeat('*', $1) || $2 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlRaw(
                    @"CREATE FUNCTION ""DollarValue"" (""starCount"" INTEGER, value TEXT)
                                                    RETURNS TEXT
                                                    AS $$ SELECT repeat('$', $1) || $2 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlRaw(
                    @"CREATE FUNCTION ""GetReportingPeriodStartDate"" (period INTEGER)
                                                    RETURNS DATE
                                                    AS $$ SELECT DATE '1998-01-01' $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlRaw(
                    @"CREATE FUNCTION ""GetCustomerWithMostOrdersAfterDate"" (searchDate TIMESTAMP)
                                                    RETURNS INTEGER
                                                    AS $$ SELECT ""CustomerId""
                                                          FROM ""Orders""
                                                          WHERE ""OrderDate"" > $1
                                                          GROUP BY ""CustomerId""
                                                          ORDER BY COUNT(""Id"") DESC
                                                          LIMIT 1 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlRaw(
                    @"CREATE FUNCTION ""IsTopCustomer"" (""customerId"" INTEGER)
                                                    RETURNS BOOL
                                                    AS $$ SELECT $1 = 1 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlRaw(
                    @"CREATE SCHEMA IF NOT EXISTS dbo");

                context.Database.ExecuteSqlRaw(
                    @"CREATE FUNCTION dbo.""IdentityString"" (""customerName"" TEXT)
                                                    RETURNS TEXT
                                                    AS $$ SELECT $1 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlRaw(
                    @"CREATE FUNCTION ""IsDate""(s TEXT)
                                                    RETURNS BOOLEAN AS $$
                                                    BEGIN
                                                        PERFORM s::DATE;
                                                        RETURN TRUE;
                                                    EXCEPTION WHEN OTHERS THEN
                                                        RETURN FALSE;
                                                    END;
                                                    $$ LANGUAGE PLPGSQL;");
                context.SaveChanges();
            }
        }

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
