// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedReferenceNoTrackingProjectionNpgsqlTest
    : OwnedReferenceNoTrackingProjectionRelationalTestBase<OwnedRelationshipsNpgsqlFixture>
{
    public OwnedReferenceNoTrackingProjectionNpgsqlTest(OwnedRelationshipsNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_root(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_root(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_trunk_optional(bool async)
    {
        await base.Select_trunk_optional(async);

        AssertSql(
            """
SELECT r."Id", r."OptionalReferenceTrunk_Name", s."RelationshipsTrunkEntityRelationshipsRootEntityId", s."Id1", s."Name", s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", s."RelationshipsBranchEntityId1", s."Id10", s."Name0", s."OptionalReferenceLeaf_Name", s."RequiredReferenceLeaf_Name", r."OptionalReferenceTrunk_OptionalReferenceBranch_Name", r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r2."Id1", r2."Name", r."OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferen~", r."OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferen~", r."OptionalReferenceTrunk_RequiredReferenceBranch_Name", r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r3."Id1", r3."Name", r."OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferen~", r."OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferen~"
FROM "RootEntities" AS r
LEFT JOIN (
    SELECT r0."RelationshipsTrunkEntityRelationshipsRootEntityId", r0."Id1", r0."Name", r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r1."RelationshipsBranchEntityId1", r1."Id1" AS "Id10", r1."Name" AS "Name0", r0."OptionalReferenceLeaf_Name", r0."RequiredReferenceLeaf_Name"
    FROM "Root_OptionalReferenceTrunk_CollectionBranch" AS r0
    LEFT JOIN "Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf" AS r1 ON r0."RelationshipsTrunkEntityRelationshipsRootEntityId" = r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r0."Id1" = r1."RelationshipsBranchEntityId1"
) AS s ON CASE
    WHEN r."OptionalReferenceTrunk_Name" IS NOT NULL THEN r."Id"
END = s."RelationshipsTrunkEntityRelationshipsRootEntityId"
LEFT JOIN "Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf" AS r2 ON CASE
    WHEN r."OptionalReferenceTrunk_OptionalReferenceBranch_Name" IS NOT NULL THEN r."Id"
END = r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN "Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf" AS r3 ON CASE
    WHEN r."OptionalReferenceTrunk_RequiredReferenceBranch_Name" IS NOT NULL THEN r."Id"
END = r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
ORDER BY r."Id" NULLS FIRST, s."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, s."Id1" NULLS FIRST, s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, s."RelationshipsBranchEntityId1" NULLS FIRST, s."Id10" NULLS FIRST, r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r2."Id1" NULLS FIRST, r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST
""");
    }

    public override async Task Select_trunk_required(bool async)
    {
        await base.Select_trunk_required(async);

        AssertSql(
            """
SELECT r."Id", r."RequiredReferenceTrunk_Name", s."RelationshipsTrunkEntityRelationshipsRootEntityId", s."Id1", s."Name", s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", s."RelationshipsBranchEntityId1", s."Id10", s."Name0", s."OptionalReferenceLeaf_Name", s."RequiredReferenceLeaf_Name", r."RequiredReferenceTrunk_OptionalReferenceBranch_Name", r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r2."Id1", r2."Name", r."RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferen~", r."RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferen~", r."RequiredReferenceTrunk_RequiredReferenceBranch_Name", r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r3."Id1", r3."Name", r."RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferen~", r."RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferen~"
FROM "RootEntities" AS r
LEFT JOIN (
    SELECT r0."RelationshipsTrunkEntityRelationshipsRootEntityId", r0."Id1", r0."Name", r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r1."RelationshipsBranchEntityId1", r1."Id1" AS "Id10", r1."Name" AS "Name0", r0."OptionalReferenceLeaf_Name", r0."RequiredReferenceLeaf_Name"
    FROM "Root_RequiredReferenceTrunk_CollectionBranch" AS r0
    LEFT JOIN "Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf" AS r1 ON r0."RelationshipsTrunkEntityRelationshipsRootEntityId" = r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r0."Id1" = r1."RelationshipsBranchEntityId1"
) AS s ON r."Id" = s."RelationshipsTrunkEntityRelationshipsRootEntityId"
LEFT JOIN "Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf" AS r2 ON CASE
    WHEN r."RequiredReferenceTrunk_OptionalReferenceBranch_Name" IS NOT NULL THEN r."Id"
END = r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf" AS r3 ON r."Id" = r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
ORDER BY r."Id" NULLS FIRST, s."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, s."Id1" NULLS FIRST, s."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, s."RelationshipsBranchEntityId1" NULLS FIRST, s."Id10" NULLS FIRST, r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r2."Id1" NULLS FIRST, r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST
""");
    }

    public override async Task Select_branch_required_required(bool async)
    {
        await base.Select_branch_required_required(async);

        AssertSql(
            """
SELECT r."Id", r."RequiredReferenceTrunk_RequiredReferenceBranch_Name", r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r0."Id1", r0."Name", r."RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferen~", r."RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferen~"
FROM "RootEntities" AS r
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf" AS r0 ON r."Id" = r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST
""");
    }

    public override async Task Select_branch_required_optional(bool async)
    {
        await base.Select_branch_required_optional(async);

        AssertSql(
            """
SELECT r."Id", r."RequiredReferenceTrunk_OptionalReferenceBranch_Name", r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r0."Id1", r0."Name", r."RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferen~", r."RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferen~"
FROM "RootEntities" AS r
LEFT JOIN "Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf" AS r0 ON CASE
    WHEN r."RequiredReferenceTrunk_OptionalReferenceBranch_Name" IS NOT NULL THEN r."Id"
END = r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST
""");
    }

    public override async Task Select_branch_optional_required(bool async)
    {
        await base.Select_branch_optional_required(async);

        AssertSql(
            """
SELECT r."Id", r."RequiredReferenceTrunk_RequiredReferenceBranch_Name", r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r0."Id1", r0."Name", r."RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferen~", r."RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferen~"
FROM "RootEntities" AS r
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf" AS r0 ON r."Id" = r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST
""");
    }

    public override async Task Select_branch_optional_optional(bool async)
    {
        await base.Select_branch_optional_optional(async);

        AssertSql(
            """
SELECT r."Id", r."RequiredReferenceTrunk_OptionalReferenceBranch_Name", r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r0."Id1", r0."Name", r."RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferen~", r."RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferen~"
FROM "RootEntities" AS r
LEFT JOIN "Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf" AS r0 ON CASE
    WHEN r."RequiredReferenceTrunk_OptionalReferenceBranch_Name" IS NOT NULL THEN r."Id"
END = r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST
""");
    }

    public override async Task Select_root_duplicated(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_root(async));

        Assert.Equal("42702", exception.SqlState);
    }

    // https://github.com/dotnet/efcore/issues/26993
    public override Task Select_trunk_and_branch_duplicated(bool async)
        => Task.CompletedTask;

    // https://github.com/dotnet/efcore/issues/26993
    public override Task Select_trunk_and_trunk_duplicated(bool async)
        => Task.CompletedTask;

    public override async Task Select_leaf_trunk_root(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_leaf_trunk_root(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async);

        AssertSql(
            """
SELECT r2."Id", r2."RequiredReferenceTrunk_RequiredReferenceBranch_Name", r."Id", r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r1."Id1", r1."Name", r2."RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferen~", r2."RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferen~"
FROM "RootEntities" AS r
LEFT JOIN LATERAL (
    SELECT r0."Id", r0."RequiredReferenceTrunk_RequiredReferenceBranch_Name", r0."RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferen~", r0."RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferen~"
    FROM "RootEntities" AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS r2 ON TRUE
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf" AS r1 ON r2."Id" = r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
ORDER BY r."Id" NULLS FIRST, r2."Id" NULLS FIRST, r1."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST
""");
    }

    // https://github.com/dotnet/efcore/issues/26993
    public override Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => Task.CompletedTask;

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
