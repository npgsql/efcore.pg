// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedTableSplittingNoTrackingProjectionNpgsqlTest
    : OwnedTableSplittingNoTrackingProjectionRelationalTestBase<OwnedTableSplittingRelationshipsNpgsqlFixture>
{
    public OwnedTableSplittingNoTrackingProjectionNpgsqlTest(OwnedTableSplittingRelationshipsNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
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
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_branch_required_collection(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_branch_optional_collection(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_branch_optional_collection(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_multiple_branch_leaf(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_multiple_branch_leaf(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task SelectMany_trunk_collection(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.SelectMany_trunk_collection(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_required_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT r1."RelationshipsTrunkEntityRelationshipsRootEntityId", r1."Id1", r1."Name", r."Id", r0."RelationshipsRootEntityId", r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r2."RelationshipsBranchEntityId1", r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r3."RelationshipsBranchEntityId1", r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r4."RelationshipsBranchEntityId1", r4."Id1", r4."Name", r2."Name", r3."Name"
FROM "RootEntities" AS r
LEFT JOIN "Root_RequiredReferenceTrunk" AS r0 ON r."Id" = r0."RelationshipsRootEntityId"
INNER JOIN "Root_RequiredReferenceTrunk_CollectionBranch" AS r1 ON r0."RelationshipsRootEntityId" = r1."RelationshipsTrunkEntityRelationshipsRootEntityId"
LEFT JOIN "Root_RequiredReferenceTrunk_CollectionBranch_OptionalReferenceLeaf" AS r2 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r1."Id1" = r2."RelationshipsBranchEntityId1"
LEFT JOIN "Root_RequiredReferenceTrunk_CollectionBranch_RequiredReferenceLeaf" AS r3 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r1."Id1" = r3."RelationshipsBranchEntityId1"
LEFT JOIN "Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf" AS r4 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r1."Id1" = r4."RelationshipsBranchEntityId1"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsRootEntityId" NULLS FIRST, r1."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, r1."Id1" NULLS FIRST, r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r2."RelationshipsBranchEntityId1" NULLS FIRST, r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r3."RelationshipsBranchEntityId1" NULLS FIRST, r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r4."RelationshipsBranchEntityId1" NULLS FIRST
""");
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_optional_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT r1."RelationshipsTrunkEntityRelationshipsRootEntityId", r1."Id1", r1."Name", r."Id", r0."RelationshipsRootEntityId", r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r2."RelationshipsBranchEntityId1", r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r3."RelationshipsBranchEntityId1", r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r4."RelationshipsBranchEntityId1", r4."Id1", r4."Name", r2."Name", r3."Name"
FROM "RootEntities" AS r
LEFT JOIN "Root_OptionalReferenceTrunk" AS r0 ON r."Id" = r0."RelationshipsRootEntityId"
INNER JOIN "Root_OptionalReferenceTrunk_CollectionBranch" AS r1 ON r0."RelationshipsRootEntityId" = r1."RelationshipsTrunkEntityRelationshipsRootEntityId"
LEFT JOIN "Root_OptionalReferenceTrunk_CollectionBranch_OptionalReferenceLeaf" AS r2 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r1."Id1" = r2."RelationshipsBranchEntityId1"
LEFT JOIN "Root_OptionalReferenceTrunk_CollectionBranch_RequiredReferenceLeaf" AS r3 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r1."Id1" = r3."RelationshipsBranchEntityId1"
LEFT JOIN "Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf" AS r4 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" AND r1."Id1" = r4."RelationshipsBranchEntityId1"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsRootEntityId" NULLS FIRST, r1."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, r1."Id1" NULLS FIRST, r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r2."RelationshipsBranchEntityId1" NULLS FIRST, r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r3."RelationshipsBranchEntityId1" NULLS FIRST, r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r4."RelationshipsBranchEntityId1" NULLS FIRST
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
