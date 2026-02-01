// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonSetOperationsNpgsqlTest(ComplexJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonSetOperationsRelationalTestBase<ComplexJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Over_associate_collections()
    {
        await base.Over_associate_collections();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT 1
        FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS ("Int" integer)) WITH ORDINALITY AS a
        WHERE a."Int" = 8
        UNION ALL
        SELECT 1
        FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS ("String" text)) WITH ORDINALITY AS a0
        WHERE a0."String" = 'foo'
    ) AS u) = 4
""");
    }

    public override async Task Over_associate_collection_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Over_associate_collection_projected(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Over_assocate_collection_Select_nested_with_aggregates_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Over_assocate_collection_Select_nested_with_aggregates_projected(queryTrackingBehavior);

        AssertSql(
            """
SELECT (
    SELECT COALESCE(sum((
        SELECT COALESCE(sum(n."Int"), 0)::int
        FROM ROWS FROM (jsonb_to_recordset(u."NestedCollection") AS ("Int" integer)) WITH ORDINALITY AS n)), 0)::int
    FROM (
        SELECT a."NestedCollection" AS "NestedCollection"
        FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS (
            "Int" integer,
            "NestedCollection" jsonb
        )) WITH ORDINALITY AS a
        WHERE a."Int" = 8
        UNION ALL
        SELECT a0."NestedCollection" AS "NestedCollection"
        FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS (
            "String" text,
            "NestedCollection" jsonb
        )) WITH ORDINALITY AS a0
        WHERE a0."String" = 'foo'
    ) AS u)
FROM "RootEntity" AS r
""");
    }

    public override async Task Over_nested_associate_collection()
    {
        await base.Over_nested_associate_collection();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT 1
        FROM ROWS FROM (jsonb_to_recordset(r."RequiredAssociate" -> 'NestedCollection') AS ("Int" integer)) WITH ORDINALITY AS n
        WHERE n."Int" = 8
        UNION ALL
        SELECT 1
        FROM ROWS FROM (jsonb_to_recordset(r."RequiredAssociate" -> 'NestedCollection') AS ("String" text)) WITH ORDINALITY AS n0
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
