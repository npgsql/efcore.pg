// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedReferenceProjectionNpgsqlTest
    : OwnedReferenceProjectionRelationalTestBase<OwnedRelationshipsNpgsqlFixture>
{
    public OwnedReferenceProjectionNpgsqlTest(OwnedRelationshipsNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
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

    public override async Task Select_root_duplicated(bool async)
    {
        // https://github.com/dotnet/efcore/issues/26993
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Select_root_duplicated(async));

        Assert.Equal("42702", exception.SqlState);
    }

    public override Task Select_trunk_optional(bool async)
        => AssertCantTrackOwned(() => base.Select_trunk_optional(async));

    public override Task Select_trunk_required(bool async)
        => AssertCantTrackOwned(() => base.Select_trunk_required(async));

    public override Task Select_branch_required_required(bool async)
        => AssertCantTrackOwned(() => base.Select_branch_required_required(async));

    public override Task Select_branch_required_optional(bool async)
        => AssertCantTrackOwned(() => base.Select_branch_required_optional(async));

    public override Task Select_branch_optional_required(bool async)
        => AssertCantTrackOwned(() => base.Select_branch_optional_required(async));

    public override Task Select_branch_optional_optional(bool async)
        => AssertCantTrackOwned(() => base.Select_branch_optional_optional(async));

    public override Task Select_trunk_and_branch_duplicated(bool async)
        => AssertCantTrackOwned(() => base.Select_trunk_and_branch_duplicated(async));

    public override Task Select_trunk_and_trunk_duplicated(bool async)
        => AssertCantTrackOwned(() => base.Select_trunk_and_trunk_duplicated(async));

    public override Task Select_leaf_trunk_root(bool async)
        => AssertCantTrackOwned(() => base.Select_leaf_trunk_root(async));

    public override Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
        => AssertCantTrackOwned(() => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async));

    public override Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => AssertCantTrackOwned(() => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async));

    private async Task AssertCantTrackOwned(Func<Task> test)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
