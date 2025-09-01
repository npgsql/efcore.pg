// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonSetOperationsNpgsqlTest(ComplexJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonSetOperationsRelationalTestBase<ComplexJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task On_related()
    {
        await base.On_related();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT 1
        FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS ("Int" integer)) WITH ORDINALITY AS r0
        WHERE r0."Int" = 8
        UNION ALL
        SELECT 1
        FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS ("String" text)) WITH ORDINALITY AS r1
        WHERE r1."String" = 'foo'
    ) AS u) = 4
""");
    }

    public override async Task On_related_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.On_related_projected(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task On_related_Select_nested_with_aggregates(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.On_related_Select_nested_with_aggregates(queryTrackingBehavior);

        AssertSql(
            """
SELECT (
    SELECT COALESCE(sum((
        SELECT COALESCE(sum(n."Int"), 0)::int
        FROM ROWS FROM (jsonb_to_recordset(u."NestedCollection") AS ("Int" integer)) WITH ORDINALITY AS n)), 0)::int
    FROM (
        SELECT r0."NestedCollection" AS "NestedCollection"
        FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS (
            "Int" integer,
            "NestedCollection" jsonb
        )) WITH ORDINALITY AS r0
        WHERE r0."Int" = 8
        UNION ALL
        SELECT r1."NestedCollection" AS "NestedCollection"
        FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS (
            "String" text,
            "NestedCollection" jsonb
        )) WITH ORDINALITY AS r1
        WHERE r1."String" = 'foo'
    ) AS u)
FROM "RootEntity" AS r
""");
    }

    public override async Task On_nested()
    {
        await base.On_nested();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT 1
        FROM ROWS FROM (jsonb_to_recordset(r."RequiredRelated" -> 'NestedCollection') AS ("Int" integer)) WITH ORDINALITY AS n
        WHERE n."Int" = 8
        UNION ALL
        SELECT 1
        FROM ROWS FROM (jsonb_to_recordset(r."RequiredRelated" -> 'NestedCollection') AS ("String" text)) WITH ORDINALITY AS n0
        WHERE n0."String" = 'foo'
    ) AS u) = 4
""");
    }

    public override async Task Over_different_collection_properties()
    {
        await base.Over_different_collection_properties();

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
