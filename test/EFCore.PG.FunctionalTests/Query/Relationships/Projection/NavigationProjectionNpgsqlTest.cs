﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class NavigationProjectionNpgsqlTest
    : NavigationProjectionRelationalTestBase<NavigationRelationshipsNpgsqlFixture>
{
    public NavigationProjectionNpgsqlTest(NavigationRelationshipsNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_everything_using_joins(bool async)
    {
        await base.Select_everything_using_joins(async);

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalReferenceTrunkId", r."RequiredReferenceTrunkId", t."Id", t."CollectionRootId", t."Name", t."OptionalReferenceBranchId", t."RequiredReferenceBranchId", b."Id", b."CollectionTrunkId", b."Name", b."OptionalReferenceLeafId", b."RequiredReferenceLeafId", l."Id", l."CollectionBranchId", l."Name"
FROM "RootEntities" AS r
INNER JOIN "TrunkEntities" AS t ON r."Id" = t."Id"
INNER JOIN "BranchEntities" AS b ON t."Id" = b."Id"
INNER JOIN "LeafEntities" AS l ON b."Id" = l."Id"
""");
    }

    public override async Task Select_trunk_collection(bool async)
    {
        await base.Select_trunk_collection(async);

        AssertSql(
            """
SELECT r."Id", t."Id", t."CollectionRootId", t."Name", t."OptionalReferenceBranchId", t."RequiredReferenceBranchId"
FROM "RootEntities" AS r
LEFT JOIN "TrunkEntities" AS t ON r."Id" = t."CollectionRootId"
ORDER BY r."Id" NULLS FIRST
""");
    }

    public override async Task Select_branch_required_collection(bool async)
    {
        await base.Select_branch_required_collection(async);

        AssertSql(
            """
SELECT r."Id", t."Id", b."Id", b."CollectionTrunkId", b."Name", b."OptionalReferenceLeafId", b."RequiredReferenceLeafId"
FROM "RootEntities" AS r
INNER JOIN "TrunkEntities" AS t ON r."RequiredReferenceTrunkId" = t."Id"
LEFT JOIN "BranchEntities" AS b ON t."Id" = b."CollectionTrunkId"
ORDER BY r."Id" NULLS FIRST, t."Id" NULLS FIRST
""");
    }

    public override async Task Select_branch_optional_collection(bool async)
    {
        await base.Select_branch_optional_collection(async);

        AssertSql(
            """
SELECT r."Id", t."Id", b."Id", b."CollectionTrunkId", b."Name", b."OptionalReferenceLeafId", b."RequiredReferenceLeafId"
FROM "RootEntities" AS r
INNER JOIN "TrunkEntities" AS t ON r."RequiredReferenceTrunkId" = t."Id"
LEFT JOIN "BranchEntities" AS b ON t."Id" = b."CollectionTrunkId"
ORDER BY r."Id" NULLS FIRST, t."Id" NULLS FIRST
""");
    }

    public override async Task Select_multiple_branch_leaf(bool async)
    {
        // https://github.com/dotnet/efcore/pull/35942
        await Assert.ThrowsAsync<EqualException>(() => base.Select_multiple_branch_leaf(async));

        AssertSql(
            """
SELECT r."Id", b."Id", b."CollectionTrunkId", b."Name", b."OptionalReferenceLeafId", b."RequiredReferenceLeafId", l."Id", l."CollectionBranchId", l."Name", t."Id", l0."Id", l0."CollectionBranchId", l0."Name", b0."Id", b0."CollectionTrunkId", b0."Name", b0."OptionalReferenceLeafId", b0."RequiredReferenceLeafId"
FROM "RootEntities" AS r
INNER JOIN "TrunkEntities" AS t ON r."RequiredReferenceTrunkId" = t."Id"
INNER JOIN "BranchEntities" AS b ON t."RequiredReferenceBranchId" = b."Id"
LEFT JOIN "LeafEntities" AS l ON b."OptionalReferenceLeafId" = l."Id"
LEFT JOIN "LeafEntities" AS l0 ON b."Id" = l0."CollectionBranchId"
LEFT JOIN "BranchEntities" AS b0 ON t."Id" = b0."CollectionTrunkId"
ORDER BY r."Id" NULLS FIRST, t."Id" NULLS FIRST, b."Id" NULLS FIRST, l."Id" NULLS FIRST, l0."Id" NULLS FIRST
""");
    }

    public override async Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
    {
        await base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async);

        AssertSql(
            """
SELECT r."Id", s."Id", s."Id0", b."Id", b."CollectionTrunkId", b."Name", b."OptionalReferenceLeafId", b."RequiredReferenceLeafId", s.c
FROM "RootEntities" AS r
LEFT JOIN LATERAL (
    SELECT 1 AS c, r0."Id", t."Id" AS "Id0"
    FROM "RootEntities" AS r0
    INNER JOIN "TrunkEntities" AS t ON r0."RequiredReferenceTrunkId" = t."Id"
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS s ON TRUE
LEFT JOIN "BranchEntities" AS b ON s."Id0" = b."CollectionTrunkId"
ORDER BY r."Id" NULLS FIRST, s."Id" NULLS FIRST, s."Id0" NULLS FIRST
""");
    }

    public override async Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
    {
        await base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async);

        AssertSql(
            """
SELECT r."Id", t."Id", b."Id", s."Id1", s."Id", s."Id0", b1."Id", b1."CollectionTrunkId", b1."Name", b1."OptionalReferenceLeafId", b1."RequiredReferenceLeafId", s."CollectionRootId", s."Name", s."OptionalReferenceBranchId", s."RequiredReferenceBranchId", s."CollectionTrunkId", s."Name0", s."OptionalReferenceLeafId", s."RequiredReferenceLeafId", s."Name1", s.c
FROM "RootEntities" AS r
INNER JOIN "TrunkEntities" AS t ON r."RequiredReferenceTrunkId" = t."Id"
INNER JOIN "BranchEntities" AS b ON t."RequiredReferenceBranchId" = b."Id"
LEFT JOIN LATERAL (
    SELECT t0."Id", t0."CollectionRootId", t0."Name", t0."OptionalReferenceBranchId", t0."RequiredReferenceBranchId", b0."Id" AS "Id0", b0."CollectionTrunkId", b0."Name" AS "Name0", b0."OptionalReferenceLeafId", b0."RequiredReferenceLeafId", b."Name" AS "Name1", 1 AS c, r0."Id" AS "Id1"
    FROM "RootEntities" AS r0
    INNER JOIN "TrunkEntities" AS t0 ON r0."RequiredReferenceTrunkId" = t0."Id"
    INNER JOIN "BranchEntities" AS b0 ON t0."RequiredReferenceBranchId" = b0."Id"
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS s ON TRUE
LEFT JOIN "BranchEntities" AS b1 ON t."Id" = b1."CollectionTrunkId"
ORDER BY r."Id" NULLS FIRST, t."Id" NULLS FIRST, b."Id" NULLS FIRST, s."Id1" NULLS FIRST, s."Id" NULLS FIRST, s."Id0" NULLS FIRST
""");
    }

    public override async Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
    {
        await base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async);

        AssertSql(
            """
SELECT r."Id", t."Id", r1."Id", b."Id", b."CollectionTrunkId", b."Name", b."OptionalReferenceLeafId", b."RequiredReferenceLeafId", r1.c
FROM "RootEntities" AS r
INNER JOIN "TrunkEntities" AS t ON r."RequiredReferenceTrunkId" = t."Id"
LEFT JOIN LATERAL (
    SELECT 1 AS c, r0."Id"
    FROM "RootEntities" AS r0
    ORDER BY r0."Id" NULLS FIRST
    LIMIT 1
) AS r1 ON TRUE
LEFT JOIN "BranchEntities" AS b ON t."Id" = b."CollectionTrunkId"
ORDER BY r."Id" NULLS FIRST, t."Id" NULLS FIRST, r1."Id" NULLS FIRST
""");
    }

    public override async Task SelectMany_trunk_collection(bool async)
    {
        await base.SelectMany_trunk_collection(async);

        AssertSql(
            """
SELECT t."Id", t."CollectionRootId", t."Name", t."OptionalReferenceBranchId", t."RequiredReferenceBranchId"
FROM "RootEntities" AS r
INNER JOIN "TrunkEntities" AS t ON r."Id" = t."CollectionRootId"
""");
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_required_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT b."Id", b."CollectionTrunkId", b."Name", b."OptionalReferenceLeafId", b."RequiredReferenceLeafId"
FROM "RootEntities" AS r
INNER JOIN "TrunkEntities" AS t ON r."RequiredReferenceTrunkId" = t."Id"
INNER JOIN "BranchEntities" AS b ON t."Id" = b."CollectionTrunkId"
""");
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_optional_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT b."Id", b."CollectionTrunkId", b."Name", b."OptionalReferenceLeafId", b."RequiredReferenceLeafId"
FROM "RootEntities" AS r
LEFT JOIN "TrunkEntities" AS t ON r."OptionalReferenceTrunkId" = t."Id"
INNER JOIN "BranchEntities" AS b ON t."Id" = b."CollectionTrunkId"
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
