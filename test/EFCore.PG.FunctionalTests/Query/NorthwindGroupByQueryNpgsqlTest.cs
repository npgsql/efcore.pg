using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindGroupByQueryNpgsqlTest : NorthwindGroupByQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindGroupByQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    #region GroupByProperty

    public override async Task GroupBy_Property_Select_Average(bool async)
    {
        await base.GroupBy_Property_Select_Average(async);

        AssertSql(
            """
SELECT avg(o."OrderID"::double precision)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");

        // Validating that we don't generate warning when translating GroupBy. See Issue#11157
        Assert.DoesNotContain(
            "The LINQ expression 'GroupBy([o].CustomerID, [o])' could not be translated and will be evaluated locally.",
            Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
    }

    public override async Task GroupBy_Property_Select_Average_with_group_enumerable_projected(bool async)
    {
        await base.GroupBy_Property_Select_Average_with_group_enumerable_projected(async);

        AssertSql();
    }

    public override async Task GroupBy_Property_Select_Count(bool async)
    {
        await base.GroupBy_Property_Select_Count(async);

        AssertSql(
            """
SELECT count(*)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_LongCount(bool async)
    {
        await base.GroupBy_Property_Select_LongCount(async);

        AssertSql(
            """
SELECT count(*)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Count_with_nulls(bool async)
    {
        await base.GroupBy_Property_Select_Count_with_nulls(async);

        AssertSql(
            """
SELECT c."City", count(*)::int AS "Faxes"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    public override async Task GroupBy_Property_Select_LongCount_with_nulls(bool async)
    {
        await base.GroupBy_Property_Select_LongCount_with_nulls(async);

        AssertSql(
            """
SELECT c."City", count(*) AS "Faxes"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    public override async Task GroupBy_Property_Select_Max(bool async)
    {
        await base.GroupBy_Property_Select_Max(async);

        AssertSql(
            """
SELECT max(o."OrderID")
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Min(bool async)
    {
        await base.GroupBy_Property_Select_Min(async);

        AssertSql(
            """
SELECT min(o."OrderID")
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Sum(bool async)
    {
        await base.GroupBy_Property_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Property_Select_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Key_Average(bool async)
    {
        await base.GroupBy_Property_Select_Key_Average(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", avg(o."OrderID"::double precision) AS "Average"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Key_Count(bool async)
    {
        await base.GroupBy_Property_Select_Key_Count(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", count(*)::int AS "Count"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Key_LongCount(bool async)
    {
        await base.GroupBy_Property_Select_Key_LongCount(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", count(*) AS "LongCount"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Key_Max(bool async)
    {
        await base.GroupBy_Property_Select_Key_Max(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", max(o."OrderID") AS "Max"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Key_Min(bool async)
    {
        await base.GroupBy_Property_Select_Key_Min(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", min(o."OrderID") AS "Min"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Key_Sum(bool async)
    {
        await base.GroupBy_Property_Select_Key_Sum(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", COALESCE(sum(o."OrderID"), 0)::int AS "Sum"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Key_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Property_Select_Key_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_Property_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", o."CustomerID" AS "Key", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_key_multiple_times_and_aggregate(bool async)
    {
        await base.GroupBy_Property_Select_key_multiple_times_and_aggregate(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key1", COALESCE(sum(o."OrderID"), 0)::int AS "Sum"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Key_with_constant(bool async)
    {
        await base.GroupBy_Property_Select_Key_with_constant(async);

        AssertSql(
            """
SELECT o0."Name", o0."CustomerID" AS "Value", count(*)::int AS "Count"
FROM (
    SELECT o."CustomerID", 'CustomerID' AS "Name"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Name", o0."CustomerID"
""");
    }

    public override async Task GroupBy_aggregate_projecting_conditional_expression(bool async)
    {
        await base.GroupBy_aggregate_projecting_conditional_expression(async);

        AssertSql(
            """
SELECT o."OrderDate" AS "Key", CASE
    WHEN count(*)::int = 0 THEN 1
    ELSE COALESCE(sum(CASE
        WHEN o."OrderID" % 2 = 0 THEN 1
        ELSE 0
    END), 0)::int / count(*)::int
END AS "SomeValue"
FROM "Orders" AS o
GROUP BY o."OrderDate"
""");
    }

    public override async Task GroupBy_aggregate_projecting_conditional_expression_based_on_group_key(bool async)
    {
        await base.GroupBy_aggregate_projecting_conditional_expression_based_on_group_key(async);

        AssertSql(
            """
SELECT CASE
    WHEN o."OrderDate" IS NULL THEN 'is null'
    ELSE 'is not null'
END AS "Key", COALESCE(sum(o."OrderID"), 0)::int AS "Sum"
FROM "Orders" AS o
GROUP BY o."OrderDate"
""");
    }

    #endregion GroupByProperty

    #region GroupByAnonymousAggregate

    public override async Task GroupBy_anonymous_Select_Average(bool async)
    {
        await base.GroupBy_anonymous_Select_Average(async);

        AssertSql(
            """
SELECT avg(o."OrderID"::double precision)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_anonymous_Select_Count(bool async)
    {
        await base.GroupBy_anonymous_Select_Count(async);

        AssertSql(
            """
SELECT count(*)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_anonymous_Select_LongCount(bool async)
    {
        await base.GroupBy_anonymous_Select_LongCount(async);

        AssertSql(
            """
SELECT count(*)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_anonymous_Select_Max(bool async)
    {
        await base.GroupBy_anonymous_Select_Max(async);

        AssertSql(
            """
SELECT max(o."OrderID")
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_anonymous_Select_Min(bool async)
    {
        await base.GroupBy_anonymous_Select_Min(async);

        AssertSql(
            """
SELECT min(o."OrderID")
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_anonymous_Select_Sum(bool async)
    {
        await base.GroupBy_anonymous_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_anonymous_Select_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_anonymous_Select_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_anonymous_with_alias_Select_Key_Sum(bool async)
    {
        await base.GroupBy_anonymous_with_alias_Select_Key_Sum(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", COALESCE(sum(o."OrderID"), 0)::int AS "Sum"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Composite_Select_Average(bool async)
    {
        await base.GroupBy_Composite_Select_Average(async);

        AssertSql(
            """
SELECT avg(o."OrderID"::double precision)
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Count(bool async)
    {
        await base.GroupBy_Composite_Select_Count(async);

        AssertSql(
            """
SELECT count(*)::int
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_LongCount(bool async)
    {
        await base.GroupBy_Composite_Select_LongCount(async);

        AssertSql(
            """
SELECT count(*)
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Max(bool async)
    {
        await base.GroupBy_Composite_Select_Max(async);

        AssertSql(
            """
SELECT max(o."OrderID")
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Min(bool async)
    {
        await base.GroupBy_Composite_Select_Min(async);

        AssertSql(
            """
SELECT min(o."OrderID")
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Sum(bool async)
    {
        await base.GroupBy_Composite_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Average(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Average(async);

        AssertSql(
            """
SELECT o."CustomerID", o."EmployeeID", avg(o."OrderID"::double precision) AS "Average"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Count(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Count(async);

        AssertSql(
            """
SELECT o."CustomerID", o."EmployeeID", count(*)::int AS "Count"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Key_LongCount(bool async)
    {
        await base.GroupBy_Composite_Select_Key_LongCount(async);

        AssertSql(
            """
SELECT o."CustomerID", o."EmployeeID", count(*) AS "LongCount"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Max(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Max(async);

        AssertSql(
            """
SELECT o."CustomerID", o."EmployeeID", max(o."OrderID") AS "Max"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Min(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Min(async);

        AssertSql(
            """
SELECT o."CustomerID", o."EmployeeID", min(o."OrderID") AS "Min"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Sum(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Sum(async);

        AssertSql(
            """
SELECT o."CustomerID", o."EmployeeID", COALESCE(sum(o."OrderID"), 0)::int AS "Sum"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT o."CustomerID", o."EmployeeID", COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", o."CustomerID", o."EmployeeID", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", o."CustomerID", o."EmployeeID", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Dto_as_key_Select_Sum(bool async)
    {
        await base.GroupBy_Dto_as_key_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", o."CustomerID", o."EmployeeID"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Dto_as_element_selector_Select_Sum(bool async)
    {
        await base.GroupBy_Dto_as_element_selector_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."EmployeeID"::bigint), 0.0)::bigint AS "Sum", o."CustomerID" AS "Key"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", o."CustomerID" AS "CustomerId", o."EmployeeID" AS "EmployeeId", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(bool async)
    {
        await base.GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", o."CustomerID", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID", o."EmployeeID"
""");
    }

    public override async Task GroupBy_Constant_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_Constant_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum", min(o0."OrderID") AS "Min", o0."Key", max(o0."OrderID") AS "Max", avg(o0."OrderID"::double precision) AS "Avg"
FROM (
    SELECT o."OrderID", 2 AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_Constant_with_element_selector_Select_Sum(bool async)
    {
        await base.GroupBy_Constant_with_element_selector_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum"
FROM (
    SELECT o."OrderID", 2 AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_Constant_with_element_selector_Select_Sum2(bool async)
    {
        await base.GroupBy_Constant_with_element_selector_Select_Sum2(async);

        AssertSql(
            """
SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum"
FROM (
    SELECT o."OrderID", 2 AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_Constant_with_element_selector_Select_Sum3(bool async)
    {
        await base.GroupBy_Constant_with_element_selector_Select_Sum3(async);

        AssertSql(
            """
SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum"
FROM (
    SELECT o."OrderID", 2 AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum", min(o0."OrderID") AS "Min", o0."Key" AS "Random", max(o0."OrderID") AS "Max", avg(o0."OrderID"::double precision) AS "Avg"
FROM (
    SELECT o."OrderID", 2 AS "Key"
    FROM "Orders" AS o
    WHERE o."OrderID" > 10500
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum", o0."Key"
FROM (
    SELECT o."OrderID", 2 AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_constant_with_where_on_grouping_with_aggregate_operators(bool async)
    {
        await base.GroupBy_constant_with_where_on_grouping_with_aggregate_operators(async);

        AssertSql(
            """
SELECT min(o0."OrderDate") FILTER (WHERE 1 = o0."Key") AS "Min", max(o0."OrderDate") FILTER (WHERE 1 = o0."Key") AS "Max", COALESCE(sum(o0."OrderID") FILTER (WHERE 1 = o0."Key"), 0)::int AS "Sum", avg(o0."OrderID"::double precision) FILTER (WHERE 1 = o0."Key") AS "Average"
FROM (
    SELECT o."OrderID", o."OrderDate", 1 AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
ORDER BY o0."Key" NULLS FIRST
""");
    }

    public override async Task GroupBy_param_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_param_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
@a='2'

SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum", min(o0."OrderID") AS "Min", o0."Key", max(o0."OrderID") AS "Max", avg(o0."OrderID"::double precision) AS "Avg"
FROM (
    SELECT o."OrderID", @a AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_param_with_element_selector_Select_Sum(bool async)
    {
        await base.GroupBy_param_with_element_selector_Select_Sum(async);

        AssertSql(
            """
@a='2'

SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum"
FROM (
    SELECT o."OrderID", @a AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_param_with_element_selector_Select_Sum2(bool async)
    {
        await base.GroupBy_param_with_element_selector_Select_Sum2(async);

        AssertSql(
            """
@a='2'

SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum"
FROM (
    SELECT o."OrderID", @a AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_param_with_element_selector_Select_Sum3(bool async)
    {
        await base.GroupBy_param_with_element_selector_Select_Sum3(async);

        AssertSql(
            """
@a='2'

SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum"
FROM (
    SELECT o."OrderID", @a AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg(bool async)
    {
        await base.GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg(async);

        AssertSql(
            """
@a='2'

SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum", o0."Key"
FROM (
    SELECT o."OrderID", @a AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_anonymous_key_type_mismatch_with_aggregate(bool async)
    {
        await base.GroupBy_anonymous_key_type_mismatch_with_aggregate(async);

        AssertSql(
            """
SELECT count(*)::int AS "I0", o0."I0" AS "I1"
FROM (
    SELECT date_part('year', o."OrderDate")::int AS "I0"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."I0"
ORDER BY o0."I0" NULLS FIRST
""");
    }

    #endregion GroupByAnonymousAggregate

    #region GroupByWithElementSelectorAggregate

    public override async Task GroupBy_Property_scalar_element_selector_Average(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Average(async);

        AssertSql(
            """
SELECT avg(o."OrderID"::double precision)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_Count(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Count(async);

        AssertSql(
            """
SELECT count(*)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_LongCount(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_LongCount(async);

        AssertSql(
            """
SELECT count(*)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_Max(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Max(async);

        AssertSql(
            """
SELECT max(o."OrderID")
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_Min(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Min(async);

        AssertSql(
            """
SELECT min(o."OrderID")
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_Sum(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Sum(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Average(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Average(async);

        AssertSql(
            """
SELECT avg(o."OrderID"::double precision)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Count(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Count(async);

        AssertSql(
            """
SELECT count(*)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_LongCount(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_LongCount(async);

        AssertSql(
            """
SELECT count(*)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Max(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Max(async);

        AssertSql(
            """
SELECT max(o."OrderID")
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Min(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Min(async);

        AssertSql(
            """
SELECT min(o."OrderID")
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Sum(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Sum(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(bool async)
    {
        await base.GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."EmployeeID") AS "Min", max(o."EmployeeID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_element_selector_complex_aggregate(bool async)
    {
        await base.GroupBy_element_selector_complex_aggregate(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID" + 1), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_element_selector_complex_aggregate2(bool async)
    {
        await base.GroupBy_element_selector_complex_aggregate2(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID" + 1), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_element_selector_complex_aggregate3(bool async)
    {
        await base.GroupBy_element_selector_complex_aggregate3(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID" + 1), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_element_selector_complex_aggregate4(bool async)
    {
        await base.GroupBy_element_selector_complex_aggregate4(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID" + 1), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task Element_selector_with_case_block_repeated_inside_another_case_block_in_projection(bool async)
    {
        await base.Element_selector_with_case_block_repeated_inside_another_case_block_in_projection(async);

        AssertSql(
            """
SELECT o."OrderID", COALESCE(sum(CASE
    WHEN o."CustomerID" = 'ALFKI' THEN CASE
        WHEN o."OrderID" > 1000 THEN o."OrderID"
        ELSE -o."OrderID"
    END
    ELSE -CASE
        WHEN o."OrderID" > 1000 THEN o."OrderID"
        ELSE -o."OrderID"
    END
END), 0)::int AS "Aggregate"
FROM "Orders" AS o
GROUP BY o."OrderID"
""");
    }

    public override async Task GroupBy_conditional_properties(bool async)
    {
        await base.GroupBy_conditional_properties(async);

        AssertSql(
            """
SELECT o0."OrderMonth", o0."CustomerID" AS "Customer", count(*)::int AS "Count"
FROM (
    SELECT o."CustomerID", NULL AS "OrderMonth"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."OrderMonth", o0."CustomerID"
""");
    }

    #endregion GroupByWithElementSelectorAggregate

    #region GroupByAfterComposition

    public override async Task GroupBy_empty_key_Aggregate(bool async)
    {
        await base.GroupBy_empty_key_Aggregate(async);

        AssertSql(
            """
SELECT COALESCE(sum(o0."OrderID"), 0)::int
FROM (
    SELECT o."OrderID", 1 AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_empty_key_Aggregate_Key(bool async)
    {
        await base.GroupBy_empty_key_Aggregate_Key(async);

        AssertSql(
            """
SELECT COALESCE(sum(o0."OrderID"), 0)::int AS "Sum"
FROM (
    SELECT o."OrderID", 1 AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task OrderBy_GroupBy_Aggregate(bool async)
    {
        await base.OrderBy_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task OrderBy_Skip_GroupBy_Aggregate(bool async)
    {
        await base.OrderBy_Skip_GroupBy_Aggregate(async);

        AssertSql(
            """
@p='80'

SELECT avg(o0."OrderID"::double precision)
FROM (
    SELECT o."OrderID", o."CustomerID"
    FROM "Orders" AS o
    ORDER BY o."OrderID" NULLS FIRST
    OFFSET @p
) AS o0
GROUP BY o0."CustomerID"
""");
    }

    public override async Task OrderBy_Take_GroupBy_Aggregate(bool async)
    {
        await base.OrderBy_Take_GroupBy_Aggregate(async);

        AssertSql(
            """
@p='500'

SELECT min(o0."OrderID")
FROM (
    SELECT o."OrderID", o."CustomerID"
    FROM "Orders" AS o
    ORDER BY o."OrderID" NULLS FIRST
    LIMIT @p
) AS o0
GROUP BY o0."CustomerID"
""");
    }

    public override async Task OrderBy_Skip_Take_GroupBy_Aggregate(bool async)
    {
        await base.OrderBy_Skip_Take_GroupBy_Aggregate(async);

        AssertSql(
            """
@p0='500'
@p='80'

SELECT max(o0."OrderID")
FROM (
    SELECT o."OrderID", o."CustomerID"
    FROM "Orders" AS o
    ORDER BY o."OrderID" NULLS FIRST
    LIMIT @p0 OFFSET @p
) AS o0
GROUP BY o0."CustomerID"
""");
    }

    public override async Task Distinct_GroupBy_Aggregate(bool async)
    {
        await base.Distinct_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT o0."CustomerID" AS "Key", count(*)::int AS c
FROM (
    SELECT DISTINCT o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."CustomerID"
""");
    }

    public override async Task Anonymous_projection_Distinct_GroupBy_Aggregate(bool async)
    {
        await base.Anonymous_projection_Distinct_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT o0."EmployeeID" AS "Key", count(*)::int AS c
FROM (
    SELECT DISTINCT o."OrderID", o."EmployeeID"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."EmployeeID"
""");
    }

    public override async Task SelectMany_GroupBy_Aggregate(bool async)
    {
        await base.SelectMany_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT o."EmployeeID" AS "Key", count(*)::int AS c
FROM "Customers" AS c
INNER JOIN "Orders" AS o ON c."CustomerID" = o."CustomerID"
GROUP BY o."EmployeeID"
""");
    }

    public override async Task Join_GroupBy_Aggregate(bool async)
    {
        await base.Join_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT c."CustomerID" AS "Key", avg(o."OrderID"::double precision) AS "Count"
FROM "Orders" AS o
INNER JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
GROUP BY c."CustomerID"
""");
    }

    public override async Task GroupBy_required_navigation_member_Aggregate(bool async)
    {
        await base.GroupBy_required_navigation_member_Aggregate(async);

        AssertSql(
            """
SELECT o0."CustomerID" AS "CustomerId", count(*)::int AS "Count"
FROM "Order Details" AS o
INNER JOIN "Orders" AS o0 ON o."OrderID" = o0."OrderID"
GROUP BY o0."CustomerID"
""");
    }

    public override async Task Join_complex_GroupBy_Aggregate(bool async)
    {
        await base.Join_complex_GroupBy_Aggregate(async);

        AssertSql(
            """
@p='100'
@p1='50'
@p0='10'

SELECT c0."CustomerID" AS "Key", avg(o0."OrderID"::double precision) AS "Count"
FROM (
    SELECT o."OrderID", o."CustomerID"
    FROM "Orders" AS o
    WHERE o."OrderID" < 10400
    ORDER BY o."OrderDate" NULLS FIRST
    LIMIT @p
) AS o0
INNER JOIN (
    SELECT c."CustomerID"
    FROM "Customers" AS c
    WHERE c."CustomerID" NOT IN ('DRACD', 'FOLKO')
    ORDER BY c."City" NULLS FIRST
    LIMIT @p1 OFFSET @p0
) AS c0 ON o0."CustomerID" = c0."CustomerID"
GROUP BY c0."CustomerID"
""");
    }

    public override async Task GroupJoin_GroupBy_Aggregate(bool async)
    {
        await base.GroupJoin_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", avg(o."OrderID"::double precision) AS "Average"
FROM "Customers" AS c
LEFT JOIN "Orders" AS o ON c."CustomerID" = o."CustomerID"
WHERE o."OrderID" IS NOT NULL
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupJoin_GroupBy_Aggregate_2(bool async)
    {
        await base.GroupJoin_GroupBy_Aggregate_2(async);

        AssertSql(
            """
SELECT c."CustomerID" AS "Key", max(c."City") AS "Max"
FROM "Customers" AS c
LEFT JOIN "Orders" AS o ON c."CustomerID" = o."CustomerID"
GROUP BY c."CustomerID"
""");
    }

    public override async Task GroupJoin_GroupBy_Aggregate_3(bool async)
    {
        await base.GroupJoin_GroupBy_Aggregate_3(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", avg(o."OrderID"::double precision) AS "Average"
FROM "Orders" AS o
LEFT JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupJoin_GroupBy_Aggregate_4(bool async)
    {
        await base.GroupJoin_GroupBy_Aggregate_4(async);

        AssertSql(
            """
SELECT c."CustomerID" AS "Value", max(c."City") AS "Max"
FROM "Customers" AS c
LEFT JOIN "Orders" AS o ON c."CustomerID" = o."CustomerID"
GROUP BY c."CustomerID"
""");
    }

    public override async Task GroupJoin_GroupBy_Aggregate_5(bool async)
    {
        await base.GroupJoin_GroupBy_Aggregate_5(async);

        AssertSql(
            """
SELECT o."OrderID" AS "Value", avg(o."OrderID"::double precision) AS "Average"
FROM "Orders" AS o
LEFT JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
GROUP BY o."OrderID"
""");
    }

    public override async Task GroupBy_optional_navigation_member_Aggregate(bool async)
    {
        await base.GroupBy_optional_navigation_member_Aggregate(async);

        AssertSql(
            """
SELECT c."Country", count(*)::int AS "Count"
FROM "Orders" AS o
LEFT JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
GROUP BY c."Country"
""");
    }

    public override async Task GroupJoin_complex_GroupBy_Aggregate(bool async)
    {
        await base.GroupJoin_complex_GroupBy_Aggregate(async);

        AssertSql(
            """
@p0='50'
@p='10'
@p1='100'

SELECT o0."CustomerID" AS "Key", avg(o0."OrderID"::double precision) AS "Count"
FROM (
    SELECT c."CustomerID"
    FROM "Customers" AS c
    WHERE c."CustomerID" NOT IN ('DRACD', 'FOLKO')
    ORDER BY c."City" NULLS FIRST
    LIMIT @p0 OFFSET @p
) AS c0
INNER JOIN (
    SELECT o."OrderID", o."CustomerID"
    FROM "Orders" AS o
    WHERE o."OrderID" < 10400
    ORDER BY o."OrderDate" NULLS FIRST
    LIMIT @p1
) AS o0 ON c0."CustomerID" = o0."CustomerID"
WHERE o0."OrderID" > 10300
GROUP BY o0."CustomerID"
""");
    }

    public override async Task Self_join_GroupBy_Aggregate(bool async)
    {
        await base.Self_join_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", avg(o0."OrderID"::double precision) AS "Count"
FROM "Orders" AS o
INNER JOIN "Orders" AS o0 ON o."OrderID" = o0."OrderID"
WHERE o."OrderID" < 10400
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_multi_navigation_members_Aggregate(bool async)
    {
        await base.GroupBy_multi_navigation_members_Aggregate(async);

        AssertSql(
            """
SELECT o0."CustomerID", p."ProductName", count(*)::int AS "Count"
FROM "Order Details" AS o
INNER JOIN "Orders" AS o0 ON o."OrderID" = o0."OrderID"
INNER JOIN "Products" AS p ON o."ProductID" = p."ProductID"
GROUP BY o0."CustomerID", p."ProductName"
""");
    }

    public override async Task Union_simple_groupby(bool async)
    {
        await base.Union_simple_groupby(async);

        AssertSql(
            """
SELECT u."City" AS "Key", count(*)::int AS "Total"
FROM (
    SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
    FROM "Customers" AS c
    WHERE c."ContactTitle" = 'Owner'
    UNION
    SELECT c0."CustomerID", c0."Address", c0."City", c0."CompanyName", c0."ContactName", c0."ContactTitle", c0."Country", c0."Fax", c0."Phone", c0."PostalCode", c0."Region"
    FROM "Customers" AS c0
    WHERE c0."City" = 'México D.F.'
) AS u
GROUP BY u."City"
""");
    }

    public override async Task Select_anonymous_GroupBy_Aggregate(bool async)
    {
        await base.Select_anonymous_GroupBy_Aggregate(async);

        AssertSql(
            """
SELECT min(o."OrderDate") AS "Min", max(o."OrderDate") AS "Max", COALESCE(sum(o."OrderID"), 0)::int AS "Sum", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
WHERE o."OrderID" < 10300
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_principal_key_property_optimization(bool async)
    {
        await base.GroupBy_principal_key_property_optimization(async);

        AssertSql(
            """
SELECT c."CustomerID" AS "Key", count(*)::int AS "Count"
FROM "Orders" AS o
LEFT JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
GROUP BY c."CustomerID"
""");
    }

    public override async Task GroupBy_after_anonymous_projection_and_distinct_followed_by_another_anonymous_projection(bool async)
    {
        await base.GroupBy_after_anonymous_projection_and_distinct_followed_by_another_anonymous_projection(async);

        AssertSql(
            """
SELECT o0."CustomerID" AS "Key", count(*)::int AS "Count"
FROM (
    SELECT DISTINCT o."CustomerID", o."OrderID"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."CustomerID"
""");
    }

    public override async Task GroupBy_complex_key_aggregate(bool async)
    {
        await base.GroupBy_complex_key_aggregate(async);

        AssertSql(
            """
SELECT s."Key", count(*)::int AS "Count"
FROM (
    SELECT substring(c."CustomerID", 1, 1) AS "Key"
    FROM "Orders" AS o
    LEFT JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
) AS s
GROUP BY s."Key"
""");
    }

    public override async Task GroupBy_complex_key_aggregate_2(bool async)
    {
        await base.GroupBy_complex_key_aggregate_2(async);

        AssertSql(
            """
SELECT o0."Key" AS "Month", COALESCE(sum(o0."OrderID"), 0)::int AS "Total", (
    SELECT COALESCE(sum(o1."OrderID"), 0)::int
    FROM "Orders" AS o1
    WHERE date_part('month', o1."OrderDate")::int = o0."Key" OR (o1."OrderDate" IS NULL AND o0."Key" IS NULL)) AS "Payment"
FROM (
    SELECT o."OrderID", date_part('month', o."OrderDate")::int AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task Select_collection_of_scalar_before_GroupBy_aggregate(bool async)
    {
        await base.Select_collection_of_scalar_before_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT c."City" AS "Key", count(*)::int AS "Count"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    #endregion GroupByAfterComposition

    #region GroupByAggregateComposition

    public override async Task GroupBy_OrderBy_key(bool async)
    {
        await base.GroupBy_OrderBy_key(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", count(*)::int AS c
FROM "Orders" AS o
GROUP BY o."CustomerID"
ORDER BY o."CustomerID" NULLS FIRST
""");
    }

    public override async Task GroupBy_OrderBy_count(bool async)
    {
        await base.GroupBy_OrderBy_count(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", count(*)::int AS "Count"
FROM "Orders" AS o
GROUP BY o."CustomerID"
ORDER BY count(*)::int NULLS FIRST, o."CustomerID" NULLS FIRST
""");
    }

    public override async Task GroupBy_OrderBy_count_Select_sum(bool async)
    {
        await base.GroupBy_OrderBy_count_Select_sum(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", COALESCE(sum(o."OrderID"), 0)::int AS "Sum"
FROM "Orders" AS o
GROUP BY o."CustomerID"
ORDER BY count(*)::int NULLS FIRST, o."CustomerID" NULLS FIRST
""");
    }

    public override async Task GroupBy_aggregate_Contains(bool async)
    {
        await base.GroupBy_aggregate_Contains(async);

        AssertSql(
            """
SELECT o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate"
FROM "Orders" AS o
WHERE EXISTS (
    SELECT 1
    FROM "Orders" AS o0
    GROUP BY o0."CustomerID"
    HAVING count(*)::int > 30 AND (o0."CustomerID" = o."CustomerID" OR (o0."CustomerID" IS NULL AND o."CustomerID" IS NULL)))
""");
    }

    public override async Task GroupBy_aggregate_Pushdown(bool async)
    {
        await base.GroupBy_aggregate_Pushdown(async);

        AssertSql(
            """
@p='20'
@p0='4'

SELECT o0."CustomerID"
FROM (
    SELECT o."CustomerID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 10
    ORDER BY o."CustomerID" NULLS FIRST
    LIMIT @p
) AS o0
ORDER BY o0."CustomerID" NULLS FIRST
OFFSET @p0
""");
    }

    public override async Task GroupBy_aggregate_using_grouping_key_Pushdown(bool async)
    {
        await base.GroupBy_aggregate_using_grouping_key_Pushdown(async);

        AssertSql(
            """
@p='20'
@p0='4'

SELECT o0."Key", o0."Max"
FROM (
    SELECT o."CustomerID" AS "Key", max(o."CustomerID") AS "Max"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 10
    ORDER BY o."CustomerID" NULLS FIRST
    LIMIT @p
) AS o0
ORDER BY o0."Key" NULLS FIRST
OFFSET @p0
""");
    }

    public override async Task GroupBy_aggregate_Pushdown_followed_by_projecting_Length(bool async)
    {
        await base.GroupBy_aggregate_Pushdown_followed_by_projecting_Length(async);

        AssertSql(
            """
@p='20'
@p0='4'

SELECT length(o0."CustomerID")::int
FROM (
    SELECT o."CustomerID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 10
    ORDER BY o."CustomerID" NULLS FIRST
    LIMIT @p
) AS o0
ORDER BY o0."CustomerID" NULLS FIRST
OFFSET @p0
""");
    }

    public override async Task GroupBy_aggregate_Pushdown_followed_by_projecting_constant(bool async)
    {
        await base.GroupBy_aggregate_Pushdown_followed_by_projecting_constant(async);

        AssertSql(
            """
@p='20'
@p0='4'

SELECT 5
FROM (
    SELECT o."CustomerID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 10
    ORDER BY o."CustomerID" NULLS FIRST
    LIMIT @p
) AS o0
ORDER BY o0."CustomerID" NULLS FIRST
OFFSET @p0
""");
    }

    public override async Task GroupBy_filter_key(bool async)
    {
        await base.GroupBy_filter_key(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", count(*)::int AS c
FROM "Orders" AS o
GROUP BY o."CustomerID"
HAVING o."CustomerID" = 'ALFKI'
""");
    }

    public override async Task GroupBy_filter_count(bool async)
    {
        await base.GroupBy_filter_count(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", count(*)::int AS "Count"
FROM "Orders" AS o
GROUP BY o."CustomerID"
HAVING count(*)::int > 4
""");
    }

    public override async Task GroupBy_count_filter(bool async)
    {
        await base.GroupBy_count_filter(async);

        AssertSql(
            """
SELECT o0."Key" AS "Name", count(*)::int AS "Count"
FROM (
    SELECT 'Order' AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
HAVING count(*)::int > 0
""");
    }

    public override async Task GroupBy_filter_count_OrderBy_count_Select_sum(bool async)
    {
        await base.GroupBy_filter_count_OrderBy_count_Select_sum(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", count(*)::int AS "Count", COALESCE(sum(o."OrderID"), 0)::int AS "Sum"
FROM "Orders" AS o
GROUP BY o."CustomerID"
HAVING count(*)::int > 4
ORDER BY count(*)::int NULLS FIRST, o."CustomerID" NULLS FIRST
""");
    }

    public override async Task GroupBy_Aggregate_Join(bool async)
    {
        await base.GroupBy_Aggregate_Join(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", o1."OrderID", o1."CustomerID", o1."EmployeeID", o1."OrderDate"
FROM (
    SELECT o."CustomerID", max(o."OrderID") AS "LastOrderID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 5
) AS o0
INNER JOIN "Customers" AS c ON o0."CustomerID" = c."CustomerID"
INNER JOIN "Orders" AS o1 ON o0."LastOrderID" = o1."OrderID"
""");
    }

    public override async Task GroupBy_Aggregate_Join_converted_from_SelectMany(bool async)
    {
        await base.GroupBy_Aggregate_Join_converted_from_SelectMany(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
INNER JOIN (
    SELECT o."CustomerID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 5
) AS o0 ON c."CustomerID" = o0."CustomerID"
""");
    }

    public override async Task GroupBy_Aggregate_LeftJoin_converted_from_SelectMany(bool async)
    {
        await base.GroupBy_Aggregate_LeftJoin_converted_from_SelectMany(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
LEFT JOIN (
    SELECT o."CustomerID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 5
) AS o0 ON c."CustomerID" = o0."CustomerID"
""");
    }

    public override async Task Join_GroupBy_Aggregate_multijoins(bool async)
    {
        await base.Join_GroupBy_Aggregate_multijoins(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", o1."OrderID", o1."CustomerID", o1."EmployeeID", o1."OrderDate"
FROM "Customers" AS c
INNER JOIN (
    SELECT o."CustomerID", max(o."OrderID") AS "LastOrderID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 5
) AS o0 ON c."CustomerID" = o0."CustomerID"
INNER JOIN "Orders" AS o1 ON o0."LastOrderID" = o1."OrderID"
""");
    }

    public override async Task Join_GroupBy_Aggregate_single_join(bool async)
    {
        await base.Join_GroupBy_Aggregate_single_join(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", o0."LastOrderID"
FROM "Customers" AS c
INNER JOIN (
    SELECT o."CustomerID", max(o."OrderID") AS "LastOrderID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 5
) AS o0 ON c."CustomerID" = o0."CustomerID"
""");
    }

    public override async Task Join_GroupBy_Aggregate_with_another_join(bool async)
    {
        await base.Join_GroupBy_Aggregate_with_another_join(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", o0."LastOrderID", o1."OrderID"
FROM "Customers" AS c
INNER JOIN (
    SELECT o."CustomerID", max(o."OrderID") AS "LastOrderID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 5
) AS o0 ON c."CustomerID" = o0."CustomerID"
INNER JOIN "Orders" AS o1 ON c."CustomerID" = o1."CustomerID"
""");
    }

    public override async Task Join_GroupBy_Aggregate_distinct_single_join(bool async)
    {
        await base.Join_GroupBy_Aggregate_distinct_single_join(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", o1."LastOrderID"
FROM "Customers" AS c
INNER JOIN (
    SELECT DISTINCT o0."CustomerID", max(o0."OrderID") AS "LastOrderID"
    FROM (
        SELECT o."OrderID", o."CustomerID", date_part('year', o."OrderDate")::int AS "Year"
        FROM "Orders" AS o
    ) AS o0
    GROUP BY o0."CustomerID", o0."Year"
    HAVING count(*)::int > 5
) AS o1 ON c."CustomerID" = o1."CustomerID"
""");
    }

    public override async Task Join_GroupBy_Aggregate_with_left_join(bool async)
    {
        await base.Join_GroupBy_Aggregate_with_left_join(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", o0."LastOrderID"
FROM "Customers" AS c
LEFT JOIN (
    SELECT o."CustomerID", max(o."OrderID") AS "LastOrderID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 5
) AS o0 ON c."CustomerID" = o0."CustomerID"
WHERE c."CustomerID" LIKE 'A%'
""");
    }

    public override async Task Join_GroupBy_Aggregate_in_subquery(bool async)
    {
        await base.Join_GroupBy_Aggregate_in_subquery(async);

        AssertSql(
            """
SELECT o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate", s."CustomerID", s."Address", s."City", s."CompanyName", s."ContactName", s."ContactTitle", s."Country", s."Fax", s."Phone", s."PostalCode", s."Region"
FROM "Orders" AS o
INNER JOIN (
    SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
    FROM "Customers" AS c
    INNER JOIN (
        SELECT o0."CustomerID"
        FROM "Orders" AS o0
        GROUP BY o0."CustomerID"
        HAVING count(*)::int > 5
    ) AS o1 ON c."CustomerID" = o1."CustomerID"
) AS s ON o."CustomerID" = s."CustomerID"
WHERE o."OrderID" < 10400
""");
    }

    public override async Task Join_GroupBy_Aggregate_on_key(bool async)
    {
        await base.Join_GroupBy_Aggregate_on_key(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", o0."LastOrderID"
FROM "Customers" AS c
INNER JOIN (
    SELECT o."CustomerID" AS "Key", max(o."OrderID") AS "LastOrderID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 5
) AS o0 ON c."CustomerID" = o0."Key"
""");
    }

    public override async Task GroupBy_with_result_selector(bool async)
    {
        await base.GroupBy_with_result_selector(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", min(o."OrderID") AS "Min", max(o."OrderID") AS "Max", avg(o."OrderID"::double precision) AS "Avg"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Sum_constant(bool async)
    {
        await base.GroupBy_Sum_constant(async);

        AssertSql(
            """
SELECT COALESCE(sum(1), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Sum_constant_cast(bool async)
    {
        await base.GroupBy_Sum_constant_cast(async);

        AssertSql(
            """
SELECT COALESCE(sum(1), 0.0)::bigint
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task Distinct_GroupBy_OrderBy_key(bool async)
    {
        await base.Distinct_GroupBy_OrderBy_key(async);

        AssertSql(
            """
SELECT o0."CustomerID" AS "Key", count(*)::int AS c
FROM (
    SELECT DISTINCT o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."CustomerID"
ORDER BY o0."CustomerID" NULLS FIRST
""");
    }

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/28410")]
    public override async Task Select_nested_collection_with_groupby(bool async)
    {
        await base.Select_nested_collection_with_groupby(async);

        AssertSql(
            """
SELECT EXISTS (
    SELECT 1
    FROM "Orders" AS o
    WHERE c."CustomerID" = o."CustomerID"), c."CustomerID", t."OrderID"
FROM "Customers" AS c
LEFT JOIN LATERAL (
    SELECT o0."OrderID"
    FROM "Orders" AS o0
    WHERE c."CustomerID" = o0."CustomerID"
    GROUP BY o0."OrderID"
) AS t ON TRUE
WHERE c."CustomerID" LIKE 'F%'
ORDER BY c."CustomerID" NULLS FIRST
""");
    }

    public override async Task Select_uncorrelated_collection_with_groupby_works(bool async)
    {
        await base.Select_uncorrelated_collection_with_groupby_works(async);

        AssertSql(
            """
SELECT c."CustomerID", o0."OrderID"
FROM "Customers" AS c
LEFT JOIN LATERAL (
    SELECT o."OrderID"
    FROM "Orders" AS o
    GROUP BY o."OrderID"
) AS o0 ON TRUE
WHERE c."CustomerID" LIKE 'A%'
ORDER BY c."CustomerID" NULLS FIRST
""");
    }

    public override async Task Select_uncorrelated_collection_with_groupby_multiple_collections_work(bool async)
    {
        await base.Select_uncorrelated_collection_with_groupby_multiple_collections_work(async);

        AssertSql(
            """
SELECT o."OrderID", p1."ProductID", p2.c, p2."ProductID"
FROM "Orders" AS o
LEFT JOIN LATERAL (
    SELECT p."ProductID"
    FROM "Products" AS p
    GROUP BY p."ProductID"
) AS p1 ON TRUE
LEFT JOIN LATERAL (
    SELECT count(*)::int AS c, p0."ProductID"
    FROM "Products" AS p0
    GROUP BY p0."ProductID"
) AS p2 ON TRUE
WHERE o."CustomerID" LIKE 'A%'
ORDER BY o."OrderID" NULLS FIRST, p1."ProductID" NULLS FIRST
""");
    }

    public override async Task Select_GroupBy_All(bool async)
    {
        await base.Select_GroupBy_All(async);

        AssertSql(
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING o."CustomerID" <> 'ALFKI' OR o."CustomerID" IS NULL)
""");
    }

    #endregion GroupByAggregateComposition

    #region GroupByAggregateChainComposition

    public override async Task GroupBy_Where_Average(bool async)
    {
        await base.GroupBy_Where_Average(async);

        AssertSql(
            """
SELECT avg(o."OrderID"::double precision) FILTER (WHERE o."OrderID" < 10300)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Where_Count(bool async)
    {
        await base.GroupBy_Where_Count(async);

        AssertSql(
            """
SELECT count(*) FILTER (WHERE o."OrderID" < 10300)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Where_LongCount(bool async)
    {
        await base.GroupBy_Where_LongCount(async);

        AssertSql(
            """
SELECT count(*) FILTER (WHERE o."OrderID" < 10300)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Where_Max(bool async)
    {
        await base.GroupBy_Where_Max(async);

        AssertSql(
            """
SELECT max(o."OrderID") FILTER (WHERE o."OrderID" < 10300)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Where_Min(bool async)
    {
        await base.GroupBy_Where_Min(async);

        AssertSql(
            """
SELECT min(o."OrderID") FILTER (WHERE o."OrderID" < 10300)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Where_Sum(bool async)
    {
        await base.GroupBy_Where_Sum(async);

        AssertSql(
            """
SELECT COALESCE(sum(o."OrderID") FILTER (WHERE o."OrderID" < 10300), 0)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Where_Count_with_predicate(bool async)
    {
        await base.GroupBy_Where_Count_with_predicate(async);

        AssertSql(
            """
SELECT count(*) FILTER (WHERE o."OrderID" < 10300 AND o."OrderDate" IS NOT NULL AND date_part('year', o."OrderDate")::int = 1997)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Where_Where_Count(bool async)
    {
        await base.GroupBy_Where_Where_Count(async);

        AssertSql(
            """
SELECT count(*) FILTER (WHERE o."OrderID" < 10300 AND o."OrderDate" IS NOT NULL AND date_part('year', o."OrderDate")::int = 1997)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Where_Select_Where_Count(bool async)
    {
        await base.GroupBy_Where_Select_Where_Count(async);

        AssertSql(
            """
SELECT count(*) FILTER (WHERE o."OrderID" < 10300 AND o."OrderDate" IS NOT NULL AND date_part('year', o."OrderDate")::int = 1997)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Where_Select_Where_Select_Min(bool async)
    {
        await base.GroupBy_Where_Select_Where_Select_Min(async);

        AssertSql(
            """
SELECT min(o."OrderID") FILTER (WHERE o."OrderID" < 10300 AND o."OrderDate" IS NOT NULL AND date_part('year', o."OrderDate")::int = 1997)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_multiple_Count_with_predicate(bool async)
    {
        await base.GroupBy_multiple_Count_with_predicate(async);

        AssertSql(
            """
SELECT o."CustomerID", count(*)::int AS "All", count(*) FILTER (WHERE o."OrderID" < 11000)::int AS "TenK", count(*) FILTER (WHERE o."OrderID" < 12000)::int AS "EleventK"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_multiple_Sum_with_conditional_projection(bool async)
    {
        await base.GroupBy_multiple_Sum_with_conditional_projection(async);

        AssertSql(
            """
SELECT o."CustomerID", COALESCE(sum(CASE
    WHEN o."OrderID" < 11000 THEN o."OrderID"
    ELSE 0
END), 0)::int AS "TenK", COALESCE(sum(CASE
    WHEN o."OrderID" >= 11000 THEN o."OrderID"
    ELSE 0
END), 0)::int AS "EleventK"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_multiple_Sum_with_Select_conditional_projection(bool async)
    {
        await base.GroupBy_multiple_Sum_with_Select_conditional_projection(async);

        AssertSql(
            """
SELECT o."CustomerID", COALESCE(sum(CASE
    WHEN o."OrderID" < 11000 THEN o."OrderID"
    ELSE 0
END), 0)::int AS "TenK", COALESCE(sum(CASE
    WHEN o."OrderID" >= 11000 THEN o."OrderID"
    ELSE 0
END), 0)::int AS "EleventK"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Key_as_part_of_element_selector(bool async)
    {
        await base.GroupBy_Key_as_part_of_element_selector(async);

        AssertSql(
            """
SELECT o."OrderID" AS "Key", avg(o."OrderID"::double precision) AS "Avg", max(o."OrderDate") AS "Max"
FROM "Orders" AS o
GROUP BY o."OrderID"
""");
    }

    public override async Task GroupBy_composite_Key_as_part_of_element_selector(bool async)
    {
        await base.GroupBy_composite_Key_as_part_of_element_selector(async);

        AssertSql(
            """
SELECT o."OrderID", o."CustomerID", avg(o."OrderID"::double precision) AS "Avg", max(o."OrderDate") AS "Max"
FROM "Orders" AS o
GROUP BY o."OrderID", o."CustomerID"
""");
    }

    public override async Task GroupBy_with_aggregate_through_navigation_property(bool async)
    {
        await base.GroupBy_with_aggregate_through_navigation_property(async);

        AssertSql(
            """
SELECT (
    SELECT max(c."Region")
    FROM "Orders" AS o0
    LEFT JOIN "Customers" AS c ON o0."CustomerID" = c."CustomerID"
    WHERE o."EmployeeID" = o0."EmployeeID" OR (o."EmployeeID" IS NULL AND o0."EmployeeID" IS NULL)) AS max
FROM "Orders" AS o
GROUP BY o."EmployeeID"
""");
    }

    public override async Task GroupBy_with_aggregate_containing_complex_where(bool async)
    {
        await base.GroupBy_with_aggregate_containing_complex_where(async);

        AssertSql(
            """
SELECT o."EmployeeID" AS "Key", (
    SELECT max(o0."OrderID")
    FROM "Orders" AS o0
    WHERE o0."EmployeeID"::bigint = CAST(max(o."OrderID") * 6 AS bigint) OR (o0."EmployeeID" IS NULL AND max(o."OrderID") IS NULL)) AS "Max"
FROM "Orders" AS o
GROUP BY o."EmployeeID"
""");
    }

    #endregion GroupByWithoutAggregate

    #region GroupBySelectFirst

    public override async Task GroupBy_Shadow(bool async)
    {
        await base.GroupBy_Shadow(async);

        AssertSql(
            """
SELECT (
    SELECT e0."Title"
    FROM "Employees" AS e0
    WHERE e0."Title" = 'Sales Representative' AND e0."EmployeeID" = 1 AND (e."Title" = e0."Title" OR (e."Title" IS NULL AND e0."Title" IS NULL))
    LIMIT 1)
FROM "Employees" AS e
WHERE e."Title" = 'Sales Representative' AND e."EmployeeID" = 1
GROUP BY e."Title"
""");
    }

    public override async Task GroupBy_Shadow2(bool async)
    {
        await base.GroupBy_Shadow2(async);

        AssertSql(
            """
SELECT e3."EmployeeID", e3."City", e3."Country", e3."FirstName", e3."ReportsTo", e3."Title"
FROM (
    SELECT e."Title"
    FROM "Employees" AS e
    WHERE e."Title" = 'Sales Representative' AND e."EmployeeID" = 1
    GROUP BY e."Title"
) AS e1
LEFT JOIN (
    SELECT e2."EmployeeID", e2."City", e2."Country", e2."FirstName", e2."ReportsTo", e2."Title"
    FROM (
        SELECT e0."EmployeeID", e0."City", e0."Country", e0."FirstName", e0."ReportsTo", e0."Title", ROW_NUMBER() OVER(PARTITION BY e0."Title" ORDER BY e0."EmployeeID" NULLS FIRST) AS row
        FROM "Employees" AS e0
        WHERE e0."Title" = 'Sales Representative' AND e0."EmployeeID" = 1
    ) AS e2
    WHERE e2.row <= 1
) AS e3 ON e1."Title" = e3."Title"
""");
    }

    public override async Task GroupBy_Shadow3(bool async)
    {
        await base.GroupBy_Shadow3(async);

        AssertSql(
            """
SELECT (
    SELECT e0."Title"
    FROM "Employees" AS e0
    WHERE e0."EmployeeID" = 1 AND e."EmployeeID" = e0."EmployeeID"
    LIMIT 1)
FROM "Employees" AS e
WHERE e."EmployeeID" = 1
GROUP BY e."EmployeeID"
""");
    }

    public override async Task GroupBy_select_grouping_list(bool async)
    {
        await base.GroupBy_select_grouping_list(async);

        AssertSql(
            """
SELECT c1."City", c0."CustomerID", c0."Address", c0."City", c0."CompanyName", c0."ContactName", c0."ContactTitle", c0."Country", c0."Fax", c0."Phone", c0."PostalCode", c0."Region"
FROM (
    SELECT c."City"
    FROM "Customers" AS c
    GROUP BY c."City"
) AS c1
LEFT JOIN "Customers" AS c0 ON c1."City" = c0."City"
ORDER BY c1."City" NULLS FIRST
""");
    }

    public override async Task GroupBy_select_grouping_array(bool async)
    {
        await base.GroupBy_select_grouping_array(async);

        AssertSql(
            """
SELECT c1."City", c0."CustomerID", c0."Address", c0."City", c0."CompanyName", c0."ContactName", c0."ContactTitle", c0."Country", c0."Fax", c0."Phone", c0."PostalCode", c0."Region"
FROM (
    SELECT c."City"
    FROM "Customers" AS c
    GROUP BY c."City"
) AS c1
LEFT JOIN "Customers" AS c0 ON c1."City" = c0."City"
ORDER BY c1."City" NULLS FIRST
""");
    }

    public override async Task GroupBy_select_grouping_composed_list(bool async)
    {
        await base.GroupBy_select_grouping_composed_list(async);

        AssertSql(
            """
SELECT c1."City", c2."CustomerID", c2."Address", c2."City", c2."CompanyName", c2."ContactName", c2."ContactTitle", c2."Country", c2."Fax", c2."Phone", c2."PostalCode", c2."Region"
FROM (
    SELECT c."City"
    FROM "Customers" AS c
    GROUP BY c."City"
) AS c1
LEFT JOIN (
    SELECT c0."CustomerID", c0."Address", c0."City", c0."CompanyName", c0."ContactName", c0."ContactTitle", c0."Country", c0."Fax", c0."Phone", c0."PostalCode", c0."Region"
    FROM "Customers" AS c0
    WHERE c0."CustomerID" LIKE 'A%'
) AS c2 ON c1."City" = c2."City"
ORDER BY c1."City" NULLS FIRST
""");
    }

    public override async Task GroupBy_select_grouping_composed_list_2(bool async)
    {
        await base.GroupBy_select_grouping_composed_list_2(async);

        AssertSql(
            """
SELECT c1."City", c0."CustomerID", c0."Address", c0."City", c0."CompanyName", c0."ContactName", c0."ContactTitle", c0."Country", c0."Fax", c0."Phone", c0."PostalCode", c0."Region"
FROM (
    SELECT c."City"
    FROM "Customers" AS c
    GROUP BY c."City"
) AS c1
LEFT JOIN "Customers" AS c0 ON c1."City" = c0."City"
ORDER BY c1."City" NULLS FIRST, c0."CustomerID" NULLS FIRST
""");
    }

    public override async Task Select_GroupBy_SelectMany(bool async)
    {
        await base.Select_GroupBy_SelectMany(async);

        AssertSql();
    }

    #endregion GroupByEntityType

    #region ResultOperatorsAfterGroupBy

    public override async Task Count_after_GroupBy_aggregate(bool async)
    {
        await base.Count_after_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT count(*)::int
FROM (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o0
""");
    }

    public override async Task LongCount_after_GroupBy_aggregate(bool async)
    {
        await base.LongCount_after_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT count(*)
FROM (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o0
""");
    }

    public override async Task GroupBy_Select_Distinct_aggregate(bool async)
    {
        await base.GroupBy_Select_Distinct_aggregate(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", avg(DISTINCT o."OrderID"::double precision) AS "Average", count(DISTINCT o."EmployeeID")::int AS "Count", count(DISTINCT o."EmployeeID") AS "LongCount", max(o."OrderDate") AS "Max", min(o."OrderDate") AS "Min", COALESCE(sum(DISTINCT o."OrderID"), 0)::int AS "Sum"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_group_Distinct_Select_Distinct_aggregate(bool async)
    {
        await base.GroupBy_group_Distinct_Select_Distinct_aggregate(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", max(o."OrderDate") AS "Max"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_group_Where_Select_Distinct_aggregate(bool async)
    {
        await base.GroupBy_group_Where_Select_Distinct_aggregate(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", max(o."OrderDate") FILTER (WHERE o."OrderDate" IS NOT NULL) AS "Max"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task MinMax_after_GroupBy_aggregate(bool async)
    {
        await base.MinMax_after_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT min(o0.c)
FROM (
    SELECT COALESCE(sum(o."OrderID"), 0)::int AS c
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o0
""",
            //
            """
SELECT max(o0.c)
FROM (
    SELECT COALESCE(sum(o."OrderID"), 0)::int AS c
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o0
""");
    }

    public override async Task All_after_GroupBy_aggregate(bool async)
    {
        await base.All_after_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING FALSE)
""");
    }

    public override async Task All_after_GroupBy_aggregate2(bool async)
    {
        await base.All_after_GroupBy_aggregate2(async);

        AssertSql(
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING COALESCE(sum(o."OrderID"), 0)::int < 0)
""");
    }

    public override async Task Any_after_GroupBy_aggregate(bool async)
    {
        await base.Any_after_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT EXISTS (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID")
""");
    }

    public override async Task Count_after_GroupBy_without_aggregate(bool async)
    {
        await base.Count_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT count(*)::int
FROM (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o0
""");
    }

    public override async Task Count_with_predicate_after_GroupBy_without_aggregate(bool async)
    {
        await base.Count_with_predicate_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT count(*)::int
FROM (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 1
) AS o0
""");
    }

    public override async Task LongCount_after_GroupBy_without_aggregate(bool async)
    {
        await base.LongCount_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT count(*)
FROM (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o0
""");
    }

    public override async Task LongCount_with_predicate_after_GroupBy_without_aggregate(bool async)
    {
        await base.LongCount_with_predicate_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT count(*)
FROM (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 1
) AS o0
""");
    }

    public override async Task Any_after_GroupBy_without_aggregate(bool async)
    {
        await base.Any_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT EXISTS (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID")
""");
    }

    public override async Task Any_with_predicate_after_GroupBy_without_aggregate(bool async)
    {
        await base.Any_with_predicate_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT EXISTS (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int > 1)
""");
    }

    public override async Task All_with_predicate_after_GroupBy_without_aggregate(bool async)
    {
        await base.All_with_predicate_after_GroupBy_without_aggregate(async);

        AssertSql(
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING count(*)::int <= 1)
""");
    }

    public override async Task GroupBy_aggregate_followed_by_another_GroupBy_aggregate(bool async)
    {
        await base.GroupBy_aggregate_followed_by_another_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT o1."Key0" AS "Key", COALESCE(sum(o1."Count"), 0)::int AS "Count"
FROM (
    SELECT o0."Count", 1 AS "Key0"
    FROM (
        SELECT count(*)::int AS "Count"
        FROM "Orders" AS o
        GROUP BY o."CustomerID"
    ) AS o0
) AS o1
GROUP BY o1."Key0"
""");
    }

    public override async Task GroupBy_Count_in_projection(bool async)
    {
        await base.GroupBy_Count_in_projection(async);

        AssertSql(
            """
SELECT o."OrderID", o."OrderDate", EXISTS (
    SELECT 1
    FROM "Order Details" AS o0
    WHERE o."OrderID" = o0."OrderID" AND o0."ProductID" < 25) AS "HasOrderDetails", CASE
    WHEN (
        SELECT count(*)::int
        FROM (
            SELECT 1
            FROM "Order Details" AS o1
            INNER JOIN "Products" AS p ON o1."ProductID" = p."ProductID"
            WHERE o."OrderID" = o1."OrderID" AND o1."ProductID" < 25
            GROUP BY p."ProductName"
        ) AS s) > 1 THEN TRUE
    ELSE FALSE
END AS "HasMultipleProducts"
FROM "Orders" AS o
WHERE o."OrderDate" IS NOT NULL
""");
    }

    public override async Task GroupBy_nominal_type_count(bool async)
    {
        await base.GroupBy_nominal_type_count(async);

        AssertSql(
            """
SELECT count(*)::int
FROM (
    SELECT 1
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o0
""");
    }

    public override async Task GroupBy_based_on_renamed_property_simple(bool async)
    {
        await base.GroupBy_based_on_renamed_property_simple(async);

        AssertSql(
            """
SELECT c."City" AS "Renamed", count(*)::int AS "Count"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    public override async Task GroupBy_based_on_renamed_property_complex(bool async)
    {
        await base.GroupBy_based_on_renamed_property_complex(async);

        AssertSql(
            """
SELECT c0."Renamed" AS "Key", count(*)::int AS "Count"
FROM (
    SELECT DISTINCT c."City" AS "Renamed", c."CustomerID"
    FROM "Customers" AS c
) AS c0
GROUP BY c0."Renamed"
""");
    }

    public override async Task Join_groupby_anonymous_orderby_anonymous_projection(bool async)
    {
        await base.Join_groupby_anonymous_orderby_anonymous_projection(async);

        AssertSql(
            """
SELECT c."CustomerID", o."OrderDate"
FROM "Customers" AS c
INNER JOIN "Orders" AS o ON c."CustomerID" = o."CustomerID"
GROUP BY c."CustomerID", o."OrderDate"
ORDER BY o."OrderDate" NULLS FIRST
""");
    }

    public override async Task Odata_groupby_empty_key(bool async)
    {
        await base.Odata_groupby_empty_key(async);

        AssertSql(
            """
SELECT 'TotalAmount' AS "Name", COALESCE(sum(o0."OrderID"::numeric), 0.0) AS "Value"
FROM (
    SELECT o."OrderID", 1 AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_with_group_key_access_thru_navigation(bool async)
    {
        await base.GroupBy_with_group_key_access_thru_navigation(async);

        AssertSql(
            """
SELECT o0."CustomerID" AS "Key", COALESCE(sum(o."OrderID"), 0)::int AS "Aggregate"
FROM "Order Details" AS o
INNER JOIN "Orders" AS o0 ON o."OrderID" = o0."OrderID"
GROUP BY o0."CustomerID"
""");
    }

    public override async Task GroupBy_with_group_key_access_thru_nested_navigation(bool async)
    {
        await base.GroupBy_with_group_key_access_thru_nested_navigation(async);

        AssertSql(
            """
SELECT c."Country" AS "Key", COALESCE(sum(o."OrderID"), 0)::int AS "Aggregate"
FROM "Order Details" AS o
INNER JOIN "Orders" AS o0 ON o."OrderID" = o0."OrderID"
LEFT JOIN "Customers" AS c ON o0."CustomerID" = c."CustomerID"
GROUP BY c."Country"
""");
    }

    #endregion GroupBySelectFirst

    #region GroupByEntityType

    public override async Task GroupBy_with_group_key_being_navigation(bool async)
    {
        await base.GroupBy_with_group_key_being_navigation(async);

        AssertSql(
            """
SELECT o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate", COALESCE(sum(o."OrderID"), 0)::int AS "Aggregate"
FROM "Order Details" AS o
INNER JOIN "Orders" AS o0 ON o."OrderID" = o0."OrderID"
GROUP BY o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate"
""");
    }

    public override async Task GroupBy_with_group_key_being_nested_navigation(bool async)
    {
        await base.GroupBy_with_group_key_being_nested_navigation(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", COALESCE(sum(o."OrderID"), 0)::int AS "Aggregate"
FROM "Order Details" AS o
INNER JOIN "Orders" AS o0 ON o."OrderID" = o0."OrderID"
LEFT JOIN "Customers" AS c ON o0."CustomerID" = c."CustomerID"
GROUP BY c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
""");
    }

    public override async Task GroupBy_with_group_key_being_navigation_with_entity_key_projection(bool async)
    {
        await base.GroupBy_with_group_key_being_navigation_with_entity_key_projection(async);

        AssertSql(
            """
SELECT o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate"
FROM "Order Details" AS o
INNER JOIN "Orders" AS o0 ON o."OrderID" = o0."OrderID"
GROUP BY o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate"
""");
    }

    public override async Task GroupBy_with_group_key_being_navigation_with_complex_projection(bool async)
    {
        await base.GroupBy_with_group_key_being_navigation_with_complex_projection(async);

        AssertSql();
    }

    public override async Task GroupBy_with_order_by_skip_and_another_order_by(bool async)
    {
        await base.GroupBy_with_order_by_skip_and_another_order_by(async);

        AssertSql(
            """
@p='80'

SELECT COALESCE(sum(o0."OrderID"), 0)::int
FROM (
    SELECT o."OrderID", o."CustomerID"
    FROM "Orders" AS o
    ORDER BY o."CustomerID" NULLS FIRST, o."OrderID" NULLS FIRST
    OFFSET @p
) AS o0
GROUP BY o0."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_Count_with_predicate(bool async)
    {
        await base.GroupBy_Property_Select_Count_with_predicate(async);

        AssertSql(
            """
SELECT count(*) FILTER (WHERE o."OrderID" < 10300)::int
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_Property_Select_LongCount_with_predicate(bool async)
    {
        await base.GroupBy_Property_Select_LongCount_with_predicate(async);

        AssertSql(
            """
SELECT count(*) FILTER (WHERE o."OrderID" < 10300)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_orderby_projection_with_coalesce_operation(bool async)
    {
        await base.GroupBy_orderby_projection_with_coalesce_operation(async);

        AssertSql(
            """
SELECT COALESCE(c."City", 'Unknown') AS "Locality", count(*)::int AS "Count"
FROM "Customers" AS c
GROUP BY c."City"
ORDER BY count(*)::int DESC NULLS LAST, c."City" NULLS FIRST
""");
    }

    public override async Task GroupBy_let_orderby_projection_with_coalesce_operation(bool async)
    {
        await base.GroupBy_let_orderby_projection_with_coalesce_operation(async);

        AssertSql();
    }

    public override async Task GroupBy_Min_Where_optional_relationship(bool async)
    {
        await base.GroupBy_Min_Where_optional_relationship(async);

        AssertSql(
            """
SELECT c."CustomerID" AS "Key", count(*)::int AS "Count"
FROM "Orders" AS o
LEFT JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
GROUP BY c."CustomerID"
HAVING count(*)::int <> 2
""");
    }

    public override async Task GroupBy_Min_Where_optional_relationship_2(bool async)
    {
        await base.GroupBy_Min_Where_optional_relationship_2(async);

        AssertSql(
            """
SELECT c."CustomerID" AS "Key", count(*)::int AS "Count"
FROM "Orders" AS o
LEFT JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
GROUP BY c."CustomerID"
HAVING count(*)::int < 2 OR count(*)::int > 2
""");
    }

    public override async Task GroupBy_aggregate_over_a_subquery(bool async)
    {
        await base.GroupBy_aggregate_over_a_subquery(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", (
    SELECT count(*)::int
    FROM "Customers" AS c
    WHERE c."CustomerID" = o."CustomerID") AS "Count"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_aggregate_join_with_grouping_key(bool async)
    {
        await base.GroupBy_aggregate_join_with_grouping_key(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", o0."Count"
FROM (
    SELECT o."CustomerID" AS "Key", count(*)::int AS "Count"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o0
INNER JOIN "Customers" AS c ON o0."Key" = c."CustomerID"
""");
    }

    public override async Task GroupBy_aggregate_join_with_group_result(bool async)
    {
        await base.GroupBy_aggregate_join_with_group_result(async);

        AssertSql(
            """
SELECT o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate"
FROM (
    SELECT o."CustomerID" AS "Key", max(o."OrderDate") AS "LastOrderDate"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o1
INNER JOIN "Orders" AS o0 ON (o1."Key" = o0."CustomerID" OR (o1."Key" IS NULL AND o0."CustomerID" IS NULL)) AND (o1."LastOrderDate" = o0."OrderDate" OR (o1."LastOrderDate" IS NULL AND o0."OrderDate" IS NULL))
""");
    }

    public override async Task GroupBy_aggregate_from_right_side_of_join(bool async)
    {
        await base.GroupBy_aggregate_from_right_side_of_join(async);

        AssertSql(
            """
@p='10'

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", o0."Max"
FROM "Customers" AS c
INNER JOIN (
    SELECT o."CustomerID" AS "Key", max(o."OrderDate") AS "Max"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o0 ON c."CustomerID" = o0."Key"
ORDER BY o0."Max" NULLS FIRST, c."CustomerID" NULLS FIRST
LIMIT @p OFFSET @p
""");
    }

    public override async Task GroupBy_aggregate_join_another_GroupBy_aggregate(bool async)
    {
        await base.GroupBy_aggregate_join_another_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT o1."Key", o1."Total", o2."ThatYear"
FROM (
    SELECT o."CustomerID" AS "Key", count(*)::int AS "Total"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o1
INNER JOIN (
    SELECT o0."CustomerID" AS "Key", count(*)::int AS "ThatYear"
    FROM "Orders" AS o0
    WHERE date_part('year', o0."OrderDate")::int = 1997
    GROUP BY o0."CustomerID"
) AS o2 ON o1."Key" = o2."Key"
""");
    }

    public override async Task GroupBy_aggregate_after_skip_0_take_0(bool async)
    {
        await base.GroupBy_aggregate_after_skip_0_take_0(async);

        AssertSql(
            """
@p='0'

SELECT o0."CustomerID" AS "Key", count(*)::int AS "Total"
FROM (
    SELECT o."CustomerID"
    FROM "Orders" AS o
    LIMIT @p OFFSET @p
) AS o0
GROUP BY o0."CustomerID"
""");
    }

    public override async Task GroupBy_skip_0_take_0_aggregate(bool async)
    {
        await base.GroupBy_skip_0_take_0_aggregate(async);

        AssertSql(
            """
@p='0'

SELECT o."CustomerID" AS "Key", count(*)::int AS "Total"
FROM "Orders" AS o
WHERE o."OrderID" > 10500
GROUP BY o."CustomerID"
LIMIT @p OFFSET @p
""");
    }

    public override async Task GroupBy_aggregate_followed_another_GroupBy_aggregate(bool async)
    {
        await base.GroupBy_aggregate_followed_another_GroupBy_aggregate(async);

        AssertSql(
            """
SELECT o1."CustomerID" AS "Key", count(*)::int AS "Count"
FROM (
    SELECT o0."CustomerID"
    FROM (
        SELECT o."CustomerID", date_part('year', o."OrderDate")::int AS "Year"
        FROM "Orders" AS o
    ) AS o0
    GROUP BY o0."CustomerID", o0."Year"
) AS o1
GROUP BY o1."CustomerID"
""");
    }

    public override async Task GroupBy_aggregate_without_selectMany_selecting_first(bool async)
    {
        await base.GroupBy_aggregate_without_selectMany_selecting_first(async);

        AssertSql(
            """
SELECT o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate"
FROM (
    SELECT min(o."OrderID") AS c
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o1
CROSS JOIN "Orders" AS o0
WHERE o0."OrderID" = o1.c
""");
    }

    public override async Task GroupBy_aggregate_left_join_GroupBy_aggregate_left_join(bool async)
    {
        await base.GroupBy_aggregate_left_join_GroupBy_aggregate_left_join(async);

        AssertSql(
            """
SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT MIN([o].[OrderID]) AS [c]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]
CROSS JOIN [Orders] AS [o0]
WHERE [o0].[OrderID] = [t].[c]
""");
    }

    public override async Task GroupBy_selecting_grouping_key_list(bool async)
    {
        await base.GroupBy_selecting_grouping_key_list(async);

        AssertSql(
            """
SELECT o1."CustomerID", o0."CustomerID", o0."OrderID"
FROM (
    SELECT o."CustomerID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o1
LEFT JOIN "Orders" AS o0 ON o1."CustomerID" = o0."CustomerID"
ORDER BY o1."CustomerID" NULLS FIRST
""");
    }

    public override async Task GroupBy_with_grouping_key_using_Like(bool async)
    {
        await base.GroupBy_with_grouping_key_using_Like(async);

        AssertSql(
            """
SELECT o0."Key", count(*)::int AS "Count"
FROM (
    SELECT o."CustomerID" LIKE 'A%' AND o."CustomerID" IS NOT NULL AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_with_grouping_key_DateTime_Day(bool async)
    {
        await base.GroupBy_with_grouping_key_DateTime_Day(async);

        AssertSql(
            """
SELECT o0."Key", count(*)::int AS "Count"
FROM (
    SELECT date_part('day', o."OrderDate")::int AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task GroupBy_with_cast_inside_grouping_aggregate(bool async)
    {
        await base.GroupBy_with_cast_inside_grouping_aggregate(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", count(*)::int AS "Count", COALESCE(sum(o."OrderID"::bigint), 0.0)::bigint AS "Sum"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    #endregion ResultOperatorsAfterGroupBy

    #region GroupByInSubquery

    public override async Task Complex_query_with_groupBy_in_subquery1(bool async)
    {
        await base.Complex_query_with_groupBy_in_subquery1(async);

        AssertSql(
            """
SELECT c."CustomerID", o0."Sum", o0."CustomerID"
FROM "Customers" AS c
LEFT JOIN LATERAL (
    SELECT COALESCE(sum(o."OrderID"), 0)::int AS "Sum", o."CustomerID"
    FROM "Orders" AS o
    WHERE c."CustomerID" = o."CustomerID"
    GROUP BY o."CustomerID"
) AS o0 ON TRUE
ORDER BY c."CustomerID" NULLS FIRST
""");
    }

    public override async Task Complex_query_with_groupBy_in_subquery2(bool async)
    {
        await base.Complex_query_with_groupBy_in_subquery2(async);

        AssertSql(
            """
SELECT c."CustomerID", o0."Max", o0."Sum", o0."CustomerID"
FROM "Customers" AS c
LEFT JOIN LATERAL (
    SELECT max(length(o."CustomerID")::int) AS "Max", COALESCE(sum(o."OrderID"), 0)::int AS "Sum", o."CustomerID"
    FROM "Orders" AS o
    WHERE c."CustomerID" = o."CustomerID"
    GROUP BY o."CustomerID"
) AS o0 ON TRUE
ORDER BY c."CustomerID" NULLS FIRST
""");
    }

    public override async Task Complex_query_with_groupBy_in_subquery3(bool async)
    {
        await base.Complex_query_with_groupBy_in_subquery3(async);

        AssertSql(
            """
SELECT c."CustomerID", o0."Max", o0."Sum", o0."CustomerID"
FROM "Customers" AS c
LEFT JOIN LATERAL (
    SELECT max(length(o."CustomerID")::int) AS "Max", COALESCE(sum(o."OrderID"), 0)::int AS "Sum", o."CustomerID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
) AS o0 ON TRUE
ORDER BY c."CustomerID" NULLS FIRST
""");
    }

    public override async Task Group_by_with_projection_into_DTO(bool async)
    {
        await base.Group_by_with_projection_into_DTO(async);

        AssertSql(
            """
SELECT o."OrderID"::bigint AS "Id", count(*)::int AS "Count"
FROM "Orders" AS o
GROUP BY o."OrderID"
""");
    }

    public override async Task Where_select_function_groupby_followed_by_another_select_with_aggregates(bool async)
    {
        await base.Where_select_function_groupby_followed_by_another_select_with_aggregates(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", COALESCE(sum(CASE
    WHEN 2020 - date_part('year', o."OrderDate")::int <= 30 THEN o."OrderID"
    ELSE 0
END), 0)::int AS "Sum1", COALESCE(sum(CASE
    WHEN 2020 - date_part('year', o."OrderDate")::int > 30 AND 2020 - date_part('year', o."OrderDate")::int <= 60 THEN o."OrderID"
    ELSE 0
END), 0)::int AS "Sum2"
FROM "Orders" AS o
WHERE o."CustomerID" LIKE 'A%'
GROUP BY o."CustomerID"
""");
    }

    public override async Task Group_by_column_project_constant(bool async)
    {
        await base.Group_by_column_project_constant(async);

        AssertSql(
            """
SELECT 42
FROM "Orders" AS o
GROUP BY o."CustomerID"
ORDER BY o."CustomerID" NULLS FIRST
""");
    }

    public override async Task Key_plus_key_in_projection(bool async)
    {
        await base.Key_plus_key_in_projection(async);

        AssertSql(
            """
SELECT o."OrderID" + o."OrderID" AS "Value", avg(o."OrderID"::double precision) AS "Average"
FROM "Orders" AS o
LEFT JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
GROUP BY o."OrderID"
""");
    }

    public override async Task Group_by_with_arithmetic_operation_inside_aggregate(bool async)
    {
        await base.Group_by_with_arithmetic_operation_inside_aggregate(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", COALESCE(sum(o."OrderID" + length(o."CustomerID")::int), 0)::int AS "Sum"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_scalar_subquery(bool async)
    {
        await base.GroupBy_scalar_subquery(async);

        AssertSql(
            """
SELECT o0."Key", count(*)::int AS "Count"
FROM (
    SELECT (
        SELECT c."ContactName"
        FROM "Customers" AS c
        WHERE c."CustomerID" = o."CustomerID"
        LIMIT 1) AS "Key"
    FROM "Orders" AS o
) AS o0
GROUP BY o0."Key"
""");
    }

    public override async Task AsEnumerable_in_subquery_for_GroupBy(bool async)
    {
        await base.AsEnumerable_in_subquery_for_GroupBy(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", s."OrderID", s."CustomerID", s."EmployeeID", s."OrderDate", s."CustomerID0"
FROM "Customers" AS c
LEFT JOIN LATERAL (
    SELECT o3."OrderID", o3."CustomerID", o3."EmployeeID", o3."OrderDate", o1."CustomerID" AS "CustomerID0"
    FROM (
        SELECT o."CustomerID"
        FROM "Orders" AS o
        WHERE o."CustomerID" = c."CustomerID"
        GROUP BY o."CustomerID"
    ) AS o1
    LEFT JOIN (
        SELECT o2."OrderID", o2."CustomerID", o2."EmployeeID", o2."OrderDate"
        FROM (
            SELECT o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate", ROW_NUMBER() OVER(PARTITION BY o0."CustomerID" ORDER BY o0."OrderDate" DESC NULLS LAST) AS row
            FROM "Orders" AS o0
            WHERE o0."CustomerID" = c."CustomerID"
        ) AS o2
        WHERE o2.row <= 1
    ) AS o3 ON o1."CustomerID" = o3."CustomerID"
) AS s ON TRUE
WHERE c."CustomerID" LIKE 'F%'
ORDER BY c."CustomerID" NULLS FIRST, s."CustomerID0" NULLS FIRST
""");
    }

    public override async Task GroupBy_aggregate_from_multiple_query_in_same_projection(bool async)
    {
        await base.GroupBy_aggregate_from_multiple_query_in_same_projection(async);

        AssertSql(
            """
SELECT [t].[CustomerID], [t0].[Key], [t0].[C], [t0].[c0]
FROM (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
) AS [t]
OUTER APPLY (
    SELECT TOP(1) [e].[City] AS [Key], COUNT(*) + (
        SELECT COUNT(*)
        FROM [Orders] AS [o0]
        WHERE [t].[CustomerID] = [o0].[CustomerID] OR ([t].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL)) AS [C], 1 AS [c0]
    FROM [Employees] AS [e]
    WHERE [e].[City] = N'Seattle'
    GROUP BY [e].[City]
    ORDER BY (SELECT 1)
) AS [t0]
""");
    }

    public override async Task GroupBy_aggregate_from_multiple_query_in_same_projection_2(bool async)
    {
        await base.GroupBy_aggregate_from_multiple_query_in_same_projection_2(async);

        AssertSql(
            """
SELECT o."CustomerID" AS "Key", COALESCE((
    SELECT count(*)::int + min(o."OrderID")
    FROM "Employees" AS e
    WHERE e."City" = 'Seattle'
    GROUP BY e."City"
    ORDER BY (SELECT 1) NULLS FIRST
    LIMIT 1), 0) AS "A"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    public override async Task GroupBy_aggregate_from_multiple_query_in_same_projection_3(bool async)
    {
        await base.GroupBy_aggregate_from_multiple_query_in_same_projection_3(async);

        AssertSql(
            """
SELECT [o].[CustomerID] AS [Key], COALESCE((
    SELECT TOP(1) COUNT(*) + (
        SELECT COUNT(*)
        FROM [Orders] AS [o0]
        WHERE [o].[CustomerID] = [o0].[CustomerID] OR ([o].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL))
    FROM [Employees] AS [e]
    WHERE [e].[City] = N'Seattle'
    GROUP BY [e].[City]
    ORDER BY COUNT(*) + (
        SELECT COUNT(*)
        FROM [Orders] AS [o0]
        WHERE [o].[CustomerID] = [o0].[CustomerID] OR ([o].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL))), 0) AS [A]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
""");
    }

    public override async Task GroupBy_scalar_aggregate_in_set_operation(bool async)
    {
        await base.GroupBy_scalar_aggregate_in_set_operation(async);

        AssertSql(
            """
SELECT c."CustomerID", 0 AS "Sequence"
FROM "Customers" AS c
WHERE c."CustomerID" LIKE 'F%'
UNION
SELECT o."CustomerID", 1 AS "Sequence"
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    #endregion GroupByInSubquery

    #region GroupByAndDistinctWithCorrelatedCollection

    public override async Task Select_uncorrelated_collection_with_groupby_when_outer_is_distinct(bool async)
    {
        await base.Select_uncorrelated_collection_with_groupby_when_outer_is_distinct(async);

        AssertSql(
            """
SELECT s."City", p1."ProductID", p2.c, p2."ProductID"
FROM (
    SELECT DISTINCT c."City"
    FROM "Orders" AS o
    LEFT JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
    WHERE o."CustomerID" LIKE 'A%'
) AS s
LEFT JOIN LATERAL (
    SELECT p."ProductID"
    FROM "Products" AS p
    GROUP BY p."ProductID"
) AS p1 ON TRUE
LEFT JOIN LATERAL (
    SELECT count(*)::int AS c, p0."ProductID"
    FROM "Products" AS p0
    GROUP BY p0."ProductID"
) AS p2 ON TRUE
ORDER BY s."City" NULLS FIRST, p1."ProductID" NULLS FIRST
""");
    }

    public override async Task Select_correlated_collection_after_GroupBy_aggregate_when_identifier_does_not_change(bool async)
    {
        await base.Select_correlated_collection_after_GroupBy_aggregate_when_identifier_does_not_change(async);

        AssertSql(
            """
SELECT c0."CustomerID", o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate"
FROM (
    SELECT c."CustomerID"
    FROM "Customers" AS c
    GROUP BY c."CustomerID"
    HAVING c."CustomerID" LIKE 'F%'
) AS c0
LEFT JOIN "Orders" AS o ON c0."CustomerID" = o."CustomerID"
ORDER BY c0."CustomerID" NULLS FIRST
""");
    }

    public override async Task Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes(bool async)
    {
        await base.Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes(async);

        AssertSql(
            """
SELECT o1."CustomerID", o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate"
FROM (
    SELECT o."CustomerID"
    FROM "Orders" AS o
    GROUP BY o."CustomerID"
    HAVING o."CustomerID" LIKE 'F%'
) AS o1
LEFT JOIN "Orders" AS o0 ON o1."CustomerID" = o0."CustomerID"
ORDER BY o1."CustomerID" NULLS FIRST
""");
    }

    public override async Task Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes_to_complex(bool async)
        => await base.Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes_to_complex(async);

    //AssertSql(" ");
    public override async Task Complex_query_with_group_by_in_subquery5(bool async)
    {
        await base.Complex_query_with_group_by_in_subquery5(async);

        AssertSql(
            """
SELECT s.c, s."ProductID", c1."CustomerID", c1."City"
FROM (
    SELECT COALESCE(sum(o."ProductID" + o."OrderID" * 1000), 0)::int AS c, o."ProductID", min(o."OrderID" / 100) AS c0
    FROM "Order Details" AS o
    INNER JOIN "Orders" AS o0 ON o."OrderID" = o0."OrderID"
    LEFT JOIN "Customers" AS c ON o0."CustomerID" = c."CustomerID"
    WHERE c."CustomerID" = 'ALFKI'
    GROUP BY o."ProductID"
) AS s
LEFT JOIN LATERAL (
    SELECT c0."CustomerID", c0."City"
    FROM "Customers" AS c0
    WHERE length(c0."CustomerID")::int < s.c0
) AS c1 ON TRUE
ORDER BY s."ProductID" NULLS FIRST, c1."CustomerID" NULLS FIRST
""");
    }

    public override async Task Complex_query_with_groupBy_in_subquery4(bool async)
    {
        await base.Complex_query_with_groupBy_in_subquery4(async);

        AssertSql(
            """
SELECT c."CustomerID", s1."Sum", s1."Count", s1."Key"
FROM "Customers" AS c
LEFT JOIN LATERAL (
    SELECT COALESCE(sum(s."OrderID"), 0)::int AS "Sum", (
        SELECT count(*)::int
        FROM (
            SELECT o0."CustomerID", COALESCE(c1."City", '') || COALESCE(o0."CustomerID", '') AS "Key"
            FROM "Orders" AS o0
            LEFT JOIN "Customers" AS c1 ON o0."CustomerID" = c1."CustomerID"
            WHERE c."CustomerID" = o0."CustomerID"
        ) AS s0
        LEFT JOIN "Customers" AS c2 ON s0."CustomerID" = c2."CustomerID"
        WHERE (s."Key" = s0."Key" OR (s."Key" IS NULL AND s0."Key" IS NULL)) AND COALESCE(c2."City", '') || COALESCE(s0."CustomerID", '') LIKE 'Lon%') AS "Count", s."Key"
    FROM (
        SELECT o."OrderID", COALESCE(c0."City", '') || COALESCE(o."CustomerID", '') AS "Key"
        FROM "Orders" AS o
        LEFT JOIN "Customers" AS c0 ON o."CustomerID" = c0."CustomerID"
        WHERE c."CustomerID" = o."CustomerID"
    ) AS s
    GROUP BY s."Key"
) AS s1 ON TRUE
ORDER BY c."CustomerID" NULLS FIRST
""");
    }

    public override async Task GroupBy_aggregate_SelectMany(bool async)
    {
        await base.GroupBy_aggregate_SelectMany(async);

        AssertSql();
    }

    public override async Task Final_GroupBy_property_entity(bool async)
    {
        await base.Final_GroupBy_property_entity(async);

        AssertSql(
            """
SELECT c."City", c."CustomerID", c."Address", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
ORDER BY c."City" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_entity(bool async)
    {
        await base.Final_GroupBy_entity(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate"
FROM "Orders" AS o
LEFT JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
WHERE o."OrderID" < 10500
ORDER BY c."CustomerID" NULLS FIRST, c."Address" NULLS FIRST, c."City" NULLS FIRST, c."CompanyName" NULLS FIRST, c."ContactName" NULLS FIRST, c."ContactTitle" NULLS FIRST, c."Country" NULLS FIRST, c."Fax" NULLS FIRST, c."Phone" NULLS FIRST, c."PostalCode" NULLS FIRST, c."Region" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_property_entity_non_nullable(bool async)
    {
        await base.Final_GroupBy_property_entity_non_nullable(async);

        AssertSql(
            """
SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
WHERE o."OrderID" < 10500
ORDER BY o."OrderID" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_property_anonymous_type(bool async)
    {
        await base.Final_GroupBy_property_anonymous_type(async);

        AssertSql(
            """
SELECT c."City", c."ContactName", c."ContactTitle"
FROM "Customers" AS c
ORDER BY c."City" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_multiple_properties_entity(bool async)
    {
        await base.Final_GroupBy_multiple_properties_entity(async);

        AssertSql(
            """
SELECT c."City", c."Region", c."CustomerID", c."Address", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode"
FROM "Customers" AS c
ORDER BY c."City" NULLS FIRST, c."Region" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_complex_key_entity(bool async)
    {
        await base.Final_GroupBy_complex_key_entity(async);

        AssertSql(
            """
SELECT c0."City", c0."Region", c0."Constant", c0."CustomerID", c0."Address", c0."CompanyName", c0."ContactName", c0."ContactTitle", c0."Country", c0."Fax", c0."Phone", c0."PostalCode"
FROM (
    SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", 1 AS "Constant"
    FROM "Customers" AS c
) AS c0
ORDER BY c0."City" NULLS FIRST, c0."Region" NULLS FIRST, c0."Constant" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_nominal_type_entity(bool async)
    {
        await base.Final_GroupBy_nominal_type_entity(async);

        AssertSql(
            """
SELECT c0."City", c0."Constant", c0."CustomerID", c0."Address", c0."CompanyName", c0."ContactName", c0."ContactTitle", c0."Country", c0."Fax", c0."Phone", c0."PostalCode", c0."Region"
FROM (
    SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", 1 AS "Constant"
    FROM "Customers" AS c
) AS c0
ORDER BY c0."City" NULLS FIRST, c0."Constant" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_property_anonymous_type_element_selector(bool async)
    {
        await base.Final_GroupBy_property_anonymous_type_element_selector(async);

        AssertSql(
            """
SELECT c."City", c."ContactName", c."ContactTitle"
FROM "Customers" AS c
ORDER BY c."City" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection(async);

        AssertSql(
            """
SELECT c."City", c."CustomerID", c."Address", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region", o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate"
FROM "Customers" AS c
LEFT JOIN "Orders" AS o ON c."CustomerID" = o."CustomerID"
WHERE c."Country" = 'USA'
ORDER BY c."City" NULLS FIRST, c."CustomerID" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_property_entity_projecting_collection(bool async)
    {
        await base.Final_GroupBy_property_entity_projecting_collection(async);

        AssertSql(
            """
SELECT c."City", c."CustomerID", o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate"
FROM "Customers" AS c
LEFT JOIN "Orders" AS o ON c."CustomerID" = o."CustomerID"
WHERE c."Country" = 'USA'
ORDER BY c."City" NULLS FIRST, c."CustomerID" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_property_entity_projecting_collection_composed(bool async)
    {
        await base.Final_GroupBy_property_entity_projecting_collection_composed(async);

        AssertSql(
            """
SELECT c."City", c."CustomerID", o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate"
FROM "Customers" AS c
LEFT JOIN (
    SELECT o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate"
    FROM "Orders" AS o
    WHERE o."OrderID" < 11000
) AS o0 ON c."CustomerID" = o0."CustomerID"
WHERE c."Country" = 'USA'
ORDER BY c."City" NULLS FIRST, c."CustomerID" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_property_entity_projecting_collection_and_single_result(bool async)
    {
        await base.Final_GroupBy_property_entity_projecting_collection_and_single_result(async);

        AssertSql(
            """
SELECT c."City", c."CustomerID", o1."OrderID", o1."CustomerID", o1."EmployeeID", o1."OrderDate", o3."OrderID", o3."CustomerID", o3."EmployeeID", o3."OrderDate"
FROM "Customers" AS c
LEFT JOIN (
    SELECT o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate"
    FROM "Orders" AS o
    WHERE o."OrderID" < 11000
) AS o1 ON c."CustomerID" = o1."CustomerID"
LEFT JOIN (
    SELECT o2."OrderID", o2."CustomerID", o2."EmployeeID", o2."OrderDate"
    FROM (
        SELECT o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate", ROW_NUMBER() OVER(PARTITION BY o0."CustomerID" ORDER BY o0."OrderDate" DESC NULLS LAST) AS row
        FROM "Orders" AS o0
    ) AS o2
    WHERE o2.row <= 1
) AS o3 ON c."CustomerID" = o3."CustomerID"
WHERE c."Country" = 'USA'
ORDER BY c."City" NULLS FIRST, c."CustomerID" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy_TagWith(bool async)
    {
        await base.Final_GroupBy_TagWith(async);

        AssertSql(
            """
-- foo

SELECT c."City", c."CustomerID", c."Address", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
ORDER BY c."City" NULLS FIRST
""");
    }

    #endregion Final

    #region GroupByWithoutAggregate

    public override async Task GroupBy_Where_with_grouping_result(bool async)
    {
        await base.GroupBy_Where_with_grouping_result(async);

        AssertSql();
    }

    public override async Task GroupBy_OrderBy_with_grouping_result(bool async)
    {
        await base.GroupBy_OrderBy_with_grouping_result(async);

        AssertSql();
    }

    public override async Task GroupBy_SelectMany(bool async)
    {
        await base.GroupBy_SelectMany(async);

        AssertSql();
    }

    public override async Task OrderBy_GroupBy_SelectMany(bool async)
    {
        await base.OrderBy_GroupBy_SelectMany(async);

        AssertSql();
    }

    public override async Task OrderBy_GroupBy_SelectMany_shadow(bool async)
    {
        await base.OrderBy_GroupBy_SelectMany_shadow(async);

        AssertSql();
    }

    public override async Task GroupBy_with_orderby_take_skip_distinct_followed_by_group_key_projection(bool async)
    {
        await base.GroupBy_with_orderby_take_skip_distinct_followed_by_group_key_projection(async);

        AssertSql();
    }

    public override async Task GroupBy_Distinct(bool async)
    {
        await base.GroupBy_Distinct(async);

        AssertSql();
    }

    public override async Task GroupBy_complex_key_without_aggregate(bool async)
    {
        await base.GroupBy_complex_key_without_aggregate(async);

        AssertSql(
            """
SELECT s1."Key", s3."OrderID", s3."CustomerID", s3."EmployeeID", s3."OrderDate", s3."CustomerID0"
FROM (
    SELECT s."Key"
    FROM (
        SELECT substring(c."CustomerID", 1, 1) AS "Key"
        FROM "Orders" AS o
        LEFT JOIN "Customers" AS c ON o."CustomerID" = c."CustomerID"
    ) AS s
    GROUP BY s."Key"
) AS s1
LEFT JOIN (
    SELECT s2."OrderID", s2."CustomerID", s2."EmployeeID", s2."OrderDate", s2."CustomerID0", s2."Key"
    FROM (
        SELECT s0."OrderID", s0."CustomerID", s0."EmployeeID", s0."OrderDate", s0."CustomerID0", s0."Key", ROW_NUMBER() OVER(PARTITION BY s0."Key" ORDER BY s0."OrderID" NULLS FIRST, s0."CustomerID0" NULLS FIRST) AS row
        FROM (
            SELECT o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate", c0."CustomerID" AS "CustomerID0", substring(c0."CustomerID", 1, 1) AS "Key"
            FROM "Orders" AS o0
            LEFT JOIN "Customers" AS c0 ON o0."CustomerID" = c0."CustomerID"
        ) AS s0
    ) AS s2
    WHERE 1 < s2.row AND s2.row <= 3
) AS s3 ON s1."Key" = s3."Key"
ORDER BY s1."Key" NULLS FIRST, s3."OrderID" NULLS FIRST
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_OrderBy_Average(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => from o in ss.Set<Order>()
                  group o by new { o.CustomerID }
                  into g
                  select g.OrderBy(e => e.OrderID).Select(e => (int?)e.OrderID).Average());

        AssertSql(
            """
SELECT avg(o."OrderID"::double precision ORDER BY o."OrderID" NULLS FIRST)
FROM "Orders" AS o
GROUP BY o."CustomerID"
""");
    }

    #endregion GroupByAndDistinctWithCorrelatedCollection

    // See aggregate tests over TimeSpan in GearsOfWarQueryNpsgqlTest

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
