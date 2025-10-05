// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public class OwnedJsonCollectionNpgsqlTest(OwnedJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedJsonCollectionRelationalTestBase<OwnedJsonNpgsqlFixture>(fixture, testOutputHelper)
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
    FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS (
        "Id" integer,
        "Int" integer,
        "Ints" jsonb,
        "Name" text,
        "String" text
    )) WITH ORDINALITY AS a
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
        "Int" integer,
        "Ints" jsonb,
        "Name" text,
        "String" text
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
        SELECT DISTINCT r."Id", a."Id" AS "Id0", a."Int", a."Ints", a."Name", a."String", a."NestedCollection" AS c, a."OptionalNestedAssociate" AS c0, a."RequiredNestedAssociate" AS c1
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

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."Id", a0."Id", a0."Id0", a0."Int", a0."Ints", a0."Name", a0."String", a0.c, a0.c0, a0.c1
FROM "RootEntity" AS r
LEFT JOIN LATERAL (
    SELECT DISTINCT r."Id", a."Id" AS "Id0", a."Int", a."Ints", a."Name", a."String", a."NestedCollection" AS c, a."OptionalNestedAssociate" AS c0, a."RequiredNestedAssociate" AS c1
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
) AS a0 ON TRUE
ORDER BY r."Id" NULLS FIRST, a0."Id0" NULLS FIRST, a0."Int" NULLS FIRST, a0."Ints" NULLS FIRST, a0."Name" NULLS FIRST
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
    SELECT COALESCE(sum(a0."Int"), 0)::int
    FROM (
        SELECT a."Id" AS "Id0", a."Int", a."Ints", a."Name", a."String", a."String" AS "Key"
        FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS (
            "Id" integer,
            "Int" integer,
            "Ints" jsonb,
            "Name" text,
            "String" text
        )) WITH ORDINALITY AS a
    ) AS a0
    GROUP BY a0."Key"
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
        FROM ROWS FROM (jsonb_to_recordset(a."NestedCollection") AS (
            "Id" integer,
            "Int" integer,
            "Ints" jsonb,
            "Name" text,
            "String" text
        )) WITH ORDINALITY AS n)), 0)::int
    FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS ("NestedCollection" jsonb)) WITH ORDINALITY AS a)
FROM "RootEntity" AS r
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
