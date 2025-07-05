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

    // https://github.com/dotnet/efcore/issues/36311
    public override Task Contains_with_parameter_list_value_type_id(bool async)
        => Assert.ThrowsAsync<UnreachableException>(() => base.Contains_with_parameter_list_value_type_id(async));

    // https://github.com/dotnet/efcore/issues/36311
    public override Task IReadOnlySet_Contains_with_parameter(bool async)
        => Assert.ThrowsAsync<UnreachableException>(() => base.IReadOnlySet_Contains_with_parameter(async));

    // https://github.com/dotnet/efcore/issues/36311
    public override Task List_Contains_with_parameter_list(bool async)
        => Assert.ThrowsAsync<UnreachableException>(() => base.List_Contains_with_parameter_list(async));

    // https://github.com/dotnet/efcore/issues/36311
    public override Task IImmutableSet_Contains_with_parameter(bool async)
        => Assert.ThrowsAsync<UnreachableException>(() => base.IImmutableSet_Contains_with_parameter(async));

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
@ids={ '0', '1' } (DbType = Object)

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
@ids={ '0', '1' } (DbType = Object)

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
@p={ 'ABCDE', 'ALFKI' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" = ANY (array_remove(@p, NULL))
""",
            //
            """
@p={ 'ABCDE', 'ANATR' } (DbType = Object)

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
@Select={ 'ABCDE', 'ALFKI' } (DbType = Object)

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
@Select={ 'ABCDE', 'ALFKI' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" = ANY (@Select)
""",
            //
            """
@Select={ 'ABCDE', 'ANATR' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" = ANY (@Select)
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
