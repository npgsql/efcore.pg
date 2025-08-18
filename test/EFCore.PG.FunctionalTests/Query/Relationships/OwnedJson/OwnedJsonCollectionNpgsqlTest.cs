// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedJson;

public class OwnedJsonCollectionNpgsqlTest(OwnedJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedJsonCollectionRelationalTestBase<OwnedJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (
    SELECT count(*)::int
    FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS (
        "Id" integer,
        "Int" integer,
        "Name" text,
        "String" text,
        "NestedCollection" jsonb,
        "OptionalNested" jsonb,
        "RequiredNested" jsonb
    )) WITH ORDINALITY AS r0) = 2
""");
    }

    public override async Task Where()
    {
        await base.Where();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (
    SELECT count(*)::int
    FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS (
        "Id" integer,
        "Int" integer,
        "Name" text,
        "String" text
    )) WITH ORDINALITY AS r0
    WHERE r0."Int" <> 8) = 2
""");
    }

    public override async Task OrderBy_ElementAt()
    {
        await base.OrderBy_ElementAt();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (
    SELECT r0."Int"
    FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS (
        "Id" integer,
        "Int" integer,
        "Name" text,
        "String" text
    )) WITH ORDINALITY AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1 OFFSET 0) = 8
""");
    }

    #region Distinct

    public override async Task Distinct()
    {
        await base.Distinct();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT DISTINCT r."Id", r0."Id" AS "Id0", r0."Int", r0."Name", r0."String", r0."NestedCollection" AS c, r0."OptionalNested" AS c0, r0."RequiredNested" AS c1
        FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS (
            "Id" integer,
            "Int" integer,
            "Name" text,
            "String" text,
            "NestedCollection" jsonb,
            "OptionalNested" jsonb,
            "RequiredNested" jsonb
        )) WITH ORDINALITY AS r0
    ) AS r1) = 2
""");
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Distinct_projected(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."Id", r1."Id", r1."Id0", r1."Int", r1."Name", r1."String", r1.c, r1.c0, r1.c1
FROM "RootEntity" AS r
LEFT JOIN LATERAL (
    SELECT DISTINCT r."Id", r0."Id" AS "Id0", r0."Int", r0."Name", r0."String", r0."NestedCollection" AS c, r0."OptionalNested" AS c0, r0."RequiredNested" AS c1
    FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS (
        "Id" integer,
        "Int" integer,
        "Name" text,
        "String" text,
        "NestedCollection" jsonb,
        "OptionalNested" jsonb,
        "RequiredNested" jsonb
    )) WITH ORDINALITY AS r0
) AS r1 ON TRUE
ORDER BY r."Id" NULLS FIRST, r1."Id0" NULLS FIRST, r1."Int" NULLS FIRST, r1."Name" NULLS FIRST
""");
        }
    }

    public override async Task Distinct_over_projected_nested_collection()
    {
        await base.Distinct_over_projected_nested_collection();

        AssertSql();
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
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (CAST(r."RelatedCollection" #>> '{0,Int}' AS integer)) = 8
""");
    }

    public override async Task Index_parameter()
    {
        await base.Index_parameter();

        AssertSql(
            """
@i='?' (DbType = Int32)

SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (CAST(r."RelatedCollection" #>> ARRAY[@i,'Int']::text[] AS integer)) = 8
""");
    }

    public override async Task Index_column()
    {
        await base.Index_column();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (CAST(r."RelatedCollection" #>> ARRAY[r."Id" - 1,'Int']::text[] AS integer)) = 8
""");
    }

    public override async Task Index_out_of_bounds()
    {
        await base.Index_out_of_bounds();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (CAST(r."RelatedCollection" #>> '{9999,Int}' AS integer)) = 8
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
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE 16 IN (
    SELECT COALESCE(sum(r1."Int"), 0)::int
    FROM (
        SELECT r0."Id" AS "Id0", r0."Int", r0."Name", r0."String", r0."String" AS "Key"
        FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS (
            "Id" integer,
            "Int" integer,
            "Name" text,
            "String" text
        )) WITH ORDINALITY AS r0
    ) AS r1
    GROUP BY r1."Key"
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
        FROM ROWS FROM (jsonb_to_recordset(r0."NestedCollection") AS (
            "Id" integer,
            "Int" integer,
            "Name" text,
            "String" text
        )) WITH ORDINALITY AS n)), 0)::int
    FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS ("NestedCollection" jsonb)) WITH ORDINALITY AS r0)
FROM "RootEntity" AS r
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
