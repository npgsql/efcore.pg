// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedNoTrackingProjectionNpgsqlTest
    : OwnedNoTrackingProjectionRelationalTestBase<OwnedRelationshipsNpgsqlFixture>
{
    public OwnedNoTrackingProjectionNpgsqlTest(OwnedRelationshipsNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_trunk_collection(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_trunk_collection(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_branch_required_collection(bool async)
    {
        await base.Select_branch_required_collection(async);

        AssertSql(
            """
SELECT r."Id", s."RelationshipsTrunkEntityRelationshipsRootEntityId", s."Id1", s."Name", s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", s."RelationshipsBranchEntityId1", s."Id10", s."Name0", s."OptionalReferenceLeaf_Name", s."RequiredReferenceLeaf_Name"
FROM "RootEntities" AS r
LEFT JOIN (
    SELECT r0."RelationshipsTrunkEntityRelationshipsRootEntityId", r0."Id1", r0."Name", r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r1."RelationshipsBranchEntityId1", r1."Id1" AS "Id10", r1."Name" AS "Name0", r0."OptionalReferenceLeaf_Name", r0."RequiredReferenceLeaf_Name"
    FROM "Root_RequiredReferenceTrunk_CollectionBranch" AS r0
    LEFT JOIN "Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf" AS r1 ON r0."RelationshipsTrunkEntityRelationshipsRootEntityId" = r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r0."Id1" = r1."RelationshipsBranchEntityId1"
) AS s ON r."Id" = s."RelationshipsTrunkEntityRelationshipsRootEntityId"
ORDER BY r."Id" NULLS FIRST, s."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, s."Id1" NULLS FIRST, s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, s."RelationshipsBranchEntityId1" NULLS FIRST
""");
    }

    public override async Task Select_branch_optional_collection(bool async)
    {
        await base.Select_branch_optional_collection(async);

        AssertSql(
            """
SELECT r."Id", s."RelationshipsTrunkEntityRelationshipsRootEntityId", s."Id1", s."Name", s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", s."RelationshipsBranchEntityId1", s."Id10", s."Name0", s."OptionalReferenceLeaf_Name", s."RequiredReferenceLeaf_Name"
FROM "RootEntities" AS r
LEFT JOIN (
    SELECT r0."RelationshipsTrunkEntityRelationshipsRootEntityId", r0."Id1", r0."Name", r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r1."RelationshipsBranchEntityId1", r1."Id1" AS "Id10", r1."Name" AS "Name0", r0."OptionalReferenceLeaf_Name", r0."RequiredReferenceLeaf_Name"
    FROM "Root_RequiredReferenceTrunk_CollectionBranch" AS r0
    LEFT JOIN "Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf" AS r1 ON r0."RelationshipsTrunkEntityRelationshipsRootEntityId" = r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r0."Id1" = r1."RelationshipsBranchEntityId1"
) AS s ON r."Id" = s."RelationshipsTrunkEntityRelationshipsRootEntityId"
ORDER BY r."Id" NULLS FIRST, s."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, s."Id1" NULLS FIRST, s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, s."RelationshipsBranchEntityId1" NULLS FIRST
""");
    }

    public override async Task Select_multiple_branch_leaf(bool async)
    {
        await base.Select_multiple_branch_leaf(async);

        AssertSql(
            """
SELECT r."Id", r."RequiredReferenceTrunk_RequiredReferenceBranch_Name", r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r0."Id1", r0."Name", r."RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferen~", r."RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferen~", r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r1."Id1", r1."Name", s."RelationshipsTrunkEntityRelationshipsRootEntityId", s."Id1", s."Name", s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", s."RelationshipsBranchEntityId1", s."Id10", s."Name0", s."OptionalReferenceLeaf_Name", s."RequiredReferenceLeaf_Name"
FROM "RootEntities" AS r
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf" AS r0 ON r."Id" = r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf" AS r1 ON r."Id" = r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN (
    SELECT r2."RelationshipsTrunkEntityRelationshipsRootEntityId", r2."Id1", r2."Name", r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r3."RelationshipsBranchEntityId1", r3."Id1" AS "Id10", r3."Name" AS "Name0", r2."OptionalReferenceLeaf_Name", r2."RequiredReferenceLeaf_Name"
    FROM "Root_RequiredReferenceTrunk_CollectionBranch" AS r2
    LEFT JOIN "Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf" AS r3 ON r2."RelationshipsTrunkEntityRelationshipsRootEntityId" = r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r2."Id1" = r3."RelationshipsBranchEntityId1"
) AS s ON r."Id" = s."RelationshipsTrunkEntityRelationshipsRootEntityId"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r0."Id1" NULLS FIRST, r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r1."Id1" NULLS FIRST, s."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, s."Id1" NULLS FIRST, s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, s."RelationshipsBranchEntityId1" NULLS FIRST
""");
    }

    public override async Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
    {
        await base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async);

        AssertSql(
            """
SELECT r."Id", r3."Id", s."RelationshipsTrunkEntityRelationshipsRootEntityId", s."Id1", s."Name", s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", s."RelationshipsBranchEntityId1", s."Id10", s."Name0", s."OptionalReferenceLeaf_Name", s."RequiredReferenceLeaf_Name", r3.c
FROM "RootEntities" AS r
LEFT JOIN LATERAL (
    SELECT 1 AS c, r0."Id"
    FROM "RootEntities" AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS r3 ON TRUE
LEFT JOIN (
    SELECT r1."RelationshipsTrunkEntityRelationshipsRootEntityId", r1."Id1", r1."Name", r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r2."RelationshipsBranchEntityId1", r2."Id1" AS "Id10", r2."Name" AS "Name0", r1."OptionalReferenceLeaf_Name", r1."RequiredReferenceLeaf_Name"
    FROM "Root_RequiredReferenceTrunk_CollectionBranch" AS r1
    LEFT JOIN "Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf" AS r2 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r1."Id1" = r2."RelationshipsBranchEntityId1"
) AS s ON r3."Id" = s."RelationshipsTrunkEntityRelationshipsRootEntityId"
ORDER BY r."Id" NULLS FIRST, r3."Id" NULLS FIRST, s."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, s."Id1" NULLS FIRST, s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, s."RelationshipsBranchEntityId1" NULLS FIRST
""");
    }

    // https://github.com/dotnet/efcore/pull/35942
    public override Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
        => Task.CompletedTask;

    public override async Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
    {
        await base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async);

        AssertSql(
            """
SELECT r."Id", r3."Id", s."RelationshipsTrunkEntityRelationshipsRootEntityId", s."Id1", s."Name", s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", s."RelationshipsBranchEntityId1", s."Id10", s."Name0", s."OptionalReferenceLeaf_Name", s."RequiredReferenceLeaf_Name", r3.c
FROM "RootEntities" AS r
LEFT JOIN LATERAL (
    SELECT 1 AS c, r0."Id"
    FROM "RootEntities" AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS r3 ON TRUE
LEFT JOIN (
    SELECT r1."RelationshipsTrunkEntityRelationshipsRootEntityId", r1."Id1", r1."Name", r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r2."RelationshipsBranchEntityId1", r2."Id1" AS "Id10", r2."Name" AS "Name0", r1."OptionalReferenceLeaf_Name", r1."RequiredReferenceLeaf_Name"
    FROM "Root_RequiredReferenceTrunk_CollectionBranch" AS r1
    LEFT JOIN "Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf" AS r2 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r1."Id1" = r2."RelationshipsBranchEntityId1"
) AS s ON r."Id" = s."RelationshipsTrunkEntityRelationshipsRootEntityId"
ORDER BY r."Id" NULLS FIRST, r3."Id" NULLS FIRST, s."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, s."Id1" NULLS FIRST, s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, s."RelationshipsBranchEntityId1" NULLS FIRST
""");
    }

    public override async Task SelectMany_trunk_collection(bool async)
    {
        await base.SelectMany_trunk_collection(async);

        AssertSql(
            """
SELECT r0."RelationshipsRootEntityId", r0."Id1", r0."Name", r."Id", s."RelationshipsTrunkEntityRelationshipsRootEntityId", s."RelationshipsTrunkEntityId1", s."Id1", s."Name", s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", s."RelationshipsBranchEntityRelationshipsTrunkEntityId1", s."RelationshipsBranchEntityId1", s."Id10", s."Name0", s."OptionalReferenceLeaf_Name", s."RequiredReferenceLeaf_Name", r0."OptionalReferenceBranch_Name", r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r3."RelationshipsBranchEntityRelationshipsTrunkEntityId1", r3."Id1", r3."Name", r0."OptionalReferenceBranch_OptionalReferenceLeaf_Name", r0."OptionalReferenceBranch_RequiredReferenceLeaf_Name", r0."RequiredReferenceBranch_Name", r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r4."RelationshipsBranchEntityRelationshipsTrunkEntityId1", r4."Id1", r4."Name", r0."RequiredReferenceBranch_OptionalReferenceLeaf_Name", r0."RequiredReferenceBranch_RequiredReferenceLeaf_Name"
FROM "RootEntities" AS r
INNER JOIN "Root_CollectionTrunk" AS r0 ON r."Id" = r0."RelationshipsRootEntityId"
LEFT JOIN (
    SELECT r1."RelationshipsTrunkEntityRelationshipsRootEntityId", r1."RelationshipsTrunkEntityId1", r1."Id1", r1."Name", r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r2."RelationshipsBranchEntityRelationshipsTrunkEntityId1", r2."RelationshipsBranchEntityId1", r2."Id1" AS "Id10", r2."Name" AS "Name0", r1."OptionalReferenceLeaf_Name", r1."RequiredReferenceLeaf_Name"
    FROM "Root_CollectionTrunk_CollectionBranch" AS r1
    LEFT JOIN "Root_CollectionTrunk_CollectionBranch_CollectionLeaf" AS r2 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r1."RelationshipsTrunkEntityId1" = r2."RelationshipsBranchEntityRelationshipsTrunkEntityId1" AND r1."Id1" = r2."RelationshipsBranchEntityId1"
) AS s ON r0."RelationshipsRootEntityId" = s."RelationshipsTrunkEntityRelationshipsRootEntityId" AND r0."Id1" = s."RelationshipsTrunkEntityId1"
LEFT JOIN "Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf" AS r3 ON CASE
    WHEN r0."OptionalReferenceBranch_Name" IS NOT NULL THEN r0."RelationshipsRootEntityId"
END = r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND CASE
    WHEN r0."OptionalReferenceBranch_Name" IS NOT NULL THEN r0."Id1"
END = r3."RelationshipsBranchEntityRelationshipsTrunkEntityId1"
LEFT JOIN "Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf" AS r4 ON r0."RelationshipsRootEntityId" = r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r0."Id1" = r4."RelationshipsBranchEntityRelationshipsTrunkEntityId1"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsRootEntityId" NULLS FIRST, r0."Id1" NULLS FIRST, s."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, s."RelationshipsTrunkEntityId1" NULLS FIRST, s."Id1" NULLS FIRST, s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, s."RelationshipsBranchEntityRelationshipsTrunkEntityId1" NULLS FIRST, s."RelationshipsBranchEntityId1" NULLS FIRST, s."Id10" NULLS FIRST, r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r3."RelationshipsBranchEntityRelationshipsTrunkEntityId1" NULLS FIRST, r3."Id1" NULLS FIRST, r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r4."RelationshipsBranchEntityRelationshipsTrunkEntityId1" NULLS FIRST
""");
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_required_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT r0."RelationshipsTrunkEntityRelationshipsRootEntityId", r0."Id1", r0."Name", r."Id", r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r1."RelationshipsBranchEntityId1", r1."Id1", r1."Name", r0."OptionalReferenceLeaf_Name", r0."RequiredReferenceLeaf_Name"
FROM "RootEntities" AS r
INNER JOIN "Root_RequiredReferenceTrunk_CollectionBranch" AS r0 ON r."Id" = r0."RelationshipsTrunkEntityRelationshipsRootEntityId"
LEFT JOIN "Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf" AS r1 ON r0."RelationshipsTrunkEntityRelationshipsRootEntityId" = r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r0."Id1" = r1."RelationshipsBranchEntityId1"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, r0."Id1" NULLS FIRST, r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r1."RelationshipsBranchEntityId1" NULLS FIRST
""");
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_optional_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT r0."RelationshipsTrunkEntityRelationshipsRootEntityId", r0."Id1", r0."Name", r."Id", r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r1."RelationshipsBranchEntityId1", r1."Id1", r1."Name", r0."OptionalReferenceLeaf_Name", r0."RequiredReferenceLeaf_Name"
FROM "RootEntities" AS r
INNER JOIN "Root_OptionalReferenceTrunk_CollectionBranch" AS r0 ON CASE
    WHEN r."OptionalReferenceTrunk_Name" IS NOT NULL THEN r."Id"
END = r0."RelationshipsTrunkEntityRelationshipsRootEntityId"
LEFT JOIN "Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf" AS r1 ON r0."RelationshipsTrunkEntityRelationshipsRootEntityId" = r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r0."Id1" = r1."RelationshipsBranchEntityId1"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, r0."Id1" NULLS FIRST, r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r1."RelationshipsBranchEntityId1" NULLS FIRST
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
