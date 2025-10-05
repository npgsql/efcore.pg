namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public class NavigationsPrimitiveCollectionNpgsqlTest(NavigationsNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : NavigationsPrimitiveCollectionRelationalTestBase<NavigationsNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalAssociateId", r."RequiredAssociateId", a."Id", a0."Id", n."Id", n0."Id", n1."Id", n2."Id", s."Id", s."CollectionRootId", s."Int", s."Ints", s."Name", s."OptionalNestedAssociateId", s."RequiredNestedAssociateId", s."String", s."Id0", s."Id1", s."Id2", s."CollectionAssociateId", s."Int0", s."Ints0", s."Name0", s."String0", s."CollectionAssociateId0", s."Int1", s."Ints1", s."Name1", s."String1", s."CollectionAssociateId1", s."Int2", s."Ints2", s."Name2", s."String2", a0."CollectionRootId", a0."Int", a0."Ints", a0."Name", a0."OptionalNestedAssociateId", a0."RequiredNestedAssociateId", a0."String", n6."Id", n6."CollectionAssociateId", n6."Int", n6."Ints", n6."Name", n6."String", n."CollectionAssociateId", n."Int", n."Ints", n."Name", n."String", n0."CollectionAssociateId", n0."Int", n0."Ints", n0."Name", n0."String", a."CollectionRootId", a."Int", a."Ints", a."Name", a."OptionalNestedAssociateId", a."RequiredNestedAssociateId", a."String", n7."Id", n7."CollectionAssociateId", n7."Int", n7."Ints", n7."Name", n7."String", n1."CollectionAssociateId", n1."Int", n1."Ints", n1."Name", n1."String", n2."CollectionAssociateId", n2."Int", n2."Ints", n2."Name", n2."String"
FROM "RootEntity" AS r
INNER JOIN "AssociateType" AS a ON r."RequiredAssociateId" = a."Id"
LEFT JOIN "AssociateType" AS a0 ON r."OptionalAssociateId" = a0."Id"
LEFT JOIN "NestedAssociateType" AS n ON a0."OptionalNestedAssociateId" = n."Id"
LEFT JOIN "NestedAssociateType" AS n0 ON a0."RequiredNestedAssociateId" = n0."Id"
LEFT JOIN "NestedAssociateType" AS n1 ON a."OptionalNestedAssociateId" = n1."Id"
INNER JOIN "NestedAssociateType" AS n2 ON a."RequiredNestedAssociateId" = n2."Id"
LEFT JOIN (
    SELECT a1."Id", a1."CollectionRootId", a1."Int", a1."Ints", a1."Name", a1."OptionalNestedAssociateId", a1."RequiredNestedAssociateId", a1."String", n3."Id" AS "Id0", n4."Id" AS "Id1", n5."Id" AS "Id2", n5."CollectionAssociateId", n5."Int" AS "Int0", n5."Ints" AS "Ints0", n5."Name" AS "Name0", n5."String" AS "String0", n3."CollectionAssociateId" AS "CollectionAssociateId0", n3."Int" AS "Int1", n3."Ints" AS "Ints1", n3."Name" AS "Name1", n3."String" AS "String1", n4."CollectionAssociateId" AS "CollectionAssociateId1", n4."Int" AS "Int2", n4."Ints" AS "Ints2", n4."Name" AS "Name2", n4."String" AS "String2"
    FROM "AssociateType" AS a1
    LEFT JOIN "NestedAssociateType" AS n3 ON a1."OptionalNestedAssociateId" = n3."Id"
    INNER JOIN "NestedAssociateType" AS n4 ON a1."RequiredNestedAssociateId" = n4."Id"
    LEFT JOIN "NestedAssociateType" AS n5 ON a1."Id" = n5."CollectionAssociateId"
) AS s ON r."Id" = s."CollectionRootId"
LEFT JOIN "NestedAssociateType" AS n6 ON a0."Id" = n6."CollectionAssociateId"
LEFT JOIN "NestedAssociateType" AS n7 ON a."Id" = n7."CollectionAssociateId"
WHERE cardinality(a."Ints") = 3
ORDER BY r."Id" NULLS FIRST, a."Id" NULLS FIRST, a0."Id" NULLS FIRST, n."Id" NULLS FIRST, n0."Id" NULLS FIRST, n1."Id" NULLS FIRST, n2."Id" NULLS FIRST, s."Id" NULLS FIRST, s."Id0" NULLS FIRST, s."Id1" NULLS FIRST, s."Id2" NULLS FIRST, n6."Id" NULLS FIRST
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalAssociateId", r."RequiredAssociateId", a."Id", a0."Id", n."Id", n0."Id", n1."Id", n2."Id", s."Id", s."CollectionRootId", s."Int", s."Ints", s."Name", s."OptionalNestedAssociateId", s."RequiredNestedAssociateId", s."String", s."Id0", s."Id1", s."Id2", s."CollectionAssociateId", s."Int0", s."Ints0", s."Name0", s."String0", s."CollectionAssociateId0", s."Int1", s."Ints1", s."Name1", s."String1", s."CollectionAssociateId1", s."Int2", s."Ints2", s."Name2", s."String2", a0."CollectionRootId", a0."Int", a0."Ints", a0."Name", a0."OptionalNestedAssociateId", a0."RequiredNestedAssociateId", a0."String", n6."Id", n6."CollectionAssociateId", n6."Int", n6."Ints", n6."Name", n6."String", n."CollectionAssociateId", n."Int", n."Ints", n."Name", n."String", n0."CollectionAssociateId", n0."Int", n0."Ints", n0."Name", n0."String", a."CollectionRootId", a."Int", a."Ints", a."Name", a."OptionalNestedAssociateId", a."RequiredNestedAssociateId", a."String", n7."Id", n7."CollectionAssociateId", n7."Int", n7."Ints", n7."Name", n7."String", n1."CollectionAssociateId", n1."Int", n1."Ints", n1."Name", n1."String", n2."CollectionAssociateId", n2."Int", n2."Ints", n2."Name", n2."String"
FROM "RootEntity" AS r
INNER JOIN "AssociateType" AS a ON r."RequiredAssociateId" = a."Id"
LEFT JOIN "AssociateType" AS a0 ON r."OptionalAssociateId" = a0."Id"
LEFT JOIN "NestedAssociateType" AS n ON a0."OptionalNestedAssociateId" = n."Id"
LEFT JOIN "NestedAssociateType" AS n0 ON a0."RequiredNestedAssociateId" = n0."Id"
LEFT JOIN "NestedAssociateType" AS n1 ON a."OptionalNestedAssociateId" = n1."Id"
INNER JOIN "NestedAssociateType" AS n2 ON a."RequiredNestedAssociateId" = n2."Id"
LEFT JOIN (
    SELECT a1."Id", a1."CollectionRootId", a1."Int", a1."Ints", a1."Name", a1."OptionalNestedAssociateId", a1."RequiredNestedAssociateId", a1."String", n3."Id" AS "Id0", n4."Id" AS "Id1", n5."Id" AS "Id2", n5."CollectionAssociateId", n5."Int" AS "Int0", n5."Ints" AS "Ints0", n5."Name" AS "Name0", n5."String" AS "String0", n3."CollectionAssociateId" AS "CollectionAssociateId0", n3."Int" AS "Int1", n3."Ints" AS "Ints1", n3."Name" AS "Name1", n3."String" AS "String1", n4."CollectionAssociateId" AS "CollectionAssociateId1", n4."Int" AS "Int2", n4."Ints" AS "Ints2", n4."Name" AS "Name2", n4."String" AS "String2"
    FROM "AssociateType" AS a1
    LEFT JOIN "NestedAssociateType" AS n3 ON a1."OptionalNestedAssociateId" = n3."Id"
    INNER JOIN "NestedAssociateType" AS n4 ON a1."RequiredNestedAssociateId" = n4."Id"
    LEFT JOIN "NestedAssociateType" AS n5 ON a1."Id" = n5."CollectionAssociateId"
) AS s ON r."Id" = s."CollectionRootId"
LEFT JOIN "NestedAssociateType" AS n6 ON a0."Id" = n6."CollectionAssociateId"
LEFT JOIN "NestedAssociateType" AS n7 ON a."Id" = n7."CollectionAssociateId"
WHERE a."Ints"[1] = 1
ORDER BY r."Id" NULLS FIRST, a."Id" NULLS FIRST, a0."Id" NULLS FIRST, n."Id" NULLS FIRST, n0."Id" NULLS FIRST, n1."Id" NULLS FIRST, n2."Id" NULLS FIRST, s."Id" NULLS FIRST, s."Id0" NULLS FIRST, s."Id1" NULLS FIRST, s."Id2" NULLS FIRST, n6."Id" NULLS FIRST
""");
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalAssociateId", r."RequiredAssociateId", a."Id", a0."Id", n."Id", n0."Id", n1."Id", n2."Id", s."Id", s."CollectionRootId", s."Int", s."Ints", s."Name", s."OptionalNestedAssociateId", s."RequiredNestedAssociateId", s."String", s."Id0", s."Id1", s."Id2", s."CollectionAssociateId", s."Int0", s."Ints0", s."Name0", s."String0", s."CollectionAssociateId0", s."Int1", s."Ints1", s."Name1", s."String1", s."CollectionAssociateId1", s."Int2", s."Ints2", s."Name2", s."String2", a0."CollectionRootId", a0."Int", a0."Ints", a0."Name", a0."OptionalNestedAssociateId", a0."RequiredNestedAssociateId", a0."String", n6."Id", n6."CollectionAssociateId", n6."Int", n6."Ints", n6."Name", n6."String", n."CollectionAssociateId", n."Int", n."Ints", n."Name", n."String", n0."CollectionAssociateId", n0."Int", n0."Ints", n0."Name", n0."String", a."CollectionRootId", a."Int", a."Ints", a."Name", a."OptionalNestedAssociateId", a."RequiredNestedAssociateId", a."String", n7."Id", n7."CollectionAssociateId", n7."Int", n7."Ints", n7."Name", n7."String", n1."CollectionAssociateId", n1."Int", n1."Ints", n1."Name", n1."String", n2."CollectionAssociateId", n2."Int", n2."Ints", n2."Name", n2."String"
FROM "RootEntity" AS r
INNER JOIN "AssociateType" AS a ON r."RequiredAssociateId" = a."Id"
LEFT JOIN "AssociateType" AS a0 ON r."OptionalAssociateId" = a0."Id"
LEFT JOIN "NestedAssociateType" AS n ON a0."OptionalNestedAssociateId" = n."Id"
LEFT JOIN "NestedAssociateType" AS n0 ON a0."RequiredNestedAssociateId" = n0."Id"
LEFT JOIN "NestedAssociateType" AS n1 ON a."OptionalNestedAssociateId" = n1."Id"
INNER JOIN "NestedAssociateType" AS n2 ON a."RequiredNestedAssociateId" = n2."Id"
LEFT JOIN (
    SELECT a1."Id", a1."CollectionRootId", a1."Int", a1."Ints", a1."Name", a1."OptionalNestedAssociateId", a1."RequiredNestedAssociateId", a1."String", n3."Id" AS "Id0", n4."Id" AS "Id1", n5."Id" AS "Id2", n5."CollectionAssociateId", n5."Int" AS "Int0", n5."Ints" AS "Ints0", n5."Name" AS "Name0", n5."String" AS "String0", n3."CollectionAssociateId" AS "CollectionAssociateId0", n3."Int" AS "Int1", n3."Ints" AS "Ints1", n3."Name" AS "Name1", n3."String" AS "String1", n4."CollectionAssociateId" AS "CollectionAssociateId1", n4."Int" AS "Int2", n4."Ints" AS "Ints2", n4."Name" AS "Name2", n4."String" AS "String2"
    FROM "AssociateType" AS a1
    LEFT JOIN "NestedAssociateType" AS n3 ON a1."OptionalNestedAssociateId" = n3."Id"
    INNER JOIN "NestedAssociateType" AS n4 ON a1."RequiredNestedAssociateId" = n4."Id"
    LEFT JOIN "NestedAssociateType" AS n5 ON a1."Id" = n5."CollectionAssociateId"
) AS s ON r."Id" = s."CollectionRootId"
LEFT JOIN "NestedAssociateType" AS n6 ON a0."Id" = n6."CollectionAssociateId"
LEFT JOIN "NestedAssociateType" AS n7 ON a."Id" = n7."CollectionAssociateId"
WHERE 3 = ANY (a."Ints")
ORDER BY r."Id" NULLS FIRST, a."Id" NULLS FIRST, a0."Id" NULLS FIRST, n."Id" NULLS FIRST, n0."Id" NULLS FIRST, n1."Id" NULLS FIRST, n2."Id" NULLS FIRST, s."Id" NULLS FIRST, s."Id0" NULLS FIRST, s."Id1" NULLS FIRST, s."Id2" NULLS FIRST, n6."Id" NULLS FIRST
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalAssociateId", r."RequiredAssociateId", a."Id", a0."Id", n."Id", n0."Id", n1."Id", n2."Id", s."Id", s."CollectionRootId", s."Int", s."Ints", s."Name", s."OptionalNestedAssociateId", s."RequiredNestedAssociateId", s."String", s."Id0", s."Id1", s."Id2", s."CollectionAssociateId", s."Int0", s."Ints0", s."Name0", s."String0", s."CollectionAssociateId0", s."Int1", s."Ints1", s."Name1", s."String1", s."CollectionAssociateId1", s."Int2", s."Ints2", s."Name2", s."String2", a0."CollectionRootId", a0."Int", a0."Ints", a0."Name", a0."OptionalNestedAssociateId", a0."RequiredNestedAssociateId", a0."String", n6."Id", n6."CollectionAssociateId", n6."Int", n6."Ints", n6."Name", n6."String", n."CollectionAssociateId", n."Int", n."Ints", n."Name", n."String", n0."CollectionAssociateId", n0."Int", n0."Ints", n0."Name", n0."String", a."CollectionRootId", a."Int", a."Ints", a."Name", a."OptionalNestedAssociateId", a."RequiredNestedAssociateId", a."String", n7."Id", n7."CollectionAssociateId", n7."Int", n7."Ints", n7."Name", n7."String", n1."CollectionAssociateId", n1."Int", n1."Ints", n1."Name", n1."String", n2."CollectionAssociateId", n2."Int", n2."Ints", n2."Name", n2."String"
FROM "RootEntity" AS r
INNER JOIN "AssociateType" AS a ON r."RequiredAssociateId" = a."Id"
LEFT JOIN "AssociateType" AS a0 ON r."OptionalAssociateId" = a0."Id"
LEFT JOIN "NestedAssociateType" AS n ON a0."OptionalNestedAssociateId" = n."Id"
LEFT JOIN "NestedAssociateType" AS n0 ON a0."RequiredNestedAssociateId" = n0."Id"
LEFT JOIN "NestedAssociateType" AS n1 ON a."OptionalNestedAssociateId" = n1."Id"
INNER JOIN "NestedAssociateType" AS n2 ON a."RequiredNestedAssociateId" = n2."Id"
LEFT JOIN (
    SELECT a1."Id", a1."CollectionRootId", a1."Int", a1."Ints", a1."Name", a1."OptionalNestedAssociateId", a1."RequiredNestedAssociateId", a1."String", n3."Id" AS "Id0", n4."Id" AS "Id1", n5."Id" AS "Id2", n5."CollectionAssociateId", n5."Int" AS "Int0", n5."Ints" AS "Ints0", n5."Name" AS "Name0", n5."String" AS "String0", n3."CollectionAssociateId" AS "CollectionAssociateId0", n3."Int" AS "Int1", n3."Ints" AS "Ints1", n3."Name" AS "Name1", n3."String" AS "String1", n4."CollectionAssociateId" AS "CollectionAssociateId1", n4."Int" AS "Int2", n4."Ints" AS "Ints2", n4."Name" AS "Name2", n4."String" AS "String2"
    FROM "AssociateType" AS a1
    LEFT JOIN "NestedAssociateType" AS n3 ON a1."OptionalNestedAssociateId" = n3."Id"
    INNER JOIN "NestedAssociateType" AS n4 ON a1."RequiredNestedAssociateId" = n4."Id"
    LEFT JOIN "NestedAssociateType" AS n5 ON a1."Id" = n5."CollectionAssociateId"
) AS s ON r."Id" = s."CollectionRootId"
LEFT JOIN "NestedAssociateType" AS n6 ON a0."Id" = n6."CollectionAssociateId"
LEFT JOIN "NestedAssociateType" AS n7 ON a."Id" = n7."CollectionAssociateId"
WHERE 2 = ANY (a."Ints")
ORDER BY r."Id" NULLS FIRST, a."Id" NULLS FIRST, a0."Id" NULLS FIRST, n."Id" NULLS FIRST, n0."Id" NULLS FIRST, n1."Id" NULLS FIRST, n2."Id" NULLS FIRST, s."Id" NULLS FIRST, s."Id0" NULLS FIRST, s."Id1" NULLS FIRST, s."Id2" NULLS FIRST, n6."Id" NULLS FIRST
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalAssociateId", r."RequiredAssociateId", a."Id", n."Id", a0."Id", n0."Id", n1."Id", n2."Id", s."Id", s."CollectionRootId", s."Int", s."Ints", s."Name", s."OptionalNestedAssociateId", s."RequiredNestedAssociateId", s."String", s."Id0", s."Id1", s."Id2", s."CollectionAssociateId", s."Int0", s."Ints0", s."Name0", s."String0", s."CollectionAssociateId0", s."Int1", s."Ints1", s."Name1", s."String1", s."CollectionAssociateId1", s."Int2", s."Ints2", s."Name2", s."String2", a0."CollectionRootId", a0."Int", a0."Ints", a0."Name", a0."OptionalNestedAssociateId", a0."RequiredNestedAssociateId", a0."String", n6."Id", n6."CollectionAssociateId", n6."Int", n6."Ints", n6."Name", n6."String", n0."CollectionAssociateId", n0."Int", n0."Ints", n0."Name", n0."String", n1."CollectionAssociateId", n1."Int", n1."Ints", n1."Name", n1."String", a."CollectionRootId", a."Int", a."Ints", a."Name", a."OptionalNestedAssociateId", a."RequiredNestedAssociateId", a."String", n7."Id", n7."CollectionAssociateId", n7."Int", n7."Ints", n7."Name", n7."String", n2."CollectionAssociateId", n2."Int", n2."Ints", n2."Name", n2."String", n."CollectionAssociateId", n."Int", n."Ints", n."Name", n."String"
FROM "RootEntity" AS r
INNER JOIN "AssociateType" AS a ON r."RequiredAssociateId" = a."Id"
INNER JOIN "NestedAssociateType" AS n ON a."RequiredNestedAssociateId" = n."Id"
LEFT JOIN "AssociateType" AS a0 ON r."OptionalAssociateId" = a0."Id"
LEFT JOIN "NestedAssociateType" AS n0 ON a0."OptionalNestedAssociateId" = n0."Id"
LEFT JOIN "NestedAssociateType" AS n1 ON a0."RequiredNestedAssociateId" = n1."Id"
LEFT JOIN "NestedAssociateType" AS n2 ON a."OptionalNestedAssociateId" = n2."Id"
LEFT JOIN (
    SELECT a1."Id", a1."CollectionRootId", a1."Int", a1."Ints", a1."Name", a1."OptionalNestedAssociateId", a1."RequiredNestedAssociateId", a1."String", n3."Id" AS "Id0", n4."Id" AS "Id1", n5."Id" AS "Id2", n5."CollectionAssociateId", n5."Int" AS "Int0", n5."Ints" AS "Ints0", n5."Name" AS "Name0", n5."String" AS "String0", n3."CollectionAssociateId" AS "CollectionAssociateId0", n3."Int" AS "Int1", n3."Ints" AS "Ints1", n3."Name" AS "Name1", n3."String" AS "String1", n4."CollectionAssociateId" AS "CollectionAssociateId1", n4."Int" AS "Int2", n4."Ints" AS "Ints2", n4."Name" AS "Name2", n4."String" AS "String2"
    FROM "AssociateType" AS a1
    LEFT JOIN "NestedAssociateType" AS n3 ON a1."OptionalNestedAssociateId" = n3."Id"
    INNER JOIN "NestedAssociateType" AS n4 ON a1."RequiredNestedAssociateId" = n4."Id"
    LEFT JOIN "NestedAssociateType" AS n5 ON a1."Id" = n5."CollectionAssociateId"
) AS s ON r."Id" = s."CollectionRootId"
LEFT JOIN "NestedAssociateType" AS n6 ON a0."Id" = n6."CollectionAssociateId"
LEFT JOIN "NestedAssociateType" AS n7 ON a."Id" = n7."CollectionAssociateId"
WHERE cardinality(n."Ints") = 3
ORDER BY r."Id" NULLS FIRST, a."Id" NULLS FIRST, n."Id" NULLS FIRST, a0."Id" NULLS FIRST, n0."Id" NULLS FIRST, n1."Id" NULLS FIRST, n2."Id" NULLS FIRST, s."Id" NULLS FIRST, s."Id0" NULLS FIRST, s."Id1" NULLS FIRST, s."Id2" NULLS FIRST, n6."Id" NULLS FIRST
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(sum(i0.value), 0)::int
    FROM unnest(a."Ints") AS i0(value))
FROM "RootEntity" AS r
INNER JOIN "AssociateType" AS a ON r."RequiredAssociateId" = a."Id"
WHERE (
    SELECT COALESCE(sum(i.value), 0)::int
    FROM unnest(a."Ints") AS i(value)) >= 6
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
