// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedTableSplittingReferenceProjectionNoTrackingNpgsqlTest
    : OwnedTableSplittingReferenceProjectionNoTrackingRelationalTestBase<OwnedTableSplittingRelationshipsNpgsqlFixture>
{
    public OwnedTableSplittingReferenceProjectionNoTrackingNpgsqlTest(OwnedTableSplittingRelationshipsNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
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
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_trunk_optional(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_trunk_required(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_trunk_required(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_branch_required_required(bool async)
    {
        await base.Select_branch_required_required(async);

        AssertSql(
            """
SELECT r1."RelationshipsTrunkEntityRelationshipsRootEntityId", r1."Name", r."Id", r0."RelationshipsRootEntityId", r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r4."Id1", r4."Name", r2."Name", r3."Name"
FROM "RootEntities" AS r
LEFT JOIN "Root_RequiredReferenceTrunk" AS r0 ON r."Id" = r0."RelationshipsRootEntityId"
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch" AS r1 ON r0."RelationshipsRootEntityId" = r1."RelationshipsTrunkEntityRelationshipsRootEntityId"
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf" AS r2 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf" AS r3 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf" AS r4 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsRootEntityId" NULLS FIRST, r1."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST
""");
    }

    public override async Task Select_branch_required_optional(bool async)
    {
        await base.Select_branch_required_optional(async);

        AssertSql(
            """
SELECT r1."RelationshipsTrunkEntityRelationshipsRootEntityId", r1."Name", r."Id", r0."RelationshipsRootEntityId", r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r4."Id1", r4."Name", r2."Name", r3."Name"
FROM "RootEntities" AS r
LEFT JOIN "Root_RequiredReferenceTrunk" AS r0 ON r."Id" = r0."RelationshipsRootEntityId"
LEFT JOIN "Root_RequiredReferenceTrunk_OptionalReferenceBranch" AS r1 ON r0."RelationshipsRootEntityId" = r1."RelationshipsTrunkEntityRelationshipsRootEntityId"
LEFT JOIN "Root_RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf" AS r2 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN "Root_RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf" AS r3 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN "Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf" AS r4 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsRootEntityId" NULLS FIRST, r1."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST
""");
    }

    public override async Task Select_branch_optional_required(bool async)
    {
        await base.Select_branch_optional_required(async);

        AssertSql(
            """
SELECT r1."RelationshipsTrunkEntityRelationshipsRootEntityId", r1."Name", r."Id", r0."RelationshipsRootEntityId", r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r4."Id1", r4."Name", r2."Name", r3."Name"
FROM "RootEntities" AS r
LEFT JOIN "Root_RequiredReferenceTrunk" AS r0 ON r."Id" = r0."RelationshipsRootEntityId"
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch" AS r1 ON r0."RelationshipsRootEntityId" = r1."RelationshipsTrunkEntityRelationshipsRootEntityId"
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf" AS r2 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf" AS r3 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN "Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf" AS r4 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsRootEntityId" NULLS FIRST, r1."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST
""");
    }

    public override async Task Select_branch_optional_optional(bool async)
    {
        await base.Select_branch_optional_optional(async);

        AssertSql(
            """
SELECT r1."RelationshipsTrunkEntityRelationshipsRootEntityId", r1."Name", r."Id", r0."RelationshipsRootEntityId", r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~", r4."Id1", r4."Name", r2."Name", r3."Name"
FROM "RootEntities" AS r
LEFT JOIN "Root_RequiredReferenceTrunk" AS r0 ON r."Id" = r0."RelationshipsRootEntityId"
LEFT JOIN "Root_RequiredReferenceTrunk_OptionalReferenceBranch" AS r1 ON r0."RelationshipsRootEntityId" = r1."RelationshipsTrunkEntityRelationshipsRootEntityId"
LEFT JOIN "Root_RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf" AS r2 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN "Root_RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf" AS r3 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
LEFT JOIN "Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf" AS r4 ON r1."RelationshipsTrunkEntityRelationshipsRootEntityId" = r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~"
ORDER BY r."Id" NULLS FIRST, r0."RelationshipsRootEntityId" NULLS FIRST, r1."RelationshipsTrunkEntityRelationshipsRootEntityId" NULLS FIRST, r2."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r3."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST, r4."RelationshipsBranchEntityRelationshipsTrunkEntityRelationships~" NULLS FIRST
""");
    }

    public override async Task Select_root_duplicated(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_root_duplicated(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_trunk_and_branch_duplicated(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_trunk_and_branch_duplicated(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_trunk_and_trunk_duplicated(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_trunk_and_trunk_duplicated(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_leaf_trunk_root(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_leaf_trunk_root(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override async Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async));

        Assert.Equal("42702", exception.SqlState);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
