using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public partial class SimpleQueryNpgsqlTest : SimpleQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public SimpleQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/18674")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Default_if_empty_top_level_arg(bool isAsync)
            => base.Default_if_empty_top_level_arg(isAsync);

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/18674")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Default_if_empty_top_level_arg_followed_by_projecting_constant(bool isAsync)
            => base.Default_if_empty_top_level_arg_followed_by_projecting_constant(isAsync);

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/17379")]
        public override Task SelectMany_correlated_with_outer_2(bool isAsync)
            => base.SelectMany_correlated_with_outer_2(isAsync);

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/17379")]
        public override Task SelectMany_correlated_with_outer_4(bool isAsync)
            => base.SelectMany_correlated_with_outer_4(isAsync);

        #region Overrides

        public override async Task Select_expression_date_add_year(bool isAsync)
        {
            await base.Select_expression_date_add_year(isAsync);

            AssertSql(
                @"SELECT o.""OrderDate"" + INTERVAL '1 years' AS ""OrderDate""
FROM ""Orders"" AS o
WHERE (o.""OrderDate"" IS NOT NULL)");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Select_expression_date_add_year_param(bool isAsync)
        {
            var years = 2;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddYears(years)
                        }),
                e => e.OrderDate);

            AssertSql(
                @"@__years_0='2'

SELECT o.""OrderDate"" + CAST((@__years_0 || ' years') AS interval) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE (o.""OrderDate"" IS NOT NULL)");
        }

        public override async Task Contains_with_local_uint_array_closure(bool isAsync)
        {
            await base.Contains_with_local_uint_array_closure(isAsync);

            // This test invokes Contains() on a uint array, but PostgreSQL doesn't support uint. As a result,
            // we can't do the PostgreSQL-specific x = ANY (a,b,c) optimization and allow EF Core to expand the
            // parameterized array to constant instead.

            AssertSql(
                @"SELECT e.""EmployeeID"", e.""City"", e.""Country"", e.""FirstName"", e.""ReportsTo"", e.""Title""
FROM ""Employees"" AS e
WHERE e.""EmployeeID"" IN (0, 1)",
                //
                @"SELECT e.""EmployeeID"", e.""City"", e.""Country"", e.""FirstName"", e.""ReportsTo"", e.""Title""
FROM ""Employees"" AS e
WHERE e.""EmployeeID"" IN (0)");
        }

        public override async Task Contains_with_local_nullable_uint_array_closure(bool isAsync)
        {
            await base.Contains_with_local_nullable_uint_array_closure(isAsync);

            // As above in Contains_with_local_int_array_closure

            AssertSql(
                @"SELECT e.""EmployeeID"", e.""City"", e.""Country"", e.""FirstName"", e.""ReportsTo"", e.""Title""
FROM ""Employees"" AS e
WHERE e.""EmployeeID"" IN (0, 1)",
                //
                @"SELECT e.""EmployeeID"", e.""City"", e.""Country"", e.""FirstName"", e.""ReportsTo"", e.""Title""
FROM ""Employees"" AS e
WHERE e.""EmployeeID"" IN (0)");
        }

        #endregion

        #region PadLeft, PadRight

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadLeft_with_constant(bool isAsync)
            => AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(20).EndsWith("Walserweg 21")),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadLeft_char_with_constant(bool isAsync)
            => AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(20, 'a').EndsWith("Walserweg 21")),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadLeft_with_parameter(bool isAsync)
        {
            var length = 20;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(length).EndsWith("Walserweg 21")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadLeft_char_with_parameter(bool isAsync)
        {
            var length = 20;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(length, 'a').EndsWith("Walserweg 21")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadRight_with_constant(bool isAsync)
            => AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadRight(20).StartsWith("Walserweg 21")),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadRight_char_with_constant(bool isAsync)
            => AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadRight(20).StartsWith("Walserweg 21")),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadRight_with_parameter(bool isAsync)
        {
            var length = 20;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadRight(length).StartsWith("Walserweg 21")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadRight_char_with_parameter(bool isAsync)
        {
            var length = 20;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadRight(length, 'a').StartsWith("Walserweg 21")),
                entryCount: 1);
        }

        #endregion

        #region Substring

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task Substring_without_length_with_Index_of(bool isAsync)
            => AssertQuery(
                isAsync,
                ss => ss.Set<Customer>()
                    .Where(x => x.Address == "Walserweg 21")
                    .Where(x => x.Address.Substring(x.Address.IndexOf("e")) == "erweg 21"),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task Substring_without_length_with_constant(bool isAsync)
            => AssertQuery(
                isAsync,
                //Walserweg 21
                cs => cs.Set<Customer>().Where(x => x.Address.Substring(5) == "rweg 21"),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task Substring_without_length_with_closure(bool isAsync)
        {
            var startIndex = 5;
            return AssertQuery(
                isAsync,
                //Walserweg 21
                ss => ss.Set<Customer>().Where(x => x.Address.Substring(startIndex) == "rweg 21"),
                entryCount: 1);
        }

        #endregion

        #region Array contains

        // Note that this also takes care of array.Any(x => x == y)
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_Contains_constant(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => new[] { "ALFKI", "ANATR" }.Contains(c.CustomerID)),
                entryCount: 2);

            // Note: for constant lists there's no advantage in using the PostgreSQL-specific x = ANY (a b, c), unlike
            // for parameterized lists.

            AssertSql(
                @"SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""CustomerID"" IN ('ALFKI', 'ANATR')");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_Contains_parameter(bool isAsync)
        {
            var ids = new[] { "ALFKI", "ANATR" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
                entryCount: 2);

            // Instead of c.""CustomerID"" x in ('ALFKI', 'ANATR') we should generate the PostgreSQL-specific x = ANY (a, b, c), which can
            // be parameterized. This is currently disabled because of null semantics, until EF Core supports caching
            // based on parameter values (https://github.com/aspnet/EntityFrameworkCore/issues/15892#issuecomment-513399906).

            AssertSql(
                @"SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""CustomerID"" IN ('ALFKI', 'ANATR')");
//            AssertSql(
//                @"@__ids_0='System.String[]' (DbType = Object)
//
//SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
//FROM ""Customers"" AS c
//WHERE c.""CustomerID"" = ANY (@__ids_0)");
        }

        #endregion Array contains

        #region Any/All Like

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_Any_Like(bool isAsync)
        {
            var collection = new[] { "A%", "B%", "C%" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => collection.Any(y => EF.Functions.Like(c.Address, y))),
                entryCount: 22);

            AssertSql(
                @"@__collection_0='System.String[]' (DbType = Object)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""Address"" LIKE ANY (@__collection_0)");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_All_Like(bool isAsync)
        {
            var collection = new[] { "A%", "B%", "C%" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => collection.All(y => EF.Functions.Like(c.Address, y))));

            AssertSql(
                @"@__collection_0='System.String[]' (DbType = Object)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""Address"" LIKE ALL (@__collection_0)");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_Any_ILike(bool isAsync)
        {
            var collection = new[] { "a%", "b%", "c%" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => collection.Any(y => EF.Functions.ILike(c.Address, y))),
                entryCount: 22);

            AssertSql(
                @"@__collection_0='System.String[]' (DbType = Object)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""Address"" ILIKE ANY (@__collection_0)");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_All_ILike(bool isAsync)
        {
            var collection = new[] { "a%", "b%", "c%" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => collection.All(y => EF.Functions.ILike(c.Address, y))));

            AssertSql(
                @"@__collection_0='System.String[]' (DbType = Object)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""Address"" ILIKE ALL (@__collection_0)");
        }

        #endregion Any/All Like

        #region Helpers

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        #endregion
    }
}
