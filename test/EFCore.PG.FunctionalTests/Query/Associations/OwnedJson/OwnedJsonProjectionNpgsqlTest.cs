// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public class OwnedJsonProjectionNpgsqlTest(OwnedJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedJsonProjectionRelationalTestBase<OwnedJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Select_root(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
""");
    }

    #region Simple properties

    public override async Task Select_property_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."RequiredRelated" ->> 'String'
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_property_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."OptionalRelated" ->> 'String'
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_value_type_property_on_null_related_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_related_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT CAST(r."OptionalRelated" ->> 'Int' AS integer)
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT CAST(r."OptionalRelated" ->> 'Int' AS integer)
FROM "RootEntity" AS r
""");
    }

    #endregion Simple properties

    #region Non-collection

    public override async Task Select_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."RequiredRelated", r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."OptionalRelated", r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_required_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."RequiredRelated" -> 'RequiredNested', r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_optional_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."RequiredRelated" -> 'OptionalNested', r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_required_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."OptionalRelated" -> 'RequiredNested', r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_optional_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."OptionalRelated" -> 'OptionalNested', r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_required_related_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_related_via_optional_navigation(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r0."RequiredRelated", r0."Id"
FROM "RootReferencingEntity" AS r
LEFT JOIN "RootEntity" AS r0 ON r."RootEntityId" = r0."Id"
""");
        }
    }

    #endregion Non-collection

    #region Collection

    public override async Task Select_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related_collection(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."RelatedCollection", r."Id"
FROM "RootEntity" AS r
ORDER BY r."Id" NULLS FIRST
""");
        }
    }

    public override async Task Select_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."RequiredRelated" -> 'NestedCollection', r."Id"
FROM "RootEntity" AS r
ORDER BY r."Id" NULLS FIRST
""");
        }
    }

    public override async Task Select_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."OptionalRelated" -> 'NestedCollection', r."Id"
FROM "RootEntity" AS r
ORDER BY r."Id" NULLS FIRST
""");
        }
    }

    public override async Task SelectMany_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_related_collection(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."Id", r0."Id", r0."Int", r0."Name", r0."String", r0."NestedCollection", r0."OptionalNested", r0."RequiredNested"
FROM "RootEntity" AS r
JOIN LATERAL ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS (
    "Id" integer,
    "Int" integer,
    "Name" text,
    "String" text,
    "NestedCollection" jsonb,
    "OptionalNested" jsonb,
    "RequiredNested" jsonb
)) WITH ORDINALITY AS r0 ON TRUE
""");
        }
    }

    public override async Task SelectMany_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."Id", n."Id", n."Int", n."Name", n."String"
FROM "RootEntity" AS r
JOIN LATERAL ROWS FROM (jsonb_to_recordset(r."RequiredRelated" -> 'NestedCollection') AS (
    "Id" integer,
    "Int" integer,
    "Name" text,
    "String" text
)) WITH ORDINALITY AS n ON TRUE
""");
        }
    }

    public override async Task SelectMany_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."Id", n."Id", n."Int", n."Name", n."String"
FROM "RootEntity" AS r
JOIN LATERAL ROWS FROM (jsonb_to_recordset(r."OptionalRelated" -> 'NestedCollection') AS (
    "Id" integer,
    "Int" integer,
    "Name" text,
    "String" text
)) WITH ORDINALITY AS n ON TRUE
""");
        }
    }

    #endregion Collection

    #region Multiple

    public override async Task Select_root_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
""");
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r1.c, r1."Id"
FROM "RootEntity" AS r
LEFT JOIN LATERAL (
    SELECT r0."RequiredRelated" -> 'RequiredNested' AS c, r0."Id"
    FROM "RootEntity" AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS r1 ON TRUE
""");
        }
    }

    public override async Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r1.c, r1."Id"
FROM "RootEntity" AS r
LEFT JOIN LATERAL (
    SELECT r0."OptionalRelated" -> 'RequiredNested' AS c, r0."Id"
    FROM "RootEntity" AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS r1 ON TRUE
""");
        }
    }

    #endregion Subquery

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
