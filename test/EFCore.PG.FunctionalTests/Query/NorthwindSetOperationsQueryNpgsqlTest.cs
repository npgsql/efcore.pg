using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindSetOperationsQueryNpgsqlTest
    : NorthwindSetOperationsQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindSetOperationsQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Leftmost_nulls_with_two_unions(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID })
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)o.OrderID, o.CustomerID })));

        AssertSql(
            """
SELECT NULL AS "OrderID", o."CustomerID"
FROM "Orders" AS o
UNION
SELECT o0."OrderID", o0."CustomerID"
FROM "Orders" AS o0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Leftmost_nulls_with_three_unions(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID })
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID }))
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)o.OrderID, o.CustomerID })));

        AssertSql(
            """
SELECT NULL::int AS "OrderID", o."CustomerID"
FROM "Orders" AS o
UNION
SELECT NULL AS "OrderID", o0."CustomerID"
FROM "Orders" AS o0
UNION
SELECT o1."OrderID", o1."CustomerID"
FROM "Orders" AS o1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Leftmost_nulls_with_four_unions(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID })
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID }))
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID }))
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)o.OrderID, o.CustomerID })));

        AssertSql(
            """
SELECT NULL::int AS "OrderID", o."CustomerID"
FROM "Orders" AS o
UNION
SELECT NULL AS "OrderID", o0."CustomerID"
FROM "Orders" AS o0
UNION
SELECT NULL AS "OrderID", o1."CustomerID"
FROM "Orders" AS o1
UNION
SELECT o2."OrderID", o2."CustomerID"
FROM "Orders" AS o2
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Leftmost_nulls_with_five_unions(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID })
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID }))
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID }))
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID }))
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)o.OrderID, o.CustomerID })));

        AssertSql(
            """
SELECT NULL::int AS "OrderID", o."CustomerID"
FROM "Orders" AS o
UNION
SELECT NULL AS "OrderID", o0."CustomerID"
FROM "Orders" AS o0
UNION
SELECT NULL AS "OrderID", o1."CustomerID"
FROM "Orders" AS o1
UNION
SELECT NULL AS "OrderID", o2."CustomerID"
FROM "Orders" AS o2
UNION
SELECT o3."OrderID", o3."CustomerID"
FROM "Orders" AS o3
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Leftmost_nulls_in_tables_and_predicate(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID })
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID }))
                .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)o.OrderID, o.CustomerID }))
                .Where(
                    o => o.CustomerID
                        == ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID })
                            .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)null, o.CustomerID }))
                            .Union(ss.Set<Order>().Select(o => new { OrderID = (int?)o.OrderID, o.CustomerID }))
                            .OrderBy(o => o.CustomerID)
                            .First()
                            .CustomerID));

        AssertSql(
            """
SELECT u0."OrderID", u0."CustomerID"
FROM (
    SELECT NULL::int AS "OrderID", o."CustomerID"
    FROM "Orders" AS o
    UNION
    SELECT NULL AS "OrderID", o0."CustomerID"
    FROM "Orders" AS o0
    UNION
    SELECT o1."OrderID", o1."CustomerID"
    FROM "Orders" AS o1
) AS u0
WHERE u0."CustomerID" = (
    SELECT u2."CustomerID"
    FROM (
        SELECT NULL::int AS "OrderID", o2."CustomerID"
        FROM "Orders" AS o2
        UNION
        SELECT NULL AS "OrderID", o3."CustomerID"
        FROM "Orders" AS o3
        UNION
        SELECT o4."OrderID", o4."CustomerID"
        FROM "Orders" AS o4
    ) AS u2
    ORDER BY u2."CustomerID" NULLS FIRST
    LIMIT 1) OR (u0."CustomerID" IS NULL AND (
    SELECT u2."CustomerID"
    FROM (
        SELECT NULL::int AS "OrderID", o2."CustomerID"
        FROM "Orders" AS o2
        UNION
        SELECT NULL AS "OrderID", o3."CustomerID"
        FROM "Orders" AS o3
        UNION
        SELECT o4."OrderID", o4."CustomerID"
        FROM "Orders" AS o4
    ) AS u2
    ORDER BY u2."CustomerID" NULLS FIRST
    LIMIT 1) IS NULL)
""");
    }

    public override async Task Client_eval_Union_FirstOrDefault(bool async)
    {
        // Client evaluation in projection. Issue #16243.
        Assert.Equal(
            RelationalStrings.SetOperationsNotAllowedAfterClientEvaluation,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_eval_Union_FirstOrDefault(async))).Message);

        AssertSql();
    }

    public override async Task Set_operations_over_null_checked_to_one_non_entity_subquery(bool async)
    {
        await base.Set_operations_over_null_checked_to_one_non_entity_subquery(async);

        AssertSql(
            """
SELECT c."CustomerID", c."City", CASE
    WHEN o1.marker IS NOT NULL THEN o1."OrderID"
    ELSE 0
END AS "LatestOrderID", o1."OrderDate" AS "LatestOrderDate"
FROM "Customers" AS c
LEFT JOIN (
    SELECT o0."OrderID", o0."OrderDate", o0.marker, o0."CustomerID"
    FROM (
        SELECT o."OrderID", o."OrderDate", 1 AS marker, o."CustomerID", ROW_NUMBER() OVER(PARTITION BY o."CustomerID" ORDER BY o."OrderDate" DESC NULLS LAST, o."OrderID" DESC NULLS LAST) AS row
        FROM "Orders" AS o
    ) AS o0
    WHERE o0.row <= 1
) AS o1 ON c."CustomerID" = o1."CustomerID"
UNION ALL
SELECT c0."CustomerID", c0."City", 0 AS "LatestOrderID", NULL AS "LatestOrderDate"
FROM "Customers" AS c0
WHERE NOT EXISTS (
    SELECT 1
    FROM "Orders" AS o2
    WHERE c0."CustomerID" = o2."CustomerID")
""",
            //
            """
SELECT c."CustomerID", c."City", 0 AS "LatestOrderID", NULL AS "LatestOrderDate"
FROM "Customers" AS c
WHERE NOT EXISTS (
    SELECT 1
    FROM "Orders" AS o
    WHERE c."CustomerID" = o."CustomerID")
UNION
SELECT c0."CustomerID", c0."City", CASE
    WHEN o2.marker IS NULL THEN 0
    ELSE o2."OrderID"
END AS "LatestOrderID", o2."OrderDate" AS "LatestOrderDate"
FROM "Customers" AS c0
LEFT JOIN (
    SELECT o1."OrderID", o1."OrderDate", o1.marker, o1."CustomerID"
    FROM (
        SELECT o0."OrderID", o0."OrderDate", 1 AS marker, o0."CustomerID", ROW_NUMBER() OVER(PARTITION BY o0."CustomerID" ORDER BY o0."OrderDate" DESC NULLS LAST, o0."OrderID" DESC NULLS LAST) AS row
        FROM "Orders" AS o0
    ) AS o1
    WHERE o1.row <= 1
) AS o2 ON c0."CustomerID" = o2."CustomerID"
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
