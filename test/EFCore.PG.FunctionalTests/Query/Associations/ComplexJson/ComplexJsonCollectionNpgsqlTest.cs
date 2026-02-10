namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonCollectionNpgsqlTest(ComplexJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonCollectionRelationalTestBase<ComplexJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (
    SELECT count(*)::int
    FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS (
        "Id" integer,
        "Int" integer,
        "Ints" jsonb,
        "Name" text,
        "String" text,
        "NestedCollection" jsonb,
        "OptionalNestedAssociate" jsonb,
        "RequiredNestedAssociate" jsonb
    )) WITH ORDINALITY AS a) = 2
""");
    }

    public override async Task Where()
    {
        await base.Where();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (
    SELECT count(*)::int
    FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS ("Int" integer)) WITH ORDINALITY AS a
    WHERE a."Int" <> 8) = 2
""");
    }

    public override async Task OrderBy_ElementAt()
    {
        await base.OrderBy_ElementAt();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (
    SELECT a."Int"
    FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS (
        "Id" integer,
        "Int" integer
    )) WITH ORDINALITY AS a
    ORDER BY a."Id" NULLS FIRST
    LIMIT 1 OFFSET 0) = 8
""");
    }

    #region Distinct

    public override async Task Distinct()
    {
        await base.Distinct();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT DISTINCT a."Id", a."Int", a."Ints", a."Name", a."String", a."NestedCollection" AS c, a."OptionalNestedAssociate" AS c0, a."RequiredNestedAssociate" AS c1
        FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS (
            "Id" integer,
            "Int" integer,
            "Ints" jsonb,
            "Name" text,
            "String" text,
            "NestedCollection" jsonb,
            "OptionalNestedAssociate" jsonb,
            "RequiredNestedAssociate" jsonb
        )) WITH ORDINALITY AS a
    ) AS a0) = 2
""");
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Distinct_projected(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Distinct_over_projected_nested_collection()
    {
        await base.Distinct_over_projected_nested_collection();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT DISTINCT a."NestedCollection" AS c
        FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS ("NestedCollection" jsonb)) WITH ORDINALITY AS a
    ) AS a0) = 2
""");
    }

    public override async Task Distinct_over_projected_filtered_nested_collection()
    {
        await base.Distinct_over_projected_filtered_nested_collection();

        AssertSql();
    }

    #endregion Distinct

    #region Index

    public override async Task Index_constant()
    {
        await base.Index_constant();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (CAST(r."AssociateCollection" #>> '{0,Int}' AS integer)) = 8
""");
    }

    public override async Task Index_parameter()
    {
        await base.Index_parameter();

        AssertSql(
            """
@i='0'

SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (CAST(r."AssociateCollection" #>> ARRAY[@i,'Int']::text[] AS integer)) = 8
""");
    }

    public override async Task Index_column()
    {
        await base.Index_column();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (CAST(r."AssociateCollection" #>> ARRAY[r."Id" - 1,'Int']::text[] AS integer)) = 8
""");
    }

    public override async Task Index_out_of_bounds()
    {
        await base.Index_out_of_bounds();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (CAST(r."AssociateCollection" #>> '{9999,Int}' AS integer)) = 8
""");
    }

    #endregion Index

    #region GroupBy

    [ConditionalFact]
    public override async Task GroupBy()
    {
        await base.GroupBy();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE 16 IN (
    SELECT COALESCE(sum(a."Int"), 0)::int
    FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS (
        "Int" integer,
        "String" text
    )) WITH ORDINALITY AS a
    GROUP BY a."String"
)
""");
    }

    #endregion GroupBy

    public override async Task Select_within_Select_within_Select_with_aggregates()
    {
        await base.Select_within_Select_within_Select_with_aggregates();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(sum((
        SELECT max(n."Int")
        FROM ROWS FROM (jsonb_to_recordset(a."NestedCollection") AS ("Int" integer)) WITH ORDINALITY AS n)), 0)::int
    FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS ("NestedCollection" jsonb)) WITH ORDINALITY AS a)
FROM "RootEntity" AS r
""");
    }

    public override async Task Index_on_nested_collection()
    {
        await base.Index_on_nested_collection();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (CAST(r."RequiredAssociate" #>> '{NestedCollection,0,Int}' AS integer)) = 8
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
