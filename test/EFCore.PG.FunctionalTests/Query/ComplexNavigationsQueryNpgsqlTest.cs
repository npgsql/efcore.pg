using Xunit.Sdk;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class ComplexNavigationsQueryNpgsqlTest : ComplexNavigationsQueryRelationalTestBase<ComplexNavigationsQueryNpgsqlFixture>
{
    public ComplexNavigationsQueryNpgsqlTest(ComplexNavigationsQueryNpgsqlFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    // https://github.com/dotnet/efcore/pull/33060
    public override Task Max_in_multi_level_nested_subquery(bool async)
        => Assert.ThrowsAsync<EqualException>(() => base.Max_in_multi_level_nested_subquery(async));

    public override async Task Join_with_result_selector_returning_queryable_throws_validation_error(bool async)
        => await Assert.ThrowsAsync<ArgumentException>(
            () => base.Join_with_result_selector_returning_queryable_throws_validation_error(async));

    public override Task GroupJoin_client_method_in_OrderBy(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.GroupJoin_client_method_in_OrderBy(async),
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.ComplexNavigationsQueryTestBase<Npgsql.EntityFrameworkCore.PostgreSQL.Query.ComplexNavigationsQueryNpgsqlFixture>",
                "ClientMethodNullableInt"));

    public override Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async)
        => Assert.ThrowsAsync<EqualException>(
            async () => await base.Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(async));
}
