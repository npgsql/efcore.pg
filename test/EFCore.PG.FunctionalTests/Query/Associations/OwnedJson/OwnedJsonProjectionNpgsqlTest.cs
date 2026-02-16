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
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
""");
    }

    #region Scalar properties

    public override async Task Select_scalar_property_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_scalar_property_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."RequiredAssociate" ->> 'String'
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_property_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."OptionalAssociate" ->> 'String'
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_value_type_property_on_null_associate_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_associate_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT CAST(r."OptionalAssociate" ->> 'Int' AS integer)
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT CAST(r."OptionalAssociate" ->> 'Int' AS integer)
FROM "RootEntity" AS r
""");
    }

    #endregion Scalar properties

    #region Structural properties

    public override async Task Select_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."RequiredAssociate", r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."OptionalAssociate", r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_required_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."RequiredAssociate" -> 'RequiredNestedAssociate', r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_optional_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."RequiredAssociate" -> 'OptionalNestedAssociate', r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_required_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."OptionalAssociate" -> 'RequiredNestedAssociate', r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_optional_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."OptionalAssociate" -> 'OptionalNestedAssociate', r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_required_associate_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_via_optional_navigation(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r0."RequiredAssociate", r0."Id"
FROM "RootReferencingEntity" AS r
LEFT JOIN "RootEntity" AS r0 ON r."RootEntityId" = r0."Id"
""");
        }
    }

    public override async Task Select_unmapped_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_unmapped_associate_scalar_property(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."RequiredAssociate", r."Id"
FROM "RootEntity" AS r
""");
        }
    }

    public override async Task Select_untranslatable_method_on_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_untranslatable_method_on_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT CAST(r."RequiredAssociate" ->> 'Int' AS integer)
FROM "RootEntity" AS r
""");
    }

    #endregion Structural properties

    #region Collection

    public override async Task Select_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_collection(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."AssociateCollection", r."Id"
FROM "RootEntity" AS r
ORDER BY r."Id" NULLS FIRST
""");
        }
    }

    public override async Task Select_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."RequiredAssociate" -> 'NestedCollection', r."Id"
FROM "RootEntity" AS r
ORDER BY r."Id" NULLS FIRST
""");
        }
    }

    public override async Task Select_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."OptionalAssociate" -> 'NestedCollection', r."Id"
FROM "RootEntity" AS r
ORDER BY r."Id" NULLS FIRST
""");
        }
    }

    public override async Task SelectMany_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_associate_collection(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."Id", a."Id", a."Int", a."Ints", a."Name", a."String", a."NestedCollection", a."OptionalNestedAssociate", a."RequiredNestedAssociate"
FROM "RootEntity" AS r
JOIN LATERAL ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS (
    "Id" integer,
    "Int" integer,
    "Ints" jsonb,
    "Name" text,
    "String" text,
    "NestedCollection" jsonb,
    "OptionalNestedAssociate" jsonb,
    "RequiredNestedAssociate" jsonb
)) WITH ORDINALITY AS a ON TRUE
""");
        }
    }

    public override async Task SelectMany_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."Id", n."Id", n."Int", n."Ints", n."Name", n."String"
FROM "RootEntity" AS r
JOIN LATERAL ROWS FROM (jsonb_to_recordset(r."RequiredAssociate" -> 'NestedCollection') AS (
    "Id" integer,
    "Int" integer,
    "Ints" jsonb,
    "Name" text,
    "String" text
)) WITH ORDINALITY AS n ON TRUE
""");
        }
    }

    public override async Task SelectMany_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."Id", n."Id", n."Int", n."Ints", n."Name", n."String"
FROM "RootEntity" AS r
JOIN LATERAL ROWS FROM (jsonb_to_recordset(r."OptionalAssociate" -> 'NestedCollection') AS (
    "Id" integer,
    "Int" integer,
    "Ints" jsonb,
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
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_associate_and_target_to_index_based_binding_via_closure(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_and_target_to_index_based_binding_via_closure(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT r."Id", r."RequiredAssociate"
FROM "RootEntity" AS r
""");
        }
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
    SELECT r0."RequiredAssociate" -> 'RequiredNestedAssociate' AS c, r0."Id"
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
    SELECT r0."OptionalAssociate" -> 'RequiredNestedAssociate' AS c, r0."Id"
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
