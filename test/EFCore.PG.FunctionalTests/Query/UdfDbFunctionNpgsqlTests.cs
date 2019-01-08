using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
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
            => Fixture.TestSqlLoggerFactory.Clear();

        #region Scalar Tests

        #region Static

        [Fact]
        public override void Scalar_Function_Extension_Method_Static()
        {
            base.Scalar_Function_Extension_Method_Static();

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE CASE
    WHEN is_date(c.""FirstName"") = TRUE
    THEN TRUE::bool ELSE FALSE::bool
END = FALSE");
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

SELECT customer_order_count(@__customerId_0)
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Correlated_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Correlated_Static();

            AssertSql(
                @"SELECT c.""LastName"", customer_order_count(c.""Id"") AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 1
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static()
        {
            base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static();

            AssertSql(
                @"SELECT c.""LastName"", customer_order_count(1) AS ""OrderCount""
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

SELECT c.""LastName"", customer_order_count(@__customerId_0) AS ""OrderCount""
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

SELECT c.""LastName"", star_value(@__starCount_1, customer_order_count(@__customerId_0)) AS ""OrderCount""
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
WHERE is_top_customer(c.""Id"") = TRUE");
        }

        [Fact]
        public override void Scalar_Function_Where_Not_Correlated_Static()
        {
            base.Scalar_Function_Where_Not_Correlated_Static();

            AssertSql(
                @"@__startDate_0='2000-04-01T00:00:00' (Nullable = true) (DbType = DateTime)

SELECT c.""Id""
FROM ""Customers"" AS c
WHERE get_customer_with_most_orders_after_date(@__startDate_0) = c.""Id""
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
WHERE c.""Id"" = get_customer_with_most_orders_after_date(get_reporting_period_start_date(@__period_0))
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Where_Nested_Static()
        {
            base.Scalar_Function_Where_Nested_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c
WHERE c.""Id"" = get_customer_with_most_orders_after_date(get_reporting_period_start_date(0))
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Correlated_Static()
        {
            base.Scalar_Function_Let_Correlated_Static();

            AssertSql(
                @"SELECT c.""LastName"", customer_order_count(c.""Id"") AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 2
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Not_Correlated_Static()
        {
            base.Scalar_Function_Let_Not_Correlated_Static();

            AssertSql(
                @"SELECT c.""LastName"", customer_order_count(2) AS ""OrderCount""
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

SELECT c.""LastName"", customer_order_count(@__customerId_0) AS ""OrderCount""
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

SELECT c.""LastName"", star_value(@__starCount_0, customer_order_count(@__customerId_1)) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_1
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_Where_Static()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Where_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
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

        [Fact]
        public override void Scalar_Nested_Function_Client_BCL_UDF_Static()
        {
            base.Scalar_Nested_Function_Client_BCL_UDF_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_UDF_BCL_Static()
        {
            base.Scalar_Nested_Function_Client_UDF_BCL_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_Client_UDF_Static()
        {
            base.Scalar_Nested_Function_BCL_Client_UDF_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_UDF_Client_Static()
        {
            base.Scalar_Nested_Function_BCL_UDF_Client_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_BCL_Client_Static()
        {
            base.Scalar_Nested_Function_UDF_BCL_Client_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_Client_BCL_Static()
        {
            base.Scalar_Nested_Function_UDF_Client_BCL_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_BCL_Static()
        {
            base.Scalar_Nested_Function_Client_BCL_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_UDF_Static()
        {
            base.Scalar_Nested_Function_Client_UDF_Static();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
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
WHERE 3 = ABS(customer_order_count(c.""Id""))
LIMIT 2");
        }

        [Fact]
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
WHERE 3 = customer_order_count(ABS(c.""Id""))
LIMIT 2");
        }

        [Fact]
        public override void Nullable_navigation_property_access_preserves_schema_for_sql_function()
        {
            base.Nullable_navigation_property_access_preserves_schema_for_sql_function();

            AssertSql(
                @"SELECT dbo.""IdentityString""(""o.Customer"".""FirstName"")
FROM ""Orders"" AS o
LEFT JOIN ""Customers"" AS ""o.Customer"" ON o.""CustomerId"" = ""o.Customer"".""Id""
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
                @"SELECT star_value(4, c.""Id"") AS ""Id"", dollar_value(2, c.""LastName"") AS ""LastName""
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
WHERE CASE
    WHEN is_date(c.""FirstName"") = TRUE
    THEN TRUE::bool ELSE FALSE::bool
END = FALSE");
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

SELECT customer_order_count(@__customerId_1)
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Correlated_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Correlated_Instance();

            AssertSql(
                @"SELECT c.""LastName"", customer_order_count(c.""Id"") AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 1
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance()
        {
            base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance();

            AssertSql(
                @"SELECT c.""LastName"", customer_order_count(1) AS ""OrderCount""
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

SELECT c.""LastName"", customer_order_count(@__customerId_0) AS ""OrderCount""
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

SELECT c.""LastName"", star_value(@__starCount_2, customer_order_count(@__customerId_0)) AS ""OrderCount""
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
WHERE is_top_customer(c.""Id"") = TRUE");
        }

        [Fact]
        public override void Scalar_Function_Where_Not_Correlated_Instance()
        {
            base.Scalar_Function_Where_Not_Correlated_Instance();

            AssertSql(
                @"@__startDate_1='2000-04-01T00:00:00' (Nullable = true) (DbType = DateTime)

SELECT c.""Id""
FROM ""Customers"" AS c
WHERE get_customer_with_most_orders_after_date(@__startDate_1) = c.""Id""
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
WHERE c.""Id"" = get_customer_with_most_orders_after_date(get_reporting_period_start_date(@__period_1))
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Where_Nested_Instance()
        {
            base.Scalar_Function_Where_Nested_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c
WHERE c.""Id"" = get_customer_with_most_orders_after_date(get_reporting_period_start_date(0))
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Correlated_Instance()
        {
            base.Scalar_Function_Let_Correlated_Instance();

            AssertSql(
                @"SELECT c.""LastName"", customer_order_count(c.""Id"") AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 2
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Not_Correlated_Instance()
        {
            base.Scalar_Function_Let_Not_Correlated_Instance();

            AssertSql(
                @"SELECT c.""LastName"", customer_order_count(2) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = 2
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Not_Parameter_Instance()
        {
            base.Scalar_Function_Let_Not_Parameter_Instance();

            AssertSql(
                @"@__8__locals1_customerId_1='2'

SELECT c.""LastName"", customer_order_count(@__8__locals1_customerId_1) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = @__8__locals1_customerId_1
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Function_Let_Nested_Instance()
        {
            base.Scalar_Function_Let_Nested_Instance();

            AssertSql(
                @"@__starCount_1='3'
@__customerId_2='1'

SELECT c.""LastName"", star_value(@__starCount_1, customer_order_count(@__customerId_2)) AS ""OrderCount""
FROM ""Customers"" AS c
WHERE c.""Id"" = @__customerId_2
LIMIT 2");
        }

        [Fact]
        public override void Scalar_Nested_Function_Unwind_Client_Eval_Where_Instance()
        {
            base.Scalar_Nested_Function_Unwind_Client_Eval_Where_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
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

        [Fact]
        public override void Scalar_Nested_Function_Client_BCL_UDF_Instance()
        {
            base.Scalar_Nested_Function_Client_BCL_UDF_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_UDF_BCL_Instance()
        {
            base.Scalar_Nested_Function_Client_UDF_BCL_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_Client_UDF_Instance()
        {
            base.Scalar_Nested_Function_BCL_Client_UDF_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_BCL_UDF_Client_Instance()
        {
            base.Scalar_Nested_Function_BCL_UDF_Client_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_BCL_Client_Instance()
        {
            base.Scalar_Nested_Function_UDF_BCL_Client_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_UDF_Client_BCL_Instance()
        {
            base.Scalar_Nested_Function_UDF_Client_BCL_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_BCL_Instance()
        {
            base.Scalar_Nested_Function_Client_BCL_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
        public override void Scalar_Nested_Function_Client_UDF_Instance()
        {
            base.Scalar_Nested_Function_Client_UDF_Instance();

            AssertSql(
                @"SELECT c.""Id""
FROM ""Customers"" AS c");
        }

        [Fact]
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
WHERE 3 = ABS(customer_order_count(c.""Id""))
LIMIT 2");
        }

        [Fact]
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
WHERE 3 = customer_order_count(ABS(c.""Id""))
LIMIT 2");
        }

        #endregion

        #endregion

        // ReSharper disable once UnusedMember.Global
        protected class UDFNpgsqlContext : UDFSqlContext
        {
            public UDFNpgsqlContext(DbContextOptions options) : base(options) {}

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                var methodInfo = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthStatic));

                // ReSharper disable once AssignNullToNotNullAttribute
                modelBuilder.HasDbFunction(methodInfo)
                    .HasTranslation(args => new SqlFunctionExpression("length", methodInfo.ReturnType, args));

                var methodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthInstance));

                // ReSharper disable once AssignNullToNotNullAttribute
                modelBuilder.HasDbFunction(methodInfo2)
                    .HasTranslation(args => new SqlFunctionExpression("length", methodInfo2.ReturnType, args));
            }
        }

        // We override our own context because we don't support functions with upper-case names for now,
        // and this is how things are hard-coded in UDFSqlContext
        // See https://github.com/aspnet/EntityFrameworkCore/issues/12757
        protected class NpgsqlUDFSqlContext : UDFSqlContext
        {
            public NpgsqlUDFSqlContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // ReSharper disable AssignNullToNotNullAttribute
                base.OnModelCreating(modelBuilder);

                //Static
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountStatic))).HasName("customer_order_count");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountWithClientStatic))).HasName("customer_order_count");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(StarValueStatic))).HasName("star_value");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsTopCustomerStatic))).HasName("is_top_customer");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateStatic))).HasName("get_customer_with_most_orders_after_date");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetReportingPeriodStartDateStatic))).HasName("get_reporting_period_start_date");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsDateStatic))).HasSchema("").HasName("is_date");

                var methodInfo = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthStatic));

                modelBuilder.HasDbFunction(methodInfo)
                    .HasTranslation(args => new SqlFunctionExpression("length", methodInfo.ReturnType, args));

                //Instance
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountInstance))).HasName("customer_order_count");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountWithClientInstance))).HasName("customer_order_count");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(StarValueInstance))).HasName("star_value");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsTopCustomerInstance))).HasName("is_top_customer");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateInstance))).HasName("get_customer_with_most_orders_after_date");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetReportingPeriodStartDateInstance))).HasName("get_reporting_period_start_date");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsDateInstance))).HasSchema("").HasName("is_date");

                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(DollarValueInstance))).HasName("dollar_value");

                var methodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthInstance));

                modelBuilder.HasDbFunction(methodInfo2)
                    .HasTranslation(args => new SqlFunctionExpression("length", methodInfo2.ReturnType, args));
                // ReSharper restore AssignNullToNotNullAttribute
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

                // TODO: The OR REPLACE shouldn't be needed, but the fixture attempts to recreate the function and PG errors
                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION customer_order_count (""customerId"" INTEGER)
                                                    RETURNS INTEGER
                                                    AS $$ SELECT COUNT(""Id"")::INTEGER FROM ""Orders"" WHERE ""CustomerId"" = $1 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION star_value (""starCount"" INTEGER, value TEXT)
                                                    RETURNS TEXT
                                                    AS $$ SELECT repeat('*', $1) || $2 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION star_value (""starCount"" INTEGER, value INTEGER)
                                                    RETURNS TEXT
                                                    AS $$ SELECT repeat('*', $1) || $2 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION dollar_value (""starCount"" INTEGER, value TEXT)
                                                    RETURNS TEXT
                                                    AS $$ SELECT repeat('$', $1) || $2 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION get_reporting_period_start_date (period INTEGER)
                                                    RETURNS DATE
                                                    AS $$ SELECT DATE '1998-01-01' $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION get_customer_with_most_orders_after_date (searchDate TIMESTAMP)
                                                    RETURNS INTEGER
                                                    AS $$ SELECT ""CustomerId""
                                                          FROM ""Orders""
                                                          WHERE ""OrderDate"" > $1
                                                          GROUP BY ""CustomerId""
                                                          ORDER BY COUNT(""Id"") DESC
                                                          LIMIT 1 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION is_top_customer (""customerId"" INTEGER)
                                                    RETURNS BOOL
                                                    AS $$ SELECT $1 = 1 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlCommand(
                    @"CREATE SCHEMA IF NOT EXISTS dbo");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION dbo.""IdentityString"" (""customerName"" TEXT)
                                                    RETURNS TEXT
                                                    AS $$ SELECT $1 $$
                                                    LANGUAGE SQL");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION is_date(s TEXT)
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
