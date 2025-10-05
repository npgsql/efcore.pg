// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonProjectionNpgsqlTest(ComplexJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonProjectionRelationalTestBase<ComplexJsonNpgsqlFixture>(fixture, testOutputHelper)
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

    #region Simple properties

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

    #endregion Simple properties

    #region Non-collection

    public override async Task Select_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."RequiredAssociate"
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."OptionalAssociate"
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_required_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."RequiredAssociate" -> 'RequiredNestedAssociate'
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_optional_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."RequiredAssociate" -> 'OptionalNestedAssociate'
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_required_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."OptionalAssociate" -> 'RequiredNestedAssociate'
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_optional_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."OptionalAssociate" -> 'OptionalNestedAssociate'
FROM "RootEntity" AS r
""");
    }

    public override async Task Select_required_associate_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_via_optional_navigation(queryTrackingBehavior);

        AssertSql(
            """
SELECT r0."RequiredAssociate"
FROM "RootReferencingEntity" AS r
LEFT JOIN "RootEntity" AS r0 ON r."RootEntityId" = r0."Id"
""");
    }

    public override async Task Select_unmapped_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_unmapped_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
""");
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

    #endregion Non-collection

    #region Collection

    public override async Task Select_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."AssociateCollection"
FROM "RootEntity" AS r
ORDER BY r."Id" NULLS FIRST
""");
    }

    public override async Task Select_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."RequiredAssociate" -> 'NestedCollection'
FROM "RootEntity" AS r
ORDER BY r."Id" NULLS FIRST
""");
    }

    public override async Task Select_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT r."OptionalAssociate" -> 'NestedCollection'
FROM "RootEntity" AS r
ORDER BY r."Id" NULLS FIRST
""");
    }

    public override async Task SelectMany_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_associate_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT a."Id", a."Int", a."Ints", a."Name", a."String", a."NestedCollection", a."OptionalNestedAssociate", a."RequiredNestedAssociate"
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

    public override async Task SelectMany_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT n."Id", n."Int", n."Ints", n."Name", n."String"
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

    public override async Task SelectMany_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT n."Id", n."Int", n."Ints", n."Name", n."String"
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

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior);

        AssertSql(
            """
SELECT r1.c
FROM "RootEntity" AS r
LEFT JOIN LATERAL (
    SELECT r0."RequiredAssociate" -> 'RequiredNestedAssociate' AS c
    FROM "RootEntity" AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS r1 ON TRUE
""");
    }

    public override async Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior);

        AssertSql(
            """
SELECT r1.c
FROM "RootEntity" AS r
LEFT JOIN LATERAL (
    SELECT r0."OptionalAssociate" -> 'RequiredNestedAssociate' AS c
    FROM "RootEntity" AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS r1 ON TRUE
""");
    }

    #endregion Subquery

    #region Value types

    public override async Task Select_root_with_value_types(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_with_value_types(queryTrackingBehavior);

        AssertSql(
            """
SELECT v."Id", v."Name", v."AssociateCollection", v."OptionalAssociate", v."RequiredAssociate"
FROM "ValueRootEntity" AS v
""");
    }

    public override async Task Select_non_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_non_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT v."RequiredAssociate"
FROM "ValueRootEntity" AS v
ORDER BY v."Id" NULLS FIRST
""");
    }

    public override async Task Select_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT v."OptionalAssociate"
FROM "ValueRootEntity" AS v
ORDER BY v."Id" NULLS FIRST
""");
    }

    public override async Task Select_nullable_value_type_with_Value(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_with_Value(queryTrackingBehavior);

        AssertSql(
            """
SELECT v."OptionalAssociate"
FROM "ValueRootEntity" AS v
ORDER BY v."Id" NULLS FIRST
""");
    }

    #endregion Value types

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
