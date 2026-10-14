namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocQueryFiltersQueryNpgsqlTest(NonSharedFixture fixture)
    : AdHocQueryFiltersQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public override async Task GroupBy_aggregate_over_required_navigation_with_query_filter(bool async)
    {
        await base.GroupBy_aggregate_over_required_navigation_with_query_filter(async);

        AssertSql(
            """
SELECT d."GroupId" AS "Key", count(*)::int AS "Count", (
    SELECT max(p0."Value")
    FROM "Dependents" AS d0
    INNER JOIN (
        SELECT p."Id", p."Value"
        FROM "Principals" AS p
        WHERE NOT (p."Filtered")
    ) AS p0 ON d0."PrincipalId" = p0."Id"
    WHERE d."GroupId" = d0."GroupId") AS "MaxValue"
FROM "Dependents" AS d
GROUP BY d."GroupId"
""",
            //
            """
SELECT d."GroupId" AS "Key", count(*)::int AS "Count", max(p."Value") AS "MaxValue"
FROM "Dependents" AS d
INNER JOIN "Principals" AS p ON d."PrincipalId" = p."Id"
GROUP BY d."GroupId"
""");
    }
}
