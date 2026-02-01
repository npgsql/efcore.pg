namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting;

public class OwnedTableSplittingPrimitiveCollectionNpgsqlTest(OwnedTableSplittingNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedTableSplittingPrimitiveCollectionRelationalTestBase<OwnedTableSplittingNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT r."Id", r."Name", s."RootEntityId", s."Id", s."Int", s."Ints", s."Name", s."String", s."AssociateTypeRootEntityId", s."AssociateTypeId", s."Id0", s."Int0", s."Ints0", s."Name0", s."String0", s."OptionalNestedAssociate_Id", s."OptionalNestedAssociate_Int", s."OptionalNestedAssociate_Ints", s."OptionalNestedAssociate_Name", s."OptionalNestedAssociate_String", s."RequiredNestedAssociate_Id", s."RequiredNestedAssociate_Int", s."RequiredNestedAssociate_Ints", s."RequiredNestedAssociate_Name", s."RequiredNestedAssociate_String", r."OptionalAssociate_Id", r."OptionalAssociate_Int", r."OptionalAssociate_Ints", r."OptionalAssociate_Name", r."OptionalAssociate_String", o."AssociateTypeRootEntityId", o."Id", o."Int", o."Ints", o."Name", o."String", r."OptionalAssociate_OptionalNestedAssociate_Id", r."OptionalAssociate_OptionalNestedAssociate_Int", r."OptionalAssociate_OptionalNestedAssociate_Ints", r."OptionalAssociate_OptionalNestedAssociate_Name", r."OptionalAssociate_OptionalNestedAssociate_String", r."OptionalAssociate_RequiredNestedAssociate_Id", r."OptionalAssociate_RequiredNestedAssociate_Int", r."OptionalAssociate_RequiredNestedAssociate_Ints", r."OptionalAssociate_RequiredNestedAssociate_Name", r."OptionalAssociate_RequiredNestedAssociate_String", r."RequiredAssociate_Id", r."RequiredAssociate_Int", r."RequiredAssociate_Ints", r."RequiredAssociate_Name", r."RequiredAssociate_String", r2."AssociateTypeRootEntityId", r2."Id", r2."Int", r2."Ints", r2."Name", r2."String", r."RequiredAssociate_OptionalNestedAssociate_Id", r."RequiredAssociate_OptionalNestedAssociate_Int", r."RequiredAssociate_OptionalNestedAssociate_Ints", r."RequiredAssociate_OptionalNestedAssociate_Name", r."RequiredAssociate_OptionalNestedAssociate_String", r."RequiredAssociate_RequiredNestedAssociate_Id", r."RequiredAssociate_RequiredNestedAssociate_Int", r."RequiredAssociate_RequiredNestedAssociate_Ints", r."RequiredAssociate_RequiredNestedAssociate_Name", r."RequiredAssociate_RequiredNestedAssociate_String"
FROM "RootEntity" AS r
LEFT JOIN (
    SELECT r0."RootEntityId", r0."Id", r0."Int", r0."Ints", r0."Name", r0."String", r1."AssociateTypeRootEntityId", r1."AssociateTypeId", r1."Id" AS "Id0", r1."Int" AS "Int0", r1."Ints" AS "Ints0", r1."Name" AS "Name0", r1."String" AS "String0", r0."OptionalNestedAssociate_Id", r0."OptionalNestedAssociate_Int", r0."OptionalNestedAssociate_Ints", r0."OptionalNestedAssociate_Name", r0."OptionalNestedAssociate_String", r0."RequiredNestedAssociate_Id", r0."RequiredNestedAssociate_Int", r0."RequiredNestedAssociate_Ints", r0."RequiredNestedAssociate_Name", r0."RequiredNestedAssociate_String"
    FROM "RelatedCollection" AS r0
    LEFT JOIN "RelatedCollection_NestedCollection" AS r1 ON r0."RootEntityId" = r1."AssociateTypeRootEntityId" AND r0."Id" = r1."AssociateTypeId"
) AS s ON r."Id" = s."RootEntityId"
LEFT JOIN "OptionalRelated_NestedCollection" AS o ON CASE
    WHEN r."OptionalAssociate_Id" IS NOT NULL AND r."OptionalAssociate_Int" IS NOT NULL AND r."OptionalAssociate_Ints" IS NOT NULL AND r."OptionalAssociate_Name" IS NOT NULL AND r."OptionalAssociate_String" IS NOT NULL THEN r."Id"
END = o."AssociateTypeRootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS r2 ON r."Id" = r2."AssociateTypeRootEntityId"
WHERE cardinality(r."RequiredAssociate_Ints") = 3
ORDER BY r."Id" NULLS FIRST, s."RootEntityId" NULLS FIRST, s."Id" NULLS FIRST, s."AssociateTypeRootEntityId" NULLS FIRST, s."AssociateTypeId" NULLS FIRST, s."Id0" NULLS FIRST, o."AssociateTypeRootEntityId" NULLS FIRST, o."Id" NULLS FIRST, r2."AssociateTypeRootEntityId" NULLS FIRST
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT r."Id", r."Name", s."RootEntityId", s."Id", s."Int", s."Ints", s."Name", s."String", s."AssociateTypeRootEntityId", s."AssociateTypeId", s."Id0", s."Int0", s."Ints0", s."Name0", s."String0", s."OptionalNestedAssociate_Id", s."OptionalNestedAssociate_Int", s."OptionalNestedAssociate_Ints", s."OptionalNestedAssociate_Name", s."OptionalNestedAssociate_String", s."RequiredNestedAssociate_Id", s."RequiredNestedAssociate_Int", s."RequiredNestedAssociate_Ints", s."RequiredNestedAssociate_Name", s."RequiredNestedAssociate_String", r."OptionalAssociate_Id", r."OptionalAssociate_Int", r."OptionalAssociate_Ints", r."OptionalAssociate_Name", r."OptionalAssociate_String", o."AssociateTypeRootEntityId", o."Id", o."Int", o."Ints", o."Name", o."String", r."OptionalAssociate_OptionalNestedAssociate_Id", r."OptionalAssociate_OptionalNestedAssociate_Int", r."OptionalAssociate_OptionalNestedAssociate_Ints", r."OptionalAssociate_OptionalNestedAssociate_Name", r."OptionalAssociate_OptionalNestedAssociate_String", r."OptionalAssociate_RequiredNestedAssociate_Id", r."OptionalAssociate_RequiredNestedAssociate_Int", r."OptionalAssociate_RequiredNestedAssociate_Ints", r."OptionalAssociate_RequiredNestedAssociate_Name", r."OptionalAssociate_RequiredNestedAssociate_String", r."RequiredAssociate_Id", r."RequiredAssociate_Int", r."RequiredAssociate_Ints", r."RequiredAssociate_Name", r."RequiredAssociate_String", r2."AssociateTypeRootEntityId", r2."Id", r2."Int", r2."Ints", r2."Name", r2."String", r."RequiredAssociate_OptionalNestedAssociate_Id", r."RequiredAssociate_OptionalNestedAssociate_Int", r."RequiredAssociate_OptionalNestedAssociate_Ints", r."RequiredAssociate_OptionalNestedAssociate_Name", r."RequiredAssociate_OptionalNestedAssociate_String", r."RequiredAssociate_RequiredNestedAssociate_Id", r."RequiredAssociate_RequiredNestedAssociate_Int", r."RequiredAssociate_RequiredNestedAssociate_Ints", r."RequiredAssociate_RequiredNestedAssociate_Name", r."RequiredAssociate_RequiredNestedAssociate_String"
FROM "RootEntity" AS r
LEFT JOIN (
    SELECT r0."RootEntityId", r0."Id", r0."Int", r0."Ints", r0."Name", r0."String", r1."AssociateTypeRootEntityId", r1."AssociateTypeId", r1."Id" AS "Id0", r1."Int" AS "Int0", r1."Ints" AS "Ints0", r1."Name" AS "Name0", r1."String" AS "String0", r0."OptionalNestedAssociate_Id", r0."OptionalNestedAssociate_Int", r0."OptionalNestedAssociate_Ints", r0."OptionalNestedAssociate_Name", r0."OptionalNestedAssociate_String", r0."RequiredNestedAssociate_Id", r0."RequiredNestedAssociate_Int", r0."RequiredNestedAssociate_Ints", r0."RequiredNestedAssociate_Name", r0."RequiredNestedAssociate_String"
    FROM "RelatedCollection" AS r0
    LEFT JOIN "RelatedCollection_NestedCollection" AS r1 ON r0."RootEntityId" = r1."AssociateTypeRootEntityId" AND r0."Id" = r1."AssociateTypeId"
) AS s ON r."Id" = s."RootEntityId"
LEFT JOIN "OptionalRelated_NestedCollection" AS o ON CASE
    WHEN r."OptionalAssociate_Id" IS NOT NULL AND r."OptionalAssociate_Int" IS NOT NULL AND r."OptionalAssociate_Ints" IS NOT NULL AND r."OptionalAssociate_Name" IS NOT NULL AND r."OptionalAssociate_String" IS NOT NULL THEN r."Id"
END = o."AssociateTypeRootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS r2 ON r."Id" = r2."AssociateTypeRootEntityId"
WHERE r."RequiredAssociate_Ints"[1] = 1
ORDER BY r."Id" NULLS FIRST, s."RootEntityId" NULLS FIRST, s."Id" NULLS FIRST, s."AssociateTypeRootEntityId" NULLS FIRST, s."AssociateTypeId" NULLS FIRST, s."Id0" NULLS FIRST, o."AssociateTypeRootEntityId" NULLS FIRST, o."Id" NULLS FIRST, r2."AssociateTypeRootEntityId" NULLS FIRST
""");
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT r."Id", r."Name", s."RootEntityId", s."Id", s."Int", s."Ints", s."Name", s."String", s."AssociateTypeRootEntityId", s."AssociateTypeId", s."Id0", s."Int0", s."Ints0", s."Name0", s."String0", s."OptionalNestedAssociate_Id", s."OptionalNestedAssociate_Int", s."OptionalNestedAssociate_Ints", s."OptionalNestedAssociate_Name", s."OptionalNestedAssociate_String", s."RequiredNestedAssociate_Id", s."RequiredNestedAssociate_Int", s."RequiredNestedAssociate_Ints", s."RequiredNestedAssociate_Name", s."RequiredNestedAssociate_String", r."OptionalAssociate_Id", r."OptionalAssociate_Int", r."OptionalAssociate_Ints", r."OptionalAssociate_Name", r."OptionalAssociate_String", o."AssociateTypeRootEntityId", o."Id", o."Int", o."Ints", o."Name", o."String", r."OptionalAssociate_OptionalNestedAssociate_Id", r."OptionalAssociate_OptionalNestedAssociate_Int", r."OptionalAssociate_OptionalNestedAssociate_Ints", r."OptionalAssociate_OptionalNestedAssociate_Name", r."OptionalAssociate_OptionalNestedAssociate_String", r."OptionalAssociate_RequiredNestedAssociate_Id", r."OptionalAssociate_RequiredNestedAssociate_Int", r."OptionalAssociate_RequiredNestedAssociate_Ints", r."OptionalAssociate_RequiredNestedAssociate_Name", r."OptionalAssociate_RequiredNestedAssociate_String", r."RequiredAssociate_Id", r."RequiredAssociate_Int", r."RequiredAssociate_Ints", r."RequiredAssociate_Name", r."RequiredAssociate_String", r2."AssociateTypeRootEntityId", r2."Id", r2."Int", r2."Ints", r2."Name", r2."String", r."RequiredAssociate_OptionalNestedAssociate_Id", r."RequiredAssociate_OptionalNestedAssociate_Int", r."RequiredAssociate_OptionalNestedAssociate_Ints", r."RequiredAssociate_OptionalNestedAssociate_Name", r."RequiredAssociate_OptionalNestedAssociate_String", r."RequiredAssociate_RequiredNestedAssociate_Id", r."RequiredAssociate_RequiredNestedAssociate_Int", r."RequiredAssociate_RequiredNestedAssociate_Ints", r."RequiredAssociate_RequiredNestedAssociate_Name", r."RequiredAssociate_RequiredNestedAssociate_String"
FROM "RootEntity" AS r
LEFT JOIN (
    SELECT r0."RootEntityId", r0."Id", r0."Int", r0."Ints", r0."Name", r0."String", r1."AssociateTypeRootEntityId", r1."AssociateTypeId", r1."Id" AS "Id0", r1."Int" AS "Int0", r1."Ints" AS "Ints0", r1."Name" AS "Name0", r1."String" AS "String0", r0."OptionalNestedAssociate_Id", r0."OptionalNestedAssociate_Int", r0."OptionalNestedAssociate_Ints", r0."OptionalNestedAssociate_Name", r0."OptionalNestedAssociate_String", r0."RequiredNestedAssociate_Id", r0."RequiredNestedAssociate_Int", r0."RequiredNestedAssociate_Ints", r0."RequiredNestedAssociate_Name", r0."RequiredNestedAssociate_String"
    FROM "RelatedCollection" AS r0
    LEFT JOIN "RelatedCollection_NestedCollection" AS r1 ON r0."RootEntityId" = r1."AssociateTypeRootEntityId" AND r0."Id" = r1."AssociateTypeId"
) AS s ON r."Id" = s."RootEntityId"
LEFT JOIN "OptionalRelated_NestedCollection" AS o ON CASE
    WHEN r."OptionalAssociate_Id" IS NOT NULL AND r."OptionalAssociate_Int" IS NOT NULL AND r."OptionalAssociate_Ints" IS NOT NULL AND r."OptionalAssociate_Name" IS NOT NULL AND r."OptionalAssociate_String" IS NOT NULL THEN r."Id"
END = o."AssociateTypeRootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS r2 ON r."Id" = r2."AssociateTypeRootEntityId"
WHERE 3 = ANY (r."RequiredAssociate_Ints")
ORDER BY r."Id" NULLS FIRST, s."RootEntityId" NULLS FIRST, s."Id" NULLS FIRST, s."AssociateTypeRootEntityId" NULLS FIRST, s."AssociateTypeId" NULLS FIRST, s."Id0" NULLS FIRST, o."AssociateTypeRootEntityId" NULLS FIRST, o."Id" NULLS FIRST, r2."AssociateTypeRootEntityId" NULLS FIRST
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT r."Id", r."Name", s."RootEntityId", s."Id", s."Int", s."Ints", s."Name", s."String", s."AssociateTypeRootEntityId", s."AssociateTypeId", s."Id0", s."Int0", s."Ints0", s."Name0", s."String0", s."OptionalNestedAssociate_Id", s."OptionalNestedAssociate_Int", s."OptionalNestedAssociate_Ints", s."OptionalNestedAssociate_Name", s."OptionalNestedAssociate_String", s."RequiredNestedAssociate_Id", s."RequiredNestedAssociate_Int", s."RequiredNestedAssociate_Ints", s."RequiredNestedAssociate_Name", s."RequiredNestedAssociate_String", r."OptionalAssociate_Id", r."OptionalAssociate_Int", r."OptionalAssociate_Ints", r."OptionalAssociate_Name", r."OptionalAssociate_String", o."AssociateTypeRootEntityId", o."Id", o."Int", o."Ints", o."Name", o."String", r."OptionalAssociate_OptionalNestedAssociate_Id", r."OptionalAssociate_OptionalNestedAssociate_Int", r."OptionalAssociate_OptionalNestedAssociate_Ints", r."OptionalAssociate_OptionalNestedAssociate_Name", r."OptionalAssociate_OptionalNestedAssociate_String", r."OptionalAssociate_RequiredNestedAssociate_Id", r."OptionalAssociate_RequiredNestedAssociate_Int", r."OptionalAssociate_RequiredNestedAssociate_Ints", r."OptionalAssociate_RequiredNestedAssociate_Name", r."OptionalAssociate_RequiredNestedAssociate_String", r."RequiredAssociate_Id", r."RequiredAssociate_Int", r."RequiredAssociate_Ints", r."RequiredAssociate_Name", r."RequiredAssociate_String", r2."AssociateTypeRootEntityId", r2."Id", r2."Int", r2."Ints", r2."Name", r2."String", r."RequiredAssociate_OptionalNestedAssociate_Id", r."RequiredAssociate_OptionalNestedAssociate_Int", r."RequiredAssociate_OptionalNestedAssociate_Ints", r."RequiredAssociate_OptionalNestedAssociate_Name", r."RequiredAssociate_OptionalNestedAssociate_String", r."RequiredAssociate_RequiredNestedAssociate_Id", r."RequiredAssociate_RequiredNestedAssociate_Int", r."RequiredAssociate_RequiredNestedAssociate_Ints", r."RequiredAssociate_RequiredNestedAssociate_Name", r."RequiredAssociate_RequiredNestedAssociate_String"
FROM "RootEntity" AS r
LEFT JOIN (
    SELECT r0."RootEntityId", r0."Id", r0."Int", r0."Ints", r0."Name", r0."String", r1."AssociateTypeRootEntityId", r1."AssociateTypeId", r1."Id" AS "Id0", r1."Int" AS "Int0", r1."Ints" AS "Ints0", r1."Name" AS "Name0", r1."String" AS "String0", r0."OptionalNestedAssociate_Id", r0."OptionalNestedAssociate_Int", r0."OptionalNestedAssociate_Ints", r0."OptionalNestedAssociate_Name", r0."OptionalNestedAssociate_String", r0."RequiredNestedAssociate_Id", r0."RequiredNestedAssociate_Int", r0."RequiredNestedAssociate_Ints", r0."RequiredNestedAssociate_Name", r0."RequiredNestedAssociate_String"
    FROM "RelatedCollection" AS r0
    LEFT JOIN "RelatedCollection_NestedCollection" AS r1 ON r0."RootEntityId" = r1."AssociateTypeRootEntityId" AND r0."Id" = r1."AssociateTypeId"
) AS s ON r."Id" = s."RootEntityId"
LEFT JOIN "OptionalRelated_NestedCollection" AS o ON CASE
    WHEN r."OptionalAssociate_Id" IS NOT NULL AND r."OptionalAssociate_Int" IS NOT NULL AND r."OptionalAssociate_Ints" IS NOT NULL AND r."OptionalAssociate_Name" IS NOT NULL AND r."OptionalAssociate_String" IS NOT NULL THEN r."Id"
END = o."AssociateTypeRootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS r2 ON r."Id" = r2."AssociateTypeRootEntityId"
WHERE 2 = ANY (r."RequiredAssociate_Ints")
ORDER BY r."Id" NULLS FIRST, s."RootEntityId" NULLS FIRST, s."Id" NULLS FIRST, s."AssociateTypeRootEntityId" NULLS FIRST, s."AssociateTypeId" NULLS FIRST, s."Id0" NULLS FIRST, o."AssociateTypeRootEntityId" NULLS FIRST, o."Id" NULLS FIRST, r2."AssociateTypeRootEntityId" NULLS FIRST
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT r."Id", r."Name", s."RootEntityId", s."Id", s."Int", s."Ints", s."Name", s."String", s."AssociateTypeRootEntityId", s."AssociateTypeId", s."Id0", s."Int0", s."Ints0", s."Name0", s."String0", s."OptionalNestedAssociate_Id", s."OptionalNestedAssociate_Int", s."OptionalNestedAssociate_Ints", s."OptionalNestedAssociate_Name", s."OptionalNestedAssociate_String", s."RequiredNestedAssociate_Id", s."RequiredNestedAssociate_Int", s."RequiredNestedAssociate_Ints", s."RequiredNestedAssociate_Name", s."RequiredNestedAssociate_String", r."OptionalAssociate_Id", r."OptionalAssociate_Int", r."OptionalAssociate_Ints", r."OptionalAssociate_Name", r."OptionalAssociate_String", o."AssociateTypeRootEntityId", o."Id", o."Int", o."Ints", o."Name", o."String", r."OptionalAssociate_OptionalNestedAssociate_Id", r."OptionalAssociate_OptionalNestedAssociate_Int", r."OptionalAssociate_OptionalNestedAssociate_Ints", r."OptionalAssociate_OptionalNestedAssociate_Name", r."OptionalAssociate_OptionalNestedAssociate_String", r."OptionalAssociate_RequiredNestedAssociate_Id", r."OptionalAssociate_RequiredNestedAssociate_Int", r."OptionalAssociate_RequiredNestedAssociate_Ints", r."OptionalAssociate_RequiredNestedAssociate_Name", r."OptionalAssociate_RequiredNestedAssociate_String", r."RequiredAssociate_Id", r."RequiredAssociate_Int", r."RequiredAssociate_Ints", r."RequiredAssociate_Name", r."RequiredAssociate_String", r2."AssociateTypeRootEntityId", r2."Id", r2."Int", r2."Ints", r2."Name", r2."String", r."RequiredAssociate_OptionalNestedAssociate_Id", r."RequiredAssociate_OptionalNestedAssociate_Int", r."RequiredAssociate_OptionalNestedAssociate_Ints", r."RequiredAssociate_OptionalNestedAssociate_Name", r."RequiredAssociate_OptionalNestedAssociate_String", r."RequiredAssociate_RequiredNestedAssociate_Id", r."RequiredAssociate_RequiredNestedAssociate_Int", r."RequiredAssociate_RequiredNestedAssociate_Ints", r."RequiredAssociate_RequiredNestedAssociate_Name", r."RequiredAssociate_RequiredNestedAssociate_String"
FROM "RootEntity" AS r
LEFT JOIN (
    SELECT r0."RootEntityId", r0."Id", r0."Int", r0."Ints", r0."Name", r0."String", r1."AssociateTypeRootEntityId", r1."AssociateTypeId", r1."Id" AS "Id0", r1."Int" AS "Int0", r1."Ints" AS "Ints0", r1."Name" AS "Name0", r1."String" AS "String0", r0."OptionalNestedAssociate_Id", r0."OptionalNestedAssociate_Int", r0."OptionalNestedAssociate_Ints", r0."OptionalNestedAssociate_Name", r0."OptionalNestedAssociate_String", r0."RequiredNestedAssociate_Id", r0."RequiredNestedAssociate_Int", r0."RequiredNestedAssociate_Ints", r0."RequiredNestedAssociate_Name", r0."RequiredNestedAssociate_String"
    FROM "RelatedCollection" AS r0
    LEFT JOIN "RelatedCollection_NestedCollection" AS r1 ON r0."RootEntityId" = r1."AssociateTypeRootEntityId" AND r0."Id" = r1."AssociateTypeId"
) AS s ON r."Id" = s."RootEntityId"
LEFT JOIN "OptionalRelated_NestedCollection" AS o ON CASE
    WHEN r."OptionalAssociate_Id" IS NOT NULL AND r."OptionalAssociate_Int" IS NOT NULL AND r."OptionalAssociate_Ints" IS NOT NULL AND r."OptionalAssociate_Name" IS NOT NULL AND r."OptionalAssociate_String" IS NOT NULL THEN r."Id"
END = o."AssociateTypeRootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS r2 ON r."Id" = r2."AssociateTypeRootEntityId"
WHERE cardinality(r."RequiredAssociate_RequiredNestedAssociate_Ints") = 3
ORDER BY r."Id" NULLS FIRST, s."RootEntityId" NULLS FIRST, s."Id" NULLS FIRST, s."AssociateTypeRootEntityId" NULLS FIRST, s."AssociateTypeId" NULLS FIRST, s."Id0" NULLS FIRST, o."AssociateTypeRootEntityId" NULLS FIRST, o."Id" NULLS FIRST, r2."AssociateTypeRootEntityId" NULLS FIRST
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(sum(r1.value), 0)::int
    FROM unnest(r."RequiredAssociate_Ints") AS r1(value))
FROM "RootEntity" AS r
WHERE (
    SELECT COALESCE(sum(r0.value), 0)::int
    FROM unnest(r."RequiredAssociate_Ints") AS r0(value)) >= 6
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
