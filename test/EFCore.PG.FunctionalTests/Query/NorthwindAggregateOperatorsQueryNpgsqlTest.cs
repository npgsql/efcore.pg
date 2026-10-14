using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

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
    public override async Task Average_over_max_subquery(bool async)
    {
        await AssertAverage(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
            selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Max(od => od.ProductID)),
            asserter: (e, a) => Assert.Equal(e, a, 10));

        AssertSql(
            """
@p='3'

SELECT avg((
    SELECT avg(CAST(5 + (
        SELECT max(o0."ProductID")
        FROM "Order Details" AS o0
        WHERE o."OrderID" = o0."OrderID") AS double precision))
    FROM "Orders" AS o
    WHERE c0."CustomerID" = o."CustomerID")::numeric)
FROM (
    SELECT c."CustomerID"
    FROM "Customers" AS c
    ORDER BY c."CustomerID" NULLS FIRST
    LIMIT @p
) AS c0
""");
    }

    public override async Task Contains_with_local_uint_array_closure(bool async)
    {
        await base.Contains_with_local_uint_array_closure(async);

        // Note: PostgreSQL doesn't support uint, but value converters make this into bigint
        AssertSql(
            """
@ids={ '0'
'1' } (DbType = Object)

SELECT e."EmployeeID", e."City", e."Country", e."FirstName", e."ReportsTo", e."Title"
FROM "Employees" AS e
WHERE e."EmployeeID" = ANY (@ids)
""",
            //
            """
@ids={ '0' } (DbType = Object)

SELECT e."EmployeeID", e."City", e."Country", e."FirstName", e."ReportsTo", e."Title"
FROM "Employees" AS e
WHERE e."EmployeeID" = ANY (@ids)
""");
    }

    public override async Task Contains_with_local_nullable_uint_array_closure(bool async)
    {
        await base.Contains_with_local_nullable_uint_array_closure(async);

        // Note: PostgreSQL doesn't support uint, but value converters make this into bigint

        AssertSql(
            """
@ids={ '0'
'1' } (DbType = Object)

SELECT e."EmployeeID", e."City", e."Country", e."FirstName", e."ReportsTo", e."Title"
FROM "Employees" AS e
WHERE e."EmployeeID" = ANY (@ids)
""",
            //
            """
@ids={ '0' } (DbType = Object)

SELECT e."EmployeeID", e."City", e."Country", e."FirstName", e."ReportsTo", e."Title"
FROM "Employees" AS e
WHERE e."EmployeeID" = ANY (@ids)
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
        await base.Contains_with_local_enumerable_inline_closure_mix(async);

        AssertSql(
            """
@p={ 'ABCDE'
'ALFKI' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" = ANY (array_remove(@p, NULL))
""",
            //
            """
@p={ 'ABCDE'
'ANATR' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" = ANY (array_remove(@p, NULL))
""");
    }

    public override async Task Contains_with_local_non_primitive_list_closure_mix(bool async)
    {
        await base.Contains_with_local_non_primitive_list_closure_mix(async);

        AssertSql(
            """
@Select={ 'ABCDE'
'ALFKI' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" = ANY (@Select)
""");
    }

    public override async Task Contains_with_local_non_primitive_list_inline_closure_mix(bool async)
    {
        await base.Contains_with_local_non_primitive_list_inline_closure_mix(async);

        AssertSql(
            """
@Select={ 'ABCDE'
'ALFKI' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" = ANY (@Select)
""",
            //
            """
@Select={ 'ABCDE'
'ANATR' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" = ANY (@Select)
""");
    }

    public override async Task Sum_over_expression_with_outer_reference(bool async)
    {
        await base.Sum_over_expression_with_outer_reference(async);

        AssertSql(
            """
SELECT o."OrderID", (
    SELECT COALESCE(sum(o0."ProductID" * o."OrderID"), 0)::int
    FROM "Order Details" AS o0
    WHERE o."OrderID" = o0."OrderID") AS "Total"
FROM "Orders" AS o
""");
    }

    public override async Task Sum_over_members_of_single_result_subquery_with_outer_reference(bool async)
    {
        await base.Sum_over_members_of_single_result_subquery_with_outer_reference(async);

        AssertSql(
            """
SELECT c."CustomerID", (
    SELECT COALESCE(sum(o2."ProductID" * length(c."CustomerID")::int), 0)::int
    FROM "Orders" AS o
    LEFT JOIN (
        SELECT o1."ProductID", o1."OrderID0"
        FROM (
            SELECT o0."ProductID", o0."OrderID" AS "OrderID0", ROW_NUMBER() OVER(PARTITION BY o0."OrderID" ORDER BY o0."ProductID" NULLS FIRST) AS row
            FROM "Order Details" AS o0
        ) AS o1
        WHERE o1.row <= 1
    ) AS o2 ON o."OrderID" = o2."OrderID0"
    WHERE c."CustomerID" = o."CustomerID") AS "TotalProducts", (
    SELECT COALESCE(sum(o6."OrderID" * length(c."CustomerID")::int), 0)::int
    FROM "Orders" AS o3
    LEFT JOIN (
        SELECT o5."OrderID", o5."OrderID0"
        FROM (
            SELECT o4."OrderID", o4."OrderID" AS "OrderID0", ROW_NUMBER() OVER(PARTITION BY o4."OrderID" ORDER BY o4."ProductID" NULLS FIRST) AS row
            FROM "Order Details" AS o4
        ) AS o5
        WHERE o5.row <= 1
    ) AS o6 ON o3."OrderID" = o6."OrderID0"
    WHERE c."CustomerID" = o3."CustomerID") AS "TotalOrders"
FROM "Customers" AS c
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
