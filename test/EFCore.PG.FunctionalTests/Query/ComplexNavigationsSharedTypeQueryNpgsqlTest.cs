using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexNavigationsSharedTypeQueryNpgsqlTest
    : ComplexNavigationsSharedTypeQueryRelationalTestBase<ComplexNavigationsSharedTypeQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public ComplexNavigationsSharedTypeQueryNpgsqlTest(
        ComplexNavigationsSharedTypeQueryNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/26353")]
    public override Task Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(bool async)
        => base.Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(async);

    public override async Task Join_with_result_selector_returning_queryable_throws_validation_error(bool async)
        => await Assert.ThrowsAsync<ArgumentException>(
            () => base.Join_with_result_selector_returning_queryable_throws_validation_error(async));

    public override Task GroupJoin_client_method_in_OrderBy(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.GroupJoin_client_method_in_OrderBy(async),
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.ComplexNavigationsQueryTestBase<Microsoft.EntityFrameworkCore.Query.ComplexNavigationsSharedTypeQueryNpgsqlFixture>",
                "ClientMethodNullableInt"));

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/26104")]
    public override Task GroupBy_aggregate_where_required_relationship(bool async)
        => base.GroupBy_aggregate_where_required_relationship(async);

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/26104")]
    public override Task GroupBy_aggregate_where_required_relationship_2(bool async)
        => base.GroupBy_aggregate_where_required_relationship_2(async);
}
