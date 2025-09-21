namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsPrimitiveCollectionNpgsqlTest(OwnedNavigationsNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedNavigationsPrimitiveCollectionRelationalTestBase<OwnedNavigationsNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT r."Id", r."Name", o."RootEntityId", o."Id", o."Int", o."Ints", o."Name", o."String", r0."RootEntityId", o0."RelatedTypeRootEntityId", o1."RelatedTypeRootEntityId", r1."RelatedTypeRootEntityId", r2."RelatedTypeRootEntityId", o2."RelatedTypeRootEntityId", o2."Id", o2."Int", o2."Ints", o2."Name", o2."String", o0."Id", o0."Int", o0."Ints", o0."Name", o0."String", o1."Id", o1."Int", o1."Ints", o1."Name", o1."String", s."RootEntityId", s."Id", s."Int", s."Ints", s."Name", s."String", s."RelatedTypeRootEntityId", s."RelatedTypeId", s."RelatedTypeRootEntityId0", s."RelatedTypeId0", s."RelatedTypeRootEntityId1", s."RelatedTypeId1", s."Id0", s."Int0", s."Ints0", s."Name0", s."String0", s."Id1", s."Int1", s."Ints1", s."Name1", s."String1", s."Id2", s."Int2", s."Ints2", s."Name2", s."String2", r0."Id", r0."Int", r0."Ints", r0."Name", r0."String", r7."RelatedTypeRootEntityId", r7."Id", r7."Int", r7."Ints", r7."Name", r7."String", r1."Id", r1."Int", r1."Ints", r1."Name", r1."String", r2."Id", r2."Int", r2."Ints", r2."Name", r2."String"
FROM "RootEntity" AS r
LEFT JOIN "RequiredRelated" AS r0 ON r."Id" = r0."RootEntityId"
LEFT JOIN "OptionalRelated" AS o ON r."Id" = o."RootEntityId"
LEFT JOIN "OptionalRelated_OptionalNested" AS o0 ON o."RootEntityId" = o0."RelatedTypeRootEntityId"
LEFT JOIN "OptionalRelated_RequiredNested" AS o1 ON o."RootEntityId" = o1."RelatedTypeRootEntityId"
LEFT JOIN "RequiredRelated_OptionalNested" AS r1 ON r0."RootEntityId" = r1."RelatedTypeRootEntityId"
LEFT JOIN "RequiredRelated_RequiredNested" AS r2 ON r0."RootEntityId" = r2."RelatedTypeRootEntityId"
LEFT JOIN "OptionalRelated_NestedCollection" AS o2 ON o."RootEntityId" = o2."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT r3."RootEntityId", r3."Id", r3."Int", r3."Ints", r3."Name", r3."String", r4."RelatedTypeRootEntityId", r4."RelatedTypeId", r5."RelatedTypeRootEntityId" AS "RelatedTypeRootEntityId0", r5."RelatedTypeId" AS "RelatedTypeId0", r6."RelatedTypeRootEntityId" AS "RelatedTypeRootEntityId1", r6."RelatedTypeId" AS "RelatedTypeId1", r6."Id" AS "Id0", r6."Int" AS "Int0", r6."Ints" AS "Ints0", r6."Name" AS "Name0", r6."String" AS "String0", r4."Id" AS "Id1", r4."Int" AS "Int1", r4."Ints" AS "Ints1", r4."Name" AS "Name1", r4."String" AS "String1", r5."Id" AS "Id2", r5."Int" AS "Int2", r5."Ints" AS "Ints2", r5."Name" AS "Name2", r5."String" AS "String2"
    FROM "RelatedCollection" AS r3
    LEFT JOIN "RelatedCollection_OptionalNested" AS r4 ON r3."RootEntityId" = r4."RelatedTypeRootEntityId" AND r3."Id" = r4."RelatedTypeId"
    LEFT JOIN "RelatedCollection_RequiredNested" AS r5 ON r3."RootEntityId" = r5."RelatedTypeRootEntityId" AND r3."Id" = r5."RelatedTypeId"
    LEFT JOIN "RelatedCollection_NestedCollection" AS r6 ON r3."RootEntityId" = r6."RelatedTypeRootEntityId" AND r3."Id" = r6."RelatedTypeId"
) AS s ON r."Id" = s."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS r7 ON r0."RootEntityId" = r7."RelatedTypeRootEntityId"
WHERE cardinality(r0."Ints") = 3
ORDER BY r."Id" NULLS FIRST, r0."RootEntityId" NULLS FIRST, o."RootEntityId" NULLS FIRST, o0."RelatedTypeRootEntityId" NULLS FIRST, o1."RelatedTypeRootEntityId" NULLS FIRST, r1."RelatedTypeRootEntityId" NULLS FIRST, r2."RelatedTypeRootEntityId" NULLS FIRST, o2."RelatedTypeRootEntityId" NULLS FIRST, o2."Id" NULLS FIRST, s."RootEntityId" NULLS FIRST, s."Id" NULLS FIRST, s."RelatedTypeRootEntityId" NULLS FIRST, s."RelatedTypeId" NULLS FIRST, s."RelatedTypeRootEntityId0" NULLS FIRST, s."RelatedTypeId0" NULLS FIRST, s."RelatedTypeRootEntityId1" NULLS FIRST, s."RelatedTypeId1" NULLS FIRST, s."Id0" NULLS FIRST, r7."RelatedTypeRootEntityId" NULLS FIRST
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT r."Id", r."Name", o."RootEntityId", o."Id", o."Int", o."Ints", o."Name", o."String", r0."RootEntityId", o0."RelatedTypeRootEntityId", o1."RelatedTypeRootEntityId", r1."RelatedTypeRootEntityId", r2."RelatedTypeRootEntityId", o2."RelatedTypeRootEntityId", o2."Id", o2."Int", o2."Ints", o2."Name", o2."String", o0."Id", o0."Int", o0."Ints", o0."Name", o0."String", o1."Id", o1."Int", o1."Ints", o1."Name", o1."String", s."RootEntityId", s."Id", s."Int", s."Ints", s."Name", s."String", s."RelatedTypeRootEntityId", s."RelatedTypeId", s."RelatedTypeRootEntityId0", s."RelatedTypeId0", s."RelatedTypeRootEntityId1", s."RelatedTypeId1", s."Id0", s."Int0", s."Ints0", s."Name0", s."String0", s."Id1", s."Int1", s."Ints1", s."Name1", s."String1", s."Id2", s."Int2", s."Ints2", s."Name2", s."String2", r0."Id", r0."Int", r0."Ints", r0."Name", r0."String", r7."RelatedTypeRootEntityId", r7."Id", r7."Int", r7."Ints", r7."Name", r7."String", r1."Id", r1."Int", r1."Ints", r1."Name", r1."String", r2."Id", r2."Int", r2."Ints", r2."Name", r2."String"
FROM "RootEntity" AS r
LEFT JOIN "RequiredRelated" AS r0 ON r."Id" = r0."RootEntityId"
LEFT JOIN "OptionalRelated" AS o ON r."Id" = o."RootEntityId"
LEFT JOIN "OptionalRelated_OptionalNested" AS o0 ON o."RootEntityId" = o0."RelatedTypeRootEntityId"
LEFT JOIN "OptionalRelated_RequiredNested" AS o1 ON o."RootEntityId" = o1."RelatedTypeRootEntityId"
LEFT JOIN "RequiredRelated_OptionalNested" AS r1 ON r0."RootEntityId" = r1."RelatedTypeRootEntityId"
LEFT JOIN "RequiredRelated_RequiredNested" AS r2 ON r0."RootEntityId" = r2."RelatedTypeRootEntityId"
LEFT JOIN "OptionalRelated_NestedCollection" AS o2 ON o."RootEntityId" = o2."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT r3."RootEntityId", r3."Id", r3."Int", r3."Ints", r3."Name", r3."String", r4."RelatedTypeRootEntityId", r4."RelatedTypeId", r5."RelatedTypeRootEntityId" AS "RelatedTypeRootEntityId0", r5."RelatedTypeId" AS "RelatedTypeId0", r6."RelatedTypeRootEntityId" AS "RelatedTypeRootEntityId1", r6."RelatedTypeId" AS "RelatedTypeId1", r6."Id" AS "Id0", r6."Int" AS "Int0", r6."Ints" AS "Ints0", r6."Name" AS "Name0", r6."String" AS "String0", r4."Id" AS "Id1", r4."Int" AS "Int1", r4."Ints" AS "Ints1", r4."Name" AS "Name1", r4."String" AS "String1", r5."Id" AS "Id2", r5."Int" AS "Int2", r5."Ints" AS "Ints2", r5."Name" AS "Name2", r5."String" AS "String2"
    FROM "RelatedCollection" AS r3
    LEFT JOIN "RelatedCollection_OptionalNested" AS r4 ON r3."RootEntityId" = r4."RelatedTypeRootEntityId" AND r3."Id" = r4."RelatedTypeId"
    LEFT JOIN "RelatedCollection_RequiredNested" AS r5 ON r3."RootEntityId" = r5."RelatedTypeRootEntityId" AND r3."Id" = r5."RelatedTypeId"
    LEFT JOIN "RelatedCollection_NestedCollection" AS r6 ON r3."RootEntityId" = r6."RelatedTypeRootEntityId" AND r3."Id" = r6."RelatedTypeId"
) AS s ON r."Id" = s."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS r7 ON r0."RootEntityId" = r7."RelatedTypeRootEntityId"
WHERE r0."Ints"[1] = 1
ORDER BY r."Id" NULLS FIRST, r0."RootEntityId" NULLS FIRST, o."RootEntityId" NULLS FIRST, o0."RelatedTypeRootEntityId" NULLS FIRST, o1."RelatedTypeRootEntityId" NULLS FIRST, r1."RelatedTypeRootEntityId" NULLS FIRST, r2."RelatedTypeRootEntityId" NULLS FIRST, o2."RelatedTypeRootEntityId" NULLS FIRST, o2."Id" NULLS FIRST, s."RootEntityId" NULLS FIRST, s."Id" NULLS FIRST, s."RelatedTypeRootEntityId" NULLS FIRST, s."RelatedTypeId" NULLS FIRST, s."RelatedTypeRootEntityId0" NULLS FIRST, s."RelatedTypeId0" NULLS FIRST, s."RelatedTypeRootEntityId1" NULLS FIRST, s."RelatedTypeId1" NULLS FIRST, s."Id0" NULLS FIRST, r7."RelatedTypeRootEntityId" NULLS FIRST
""");
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT r."Id", r."Name", o."RootEntityId", o."Id", o."Int", o."Ints", o."Name", o."String", r0."RootEntityId", o0."RelatedTypeRootEntityId", o1."RelatedTypeRootEntityId", r1."RelatedTypeRootEntityId", r2."RelatedTypeRootEntityId", o2."RelatedTypeRootEntityId", o2."Id", o2."Int", o2."Ints", o2."Name", o2."String", o0."Id", o0."Int", o0."Ints", o0."Name", o0."String", o1."Id", o1."Int", o1."Ints", o1."Name", o1."String", s."RootEntityId", s."Id", s."Int", s."Ints", s."Name", s."String", s."RelatedTypeRootEntityId", s."RelatedTypeId", s."RelatedTypeRootEntityId0", s."RelatedTypeId0", s."RelatedTypeRootEntityId1", s."RelatedTypeId1", s."Id0", s."Int0", s."Ints0", s."Name0", s."String0", s."Id1", s."Int1", s."Ints1", s."Name1", s."String1", s."Id2", s."Int2", s."Ints2", s."Name2", s."String2", r0."Id", r0."Int", r0."Ints", r0."Name", r0."String", r7."RelatedTypeRootEntityId", r7."Id", r7."Int", r7."Ints", r7."Name", r7."String", r1."Id", r1."Int", r1."Ints", r1."Name", r1."String", r2."Id", r2."Int", r2."Ints", r2."Name", r2."String"
FROM "RootEntity" AS r
LEFT JOIN "RequiredRelated" AS r0 ON r."Id" = r0."RootEntityId"
LEFT JOIN "OptionalRelated" AS o ON r."Id" = o."RootEntityId"
LEFT JOIN "OptionalRelated_OptionalNested" AS o0 ON o."RootEntityId" = o0."RelatedTypeRootEntityId"
LEFT JOIN "OptionalRelated_RequiredNested" AS o1 ON o."RootEntityId" = o1."RelatedTypeRootEntityId"
LEFT JOIN "RequiredRelated_OptionalNested" AS r1 ON r0."RootEntityId" = r1."RelatedTypeRootEntityId"
LEFT JOIN "RequiredRelated_RequiredNested" AS r2 ON r0."RootEntityId" = r2."RelatedTypeRootEntityId"
LEFT JOIN "OptionalRelated_NestedCollection" AS o2 ON o."RootEntityId" = o2."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT r3."RootEntityId", r3."Id", r3."Int", r3."Ints", r3."Name", r3."String", r4."RelatedTypeRootEntityId", r4."RelatedTypeId", r5."RelatedTypeRootEntityId" AS "RelatedTypeRootEntityId0", r5."RelatedTypeId" AS "RelatedTypeId0", r6."RelatedTypeRootEntityId" AS "RelatedTypeRootEntityId1", r6."RelatedTypeId" AS "RelatedTypeId1", r6."Id" AS "Id0", r6."Int" AS "Int0", r6."Ints" AS "Ints0", r6."Name" AS "Name0", r6."String" AS "String0", r4."Id" AS "Id1", r4."Int" AS "Int1", r4."Ints" AS "Ints1", r4."Name" AS "Name1", r4."String" AS "String1", r5."Id" AS "Id2", r5."Int" AS "Int2", r5."Ints" AS "Ints2", r5."Name" AS "Name2", r5."String" AS "String2"
    FROM "RelatedCollection" AS r3
    LEFT JOIN "RelatedCollection_OptionalNested" AS r4 ON r3."RootEntityId" = r4."RelatedTypeRootEntityId" AND r3."Id" = r4."RelatedTypeId"
    LEFT JOIN "RelatedCollection_RequiredNested" AS r5 ON r3."RootEntityId" = r5."RelatedTypeRootEntityId" AND r3."Id" = r5."RelatedTypeId"
    LEFT JOIN "RelatedCollection_NestedCollection" AS r6 ON r3."RootEntityId" = r6."RelatedTypeRootEntityId" AND r3."Id" = r6."RelatedTypeId"
) AS s ON r."Id" = s."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS r7 ON r0."RootEntityId" = r7."RelatedTypeRootEntityId"
WHERE 3 = ANY (r0."Ints")
ORDER BY r."Id" NULLS FIRST, r0."RootEntityId" NULLS FIRST, o."RootEntityId" NULLS FIRST, o0."RelatedTypeRootEntityId" NULLS FIRST, o1."RelatedTypeRootEntityId" NULLS FIRST, r1."RelatedTypeRootEntityId" NULLS FIRST, r2."RelatedTypeRootEntityId" NULLS FIRST, o2."RelatedTypeRootEntityId" NULLS FIRST, o2."Id" NULLS FIRST, s."RootEntityId" NULLS FIRST, s."Id" NULLS FIRST, s."RelatedTypeRootEntityId" NULLS FIRST, s."RelatedTypeId" NULLS FIRST, s."RelatedTypeRootEntityId0" NULLS FIRST, s."RelatedTypeId0" NULLS FIRST, s."RelatedTypeRootEntityId1" NULLS FIRST, s."RelatedTypeId1" NULLS FIRST, s."Id0" NULLS FIRST, r7."RelatedTypeRootEntityId" NULLS FIRST
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT r."Id", r."Name", o."RootEntityId", o."Id", o."Int", o."Ints", o."Name", o."String", r0."RootEntityId", o0."RelatedTypeRootEntityId", o1."RelatedTypeRootEntityId", r1."RelatedTypeRootEntityId", r2."RelatedTypeRootEntityId", o2."RelatedTypeRootEntityId", o2."Id", o2."Int", o2."Ints", o2."Name", o2."String", o0."Id", o0."Int", o0."Ints", o0."Name", o0."String", o1."Id", o1."Int", o1."Ints", o1."Name", o1."String", s."RootEntityId", s."Id", s."Int", s."Ints", s."Name", s."String", s."RelatedTypeRootEntityId", s."RelatedTypeId", s."RelatedTypeRootEntityId0", s."RelatedTypeId0", s."RelatedTypeRootEntityId1", s."RelatedTypeId1", s."Id0", s."Int0", s."Ints0", s."Name0", s."String0", s."Id1", s."Int1", s."Ints1", s."Name1", s."String1", s."Id2", s."Int2", s."Ints2", s."Name2", s."String2", r0."Id", r0."Int", r0."Ints", r0."Name", r0."String", r7."RelatedTypeRootEntityId", r7."Id", r7."Int", r7."Ints", r7."Name", r7."String", r1."Id", r1."Int", r1."Ints", r1."Name", r1."String", r2."Id", r2."Int", r2."Ints", r2."Name", r2."String"
FROM "RootEntity" AS r
LEFT JOIN "RequiredRelated" AS r0 ON r."Id" = r0."RootEntityId"
LEFT JOIN "OptionalRelated" AS o ON r."Id" = o."RootEntityId"
LEFT JOIN "OptionalRelated_OptionalNested" AS o0 ON o."RootEntityId" = o0."RelatedTypeRootEntityId"
LEFT JOIN "OptionalRelated_RequiredNested" AS o1 ON o."RootEntityId" = o1."RelatedTypeRootEntityId"
LEFT JOIN "RequiredRelated_OptionalNested" AS r1 ON r0."RootEntityId" = r1."RelatedTypeRootEntityId"
LEFT JOIN "RequiredRelated_RequiredNested" AS r2 ON r0."RootEntityId" = r2."RelatedTypeRootEntityId"
LEFT JOIN "OptionalRelated_NestedCollection" AS o2 ON o."RootEntityId" = o2."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT r3."RootEntityId", r3."Id", r3."Int", r3."Ints", r3."Name", r3."String", r4."RelatedTypeRootEntityId", r4."RelatedTypeId", r5."RelatedTypeRootEntityId" AS "RelatedTypeRootEntityId0", r5."RelatedTypeId" AS "RelatedTypeId0", r6."RelatedTypeRootEntityId" AS "RelatedTypeRootEntityId1", r6."RelatedTypeId" AS "RelatedTypeId1", r6."Id" AS "Id0", r6."Int" AS "Int0", r6."Ints" AS "Ints0", r6."Name" AS "Name0", r6."String" AS "String0", r4."Id" AS "Id1", r4."Int" AS "Int1", r4."Ints" AS "Ints1", r4."Name" AS "Name1", r4."String" AS "String1", r5."Id" AS "Id2", r5."Int" AS "Int2", r5."Ints" AS "Ints2", r5."Name" AS "Name2", r5."String" AS "String2"
    FROM "RelatedCollection" AS r3
    LEFT JOIN "RelatedCollection_OptionalNested" AS r4 ON r3."RootEntityId" = r4."RelatedTypeRootEntityId" AND r3."Id" = r4."RelatedTypeId"
    LEFT JOIN "RelatedCollection_RequiredNested" AS r5 ON r3."RootEntityId" = r5."RelatedTypeRootEntityId" AND r3."Id" = r5."RelatedTypeId"
    LEFT JOIN "RelatedCollection_NestedCollection" AS r6 ON r3."RootEntityId" = r6."RelatedTypeRootEntityId" AND r3."Id" = r6."RelatedTypeId"
) AS s ON r."Id" = s."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS r7 ON r0."RootEntityId" = r7."RelatedTypeRootEntityId"
WHERE 2 = ANY (r0."Ints")
ORDER BY r."Id" NULLS FIRST, r0."RootEntityId" NULLS FIRST, o."RootEntityId" NULLS FIRST, o0."RelatedTypeRootEntityId" NULLS FIRST, o1."RelatedTypeRootEntityId" NULLS FIRST, r1."RelatedTypeRootEntityId" NULLS FIRST, r2."RelatedTypeRootEntityId" NULLS FIRST, o2."RelatedTypeRootEntityId" NULLS FIRST, o2."Id" NULLS FIRST, s."RootEntityId" NULLS FIRST, s."Id" NULLS FIRST, s."RelatedTypeRootEntityId" NULLS FIRST, s."RelatedTypeId" NULLS FIRST, s."RelatedTypeRootEntityId0" NULLS FIRST, s."RelatedTypeId0" NULLS FIRST, s."RelatedTypeRootEntityId1" NULLS FIRST, s."RelatedTypeId1" NULLS FIRST, s."Id0" NULLS FIRST, r7."RelatedTypeRootEntityId" NULLS FIRST
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT r."Id", r."Name", o."RootEntityId", o."Id", o."Int", o."Ints", o."Name", o."String", r0."RootEntityId", r1."RelatedTypeRootEntityId", o0."RelatedTypeRootEntityId", o1."RelatedTypeRootEntityId", r2."RelatedTypeRootEntityId", o2."RelatedTypeRootEntityId", o2."Id", o2."Int", o2."Ints", o2."Name", o2."String", o0."Id", o0."Int", o0."Ints", o0."Name", o0."String", o1."Id", o1."Int", o1."Ints", o1."Name", o1."String", s."RootEntityId", s."Id", s."Int", s."Ints", s."Name", s."String", s."RelatedTypeRootEntityId", s."RelatedTypeId", s."RelatedTypeRootEntityId0", s."RelatedTypeId0", s."RelatedTypeRootEntityId1", s."RelatedTypeId1", s."Id0", s."Int0", s."Ints0", s."Name0", s."String0", s."Id1", s."Int1", s."Ints1", s."Name1", s."String1", s."Id2", s."Int2", s."Ints2", s."Name2", s."String2", r0."Id", r0."Int", r0."Ints", r0."Name", r0."String", r7."RelatedTypeRootEntityId", r7."Id", r7."Int", r7."Ints", r7."Name", r7."String", r2."Id", r2."Int", r2."Ints", r2."Name", r2."String", r1."Id", r1."Int", r1."Ints", r1."Name", r1."String"
FROM "RootEntity" AS r
LEFT JOIN "RequiredRelated" AS r0 ON r."Id" = r0."RootEntityId"
LEFT JOIN "RequiredRelated_RequiredNested" AS r1 ON r0."RootEntityId" = r1."RelatedTypeRootEntityId"
LEFT JOIN "OptionalRelated" AS o ON r."Id" = o."RootEntityId"
LEFT JOIN "OptionalRelated_OptionalNested" AS o0 ON o."RootEntityId" = o0."RelatedTypeRootEntityId"
LEFT JOIN "OptionalRelated_RequiredNested" AS o1 ON o."RootEntityId" = o1."RelatedTypeRootEntityId"
LEFT JOIN "RequiredRelated_OptionalNested" AS r2 ON r0."RootEntityId" = r2."RelatedTypeRootEntityId"
LEFT JOIN "OptionalRelated_NestedCollection" AS o2 ON o."RootEntityId" = o2."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT r3."RootEntityId", r3."Id", r3."Int", r3."Ints", r3."Name", r3."String", r4."RelatedTypeRootEntityId", r4."RelatedTypeId", r5."RelatedTypeRootEntityId" AS "RelatedTypeRootEntityId0", r5."RelatedTypeId" AS "RelatedTypeId0", r6."RelatedTypeRootEntityId" AS "RelatedTypeRootEntityId1", r6."RelatedTypeId" AS "RelatedTypeId1", r6."Id" AS "Id0", r6."Int" AS "Int0", r6."Ints" AS "Ints0", r6."Name" AS "Name0", r6."String" AS "String0", r4."Id" AS "Id1", r4."Int" AS "Int1", r4."Ints" AS "Ints1", r4."Name" AS "Name1", r4."String" AS "String1", r5."Id" AS "Id2", r5."Int" AS "Int2", r5."Ints" AS "Ints2", r5."Name" AS "Name2", r5."String" AS "String2"
    FROM "RelatedCollection" AS r3
    LEFT JOIN "RelatedCollection_OptionalNested" AS r4 ON r3."RootEntityId" = r4."RelatedTypeRootEntityId" AND r3."Id" = r4."RelatedTypeId"
    LEFT JOIN "RelatedCollection_RequiredNested" AS r5 ON r3."RootEntityId" = r5."RelatedTypeRootEntityId" AND r3."Id" = r5."RelatedTypeId"
    LEFT JOIN "RelatedCollection_NestedCollection" AS r6 ON r3."RootEntityId" = r6."RelatedTypeRootEntityId" AND r3."Id" = r6."RelatedTypeId"
) AS s ON r."Id" = s."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS r7 ON r0."RootEntityId" = r7."RelatedTypeRootEntityId"
WHERE cardinality(r1."Ints") = 3
ORDER BY r."Id" NULLS FIRST, r0."RootEntityId" NULLS FIRST, r1."RelatedTypeRootEntityId" NULLS FIRST, o."RootEntityId" NULLS FIRST, o0."RelatedTypeRootEntityId" NULLS FIRST, o1."RelatedTypeRootEntityId" NULLS FIRST, r2."RelatedTypeRootEntityId" NULLS FIRST, o2."RelatedTypeRootEntityId" NULLS FIRST, o2."Id" NULLS FIRST, s."RootEntityId" NULLS FIRST, s."Id" NULLS FIRST, s."RelatedTypeRootEntityId" NULLS FIRST, s."RelatedTypeId" NULLS FIRST, s."RelatedTypeRootEntityId0" NULLS FIRST, s."RelatedTypeId0" NULLS FIRST, s."RelatedTypeRootEntityId1" NULLS FIRST, s."RelatedTypeId1" NULLS FIRST, s."Id0" NULLS FIRST, r7."RelatedTypeRootEntityId" NULLS FIRST
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(sum(i0.value), 0)::int
    FROM unnest(r0."Ints") AS i0(value))
FROM "RootEntity" AS r
LEFT JOIN "RequiredRelated" AS r0 ON r."Id" = r0."RootEntityId"
WHERE (
    SELECT COALESCE(sum(i.value), 0)::int
    FROM unnest(r0."Ints") AS i(value)) >= 6
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
