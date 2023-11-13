using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindAggregateOperatorsQueryNpgsqlTest : NorthwindAggregateOperatorsQueryRelationalTestBase<
    NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindAggregateOperatorsQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Overriding to add equality tolerance because of floating point precision
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Average_over_max_subquery_is_client_eval(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
            selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Max(od => od.ProductID)),
            asserter: (a, b) => Assert.Equal(a, b, 15));

    public override async Task Contains_with_local_uint_array_closure(bool async)
    {
        await base.Contains_with_local_uint_array_closure(async);

        // Note: PostgreSQL doesn't support uint, but value converters make this into bigint
        AssertSql(
            """
@__ids_0={ '0', '1' } (DbType = Object)

SELECT e."EmployeeID", e."City", e."Country", e."FirstName", e."ReportsTo", e."Title"
FROM "Employees" AS e
WHERE e."EmployeeID" = ANY (@__ids_0)
""",
            //
            """
@__ids_0={ '0' } (DbType = Object)

SELECT e."EmployeeID", e."City", e."Country", e."FirstName", e."ReportsTo", e."Title"
FROM "Employees" AS e
WHERE e."EmployeeID" = ANY (@__ids_0)
""");
    }

    public override async Task Contains_with_local_nullable_uint_array_closure(bool async)
    {
        await base.Contains_with_local_nullable_uint_array_closure(async);

        // Note: PostgreSQL doesn't support uint, but value converters make this into bigint

        AssertSql(
            """
@__ids_0={ '0', '1' } (DbType = Object)

SELECT e."EmployeeID", e."City", e."Country", e."FirstName", e."ReportsTo", e."Title"
FROM "Employees" AS e
WHERE e."EmployeeID" = ANY (@__ids_0)
""",
            //
            """
@__ids_0={ '0' } (DbType = Object)

SELECT e."EmployeeID", e."City", e."Country", e."FirstName", e."ReportsTo", e."Title"
FROM "Employees" AS e
WHERE e."EmployeeID" = ANY (@__ids_0)
""");
    }

    public override Task Contains_with_local_anonymous_type_array_closure(bool async)
        // Aggregates. Issue #15937.
        => AssertTranslationFailed(() => base.Contains_with_local_anonymous_type_array_closure(async));

    public override Task Contains_with_local_tuple_array_closure(bool async)
        => Assert.ThrowsAsync<InvalidCastException>(() => base.Contains_with_local_tuple_array_closure(async: true));

    public override async Task Contains_with_local_enumerable_inline(bool async)
    {
        // Issue #31776
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await base.Contains_with_local_enumerable_inline(async));

        AssertSql();
    }

    public override async Task Contains_with_local_enumerable_inline_closure_mix(bool async)
    {
        // Issue #31776
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await base.Contains_with_local_enumerable_inline_closure_mix(async));

        AssertSql();
    }

    public override async Task Contains_with_local_non_primitive_list_closure_mix(bool async)
    {
        await base.Contains_with_local_non_primitive_list_closure_mix(async);

        AssertSql(
            """
@__Select_0={ 'ABCDE', 'ALFKI' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" = ANY (@__Select_0)
""");
    }

    public override async Task Contains_with_local_non_primitive_list_inline_closure_mix(bool async)
    {
        await base.Contains_with_local_non_primitive_list_inline_closure_mix(async);

        AssertSql(
            """
@__Select_0={ 'ABCDE', 'ALFKI' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" = ANY (@__Select_0)
""",
            //
            """
@__Select_0={ 'ABCDE', 'ANATR' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" = ANY (@__Select_0)
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
