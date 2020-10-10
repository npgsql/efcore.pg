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
    public class NorthwindMiscellaneousQueryNpgsqlTest : NorthwindMiscellaneousQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public NorthwindMiscellaneousQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Query_expression_with_to_string_and_contains(bool async)
        {
            await base.Query_expression_with_to_string_and_contains(async);
            AssertContainsSqlFragment(@"strpos(CAST(o.""EmployeeID"" AS text), '10') > 0");
        }

        public override async Task Select_expression_date_add_year(bool async)
        {
            await base.Select_expression_date_add_year(async);

            AssertSql(
                @"SELECT o.""OrderDate"" + INTERVAL '1 years' AS ""OrderDate""
FROM ""Orders"" AS o
WHERE (o.""OrderDate"" IS NOT NULL)");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Select_expression_date_add_year_param(bool async)
        {
            var years = 2;

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddYears(years)
                        }),
                e => e.OrderDate);

            AssertSql(
                @"@__years_0='2'

SELECT o.""OrderDate"" + CAST((@__years_0::text || ' years') AS interval) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE (o.""OrderDate"" IS NOT NULL)");
        }

        // TODO: Array tests can probably move to the dedicated ArrayQueryTest suite

        #region Array contains

        // Note that this also takes care of array.Any(x => x == y)
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_Contains_constant(bool async)
        {
            await AssertQuery(
                async,
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
        public async Task Array_Contains_parameter(bool async)
        {
            var regions = new[] { "UK", "SP" };

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => regions.Contains(c.Region)),
                entryCount: 6);

            // Instead of c.""Region"" IN ('UK', 'SP') we generate the PostgreSQL-specific x = ANY (a, b, c), which can
            // be parameterized.
            // Ideally parameter sniffing would allow us to produce SQL without the null check since the regions array doesn't contain one
            // (see https://github.com/aspnet/EntityFrameworkCore/issues/17598).
            AssertSql(
                @"@__regions_0='System.String[]' (DbType = Object)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""Region"" = ANY (@__regions_0) OR ((c.""Region"" IS NULL) AND (array_position(@__regions_0, NULL) IS NOT NULL))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_Contains_parameter_with_null(bool async)
        {
            var regions = new[] { "UK", "SP", null };

            await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => regions.Contains(c.Region)),
            entryCount: 66);

            // Instead of c.""Region"" IN ('UK', 'SP') we generate the PostgreSQL-specific x = ANY (a, b, c), which can
            // be parameterized.
            // Ideally parameter sniffing would allow us to produce SQL with an optimized null check (no need to check the array server-side)
            // (see https://github.com/aspnet/EntityFrameworkCore/issues/17598).
            AssertSql(
                @"@__regions_0='System.String[]' (DbType = Object)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""Region"" = ANY (@__regions_0) OR ((c.""Region"" IS NULL) AND (array_position(@__regions_0, NULL) IS NOT NULL))");
        }

        #endregion Array contains

        #region Any/All Like

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_Any_Like(bool async)
        {
            using var context = CreateContext();

            var collection = new[] { "A%", "B%", "C%" };
            var query = context.Set<Customer>().Where(c => collection.Any(y => EF.Functions.Like(c.Address, y)));
            var result = async ? await query.ToListAsync() : query.ToList();

            Assert.Equal(new[]
            {
                "ANATR", "BERGS", "BOLID", "CACTU", "COMMI", "CONSH", "FISSA", "FRANK", "GODOS", "GOURL", "HILAA",
                "HUNGC", "LILAS", "LINOD", "PERIC", "QUEEN", "RANCH", "RICAR", "SUPRD", "TORTU", "TRADH", "WANDK"
            }, result.Select(e => e.CustomerID));

            AssertSql(
                @"@__collection_0='System.String[]' (DbType = Object)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""Address"" LIKE ANY (@__collection_0)");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_All_Like(bool async)
        {
            using var context = CreateContext();

            var collection = new[] { "A%", "B%", "C%" };
            var query = context.Set<Customer>().Where(c => collection.All(y => EF.Functions.Like(c.Address, y)));
            var result = async ? await query.ToListAsync() : query.ToList();

            Assert.Empty(result);

            AssertSql(
                @"@__collection_0='System.String[]' (DbType = Object)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""Address"" LIKE ALL (@__collection_0)");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_Any_ILike(bool async)
        {
            using var context = CreateContext();

            var collection = new[] { "a%", "b%", "c%" };
            var query = context.Set<Customer>().Where(c => collection.Any(y => EF.Functions.ILike(c.Address, y)));
            var result = async ? await query.ToListAsync() : query.ToList();

            Assert.Equal(new[]
            {
                "ANATR", "BERGS", "BOLID", "CACTU", "COMMI", "CONSH", "FISSA", "FRANK", "GODOS", "GOURL", "HILAA",
                "HUNGC", "LILAS", "LINOD", "PERIC", "QUEEN", "RANCH", "RICAR", "SUPRD", "TORTU", "TRADH", "WANDK"
            }, result.Select(e => e.CustomerID));

            AssertSql(
                @"@__collection_0='System.String[]' (DbType = Object)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""Address"" ILIKE ANY (@__collection_0)");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Array_All_ILike(bool async)
        {
            using var context = CreateContext();

            var collection = new[] { "a%", "b%", "c%" };
            var query = context.Set<Customer>().Where(c => collection.All(y => EF.Functions.ILike(c.Address, y)));
            var result = async ? await query.ToListAsync() : query.ToList();

            Assert.Empty(result);

            AssertSql(
                @"@__collection_0='System.String[]' (DbType = Object)

SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE c.""Address"" ILIKE ALL (@__collection_0)");
        }

        #endregion Any/All Like

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        void AssertContainsSqlFragment(string expectedFragment)
            => Assert.Contains(Fixture.TestSqlLoggerFactory.SqlStatements, s => s.Contains(expectedFragment));
    }
}
