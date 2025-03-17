// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedJsonNoTrackingProjectionNpgsqlTest
    : OwnedJsonNoTrackingProjectionRelationalTestBase<OwnedJsonRelationshipsNpgsqlFixture>
{
    public OwnedJsonNoTrackingProjectionNpgsqlTest(OwnedJsonRelationshipsNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_trunk_collection(bool async)
    {
        await base.Select_trunk_collection(async);

        AssertSql(
            """
SELECT r."CollectionTrunk", r."Id"
FROM "RootEntities" AS r
ORDER BY r."Id" NULLS FIRST
""");
    }

    public override async Task Select_branch_required_collection(bool async)
    {
        await base.Select_branch_required_collection(async);

        AssertSql(
            """
SELECT r."RequiredReferenceTrunk" -> 'CollectionBranch', r."Id"
FROM "RootEntities" AS r
ORDER BY r."Id" NULLS FIRST
""");
    }

    public override async Task Select_branch_optional_collection(bool async)
    {
        await base.Select_branch_optional_collection(async);

        AssertSql(
            """
SELECT r."RequiredReferenceTrunk" -> 'CollectionBranch', r."Id"
FROM "RootEntities" AS r
ORDER BY r."Id" NULLS FIRST
""");
    }

    public override async Task Select_multiple_branch_leaf(bool async)
    {
        await base.Select_multiple_branch_leaf(async);

        AssertSql(
            """
SELECT r."Id", r."RequiredReferenceTrunk" -> 'RequiredReferenceBranch', r."RequiredReferenceTrunk" #> '{RequiredReferenceBranch,OptionalReferenceLeaf}', r."RequiredReferenceTrunk" #> '{RequiredReferenceBranch,CollectionLeaf}', r."RequiredReferenceTrunk" -> 'CollectionBranch', r."RequiredReferenceTrunk" #>> '{RequiredReferenceBranch,OptionalReferenceLeaf,Name}'
FROM "RootEntities" AS r
""");
    }

    public override async Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
    {
        await base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async);

        AssertSql(
            """
SELECT r1.c, r1."Id", r1.c0
FROM "RootEntities" AS r
LEFT JOIN LATERAL (
    SELECT r0."RequiredReferenceTrunk" -> 'CollectionBranch' AS c, r0."Id", 1 AS c0
    FROM "RootEntities" AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS r1 ON TRUE
ORDER BY r."Id" NULLS FIRST
""");
    }

    public override async Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
    {
        await base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async);

        AssertSql(
            """
SELECT r1.c, r1."Id", r1.c0, r1."Id0", r1.c1, r1.c2, r1.c3, r1.c4
FROM "RootEntities" AS r
LEFT JOIN LATERAL (
    SELECT r."RequiredReferenceTrunk" -> 'CollectionBranch' AS c, r."Id", r0."RequiredReferenceTrunk" AS c0, r0."Id" AS "Id0", r0."RequiredReferenceTrunk" -> 'RequiredReferenceBranch' AS c1, r0."RequiredReferenceTrunk" ->> 'Name' AS c2, r."RequiredReferenceTrunk" #>> '{RequiredReferenceBranch,Name}' AS c3, 1 AS c4
    FROM "RootEntities" AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS r1 ON TRUE
ORDER BY r."Id" NULLS FIRST
""");
    }

    public override async Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
    {
        await base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async);

        AssertSql(
            """
SELECT r1.c, r1."Id", r1.c0
FROM "RootEntities" AS r
LEFT JOIN LATERAL (
    SELECT r."RequiredReferenceTrunk" -> 'CollectionBranch' AS c, r."Id", 1 AS c0
    FROM "RootEntities" AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS r1 ON TRUE
ORDER BY r."Id" NULLS FIRST
""");
    }

    public override async Task SelectMany_trunk_collection(bool async)
    {
        await base.SelectMany_trunk_collection(async);

        AssertSql(
            """
SELECT r."Id", c."Name", c."CollectionBranch", c."OptionalReferenceBranch", c."RequiredReferenceBranch"
FROM "RootEntities" AS r
JOIN LATERAL ROWS FROM (jsonb_to_recordset(r."CollectionTrunk") AS (
    "Name" text,
    "CollectionBranch" jsonb,
    "OptionalReferenceBranch" jsonb,
    "RequiredReferenceBranch" jsonb
)) WITH ORDINALITY AS c ON TRUE
""");
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_required_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT r."Id", c."Name", c."CollectionLeaf", c."OptionalReferenceLeaf", c."RequiredReferenceLeaf"
FROM "RootEntities" AS r
JOIN LATERAL ROWS FROM (jsonb_to_recordset(r."RequiredReferenceTrunk" -> 'CollectionBranch') AS (
    "Name" text,
    "CollectionLeaf" jsonb,
    "OptionalReferenceLeaf" jsonb,
    "RequiredReferenceLeaf" jsonb
)) WITH ORDINALITY AS c ON TRUE
""");
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_optional_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT r."Id", c."Name", c."CollectionLeaf", c."OptionalReferenceLeaf", c."RequiredReferenceLeaf"
FROM "RootEntities" AS r
JOIN LATERAL ROWS FROM (jsonb_to_recordset(r."OptionalReferenceTrunk" -> 'CollectionBranch') AS (
    "Name" text,
    "CollectionLeaf" jsonb,
    "OptionalReferenceLeaf" jsonb,
    "RequiredReferenceLeaf" jsonb
)) WITH ORDINALITY AS c ON TRUE
""");
    }

    public override async Task Project_branch_collection_element_using_indexer_constant(bool async)
    {
        await base.Project_branch_collection_element_using_indexer_constant(async);

        AssertSql(
            """
SELECT r."RequiredReferenceTrunk" -> 'CollectionBranch', r."Id"
FROM "RootEntities" AS r
ORDER BY r."Id" NULLS FIRST
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
