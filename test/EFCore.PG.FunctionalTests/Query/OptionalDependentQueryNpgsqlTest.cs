using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit.Sdk;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class OptionalDependentQueryNpgsqlTest : OptionalDependentQueryTestBase<
    OptionalDependentQueryNpgsqlTest.OptionalDependentQueryNpgsqlFixture>
{
    public OptionalDependentQueryNpgsqlTest(OptionalDependentQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Basic_projection_entity_with_all_optional(bool async)
    {
        await base.Basic_projection_entity_with_all_optional(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesAllOptional" AS e
""");
    }

    public override async Task Basic_projection_entity_with_some_required(bool async)
    {
        await base.Basic_projection_entity_with_some_required(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesSomeRequired" AS e
""");
    }

    public override async Task Filter_optional_dependent_with_all_optional_compared_to_null(bool async)
    {
        await base.Filter_optional_dependent_with_all_optional_compared_to_null(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesAllOptional" AS e
WHERE (e."Json") IS NULL
""");
    }

    public override async Task Filter_optional_dependent_with_all_optional_compared_to_not_null(bool async)
    {
        await base.Filter_optional_dependent_with_all_optional_compared_to_not_null(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesAllOptional" AS e
WHERE (e."Json") IS NOT NULL
""");
    }

    public override async Task Filter_optional_dependent_with_some_required_compared_to_null(bool async)
    {
        await base.Filter_optional_dependent_with_some_required_compared_to_null(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesSomeRequired" AS e
WHERE (e."Json") IS NULL
""");
    }

    public override async Task Filter_optional_dependent_with_some_required_compared_to_not_null(bool async)
    {
        await base.Filter_optional_dependent_with_some_required_compared_to_not_null(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesSomeRequired" AS e
WHERE (e."Json") IS NOT NULL
""");
    }

    public override async Task Filter_nested_optional_dependent_with_all_optional_compared_to_null(bool async)
    {
        // #2942
        await Assert.ThrowsAsync<EqualException>(() => base.Filter_nested_optional_dependent_with_all_optional_compared_to_not_null(async));

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesAllOptional" AS e
WHERE (e."Json" -> 'OpNav2') IS NOT NULL
""");
    }

    public override async Task Filter_nested_optional_dependent_with_all_optional_compared_to_not_null(bool async)
    {
        // #2942
        await Assert.ThrowsAsync<EqualException>(() => base.Filter_nested_optional_dependent_with_all_optional_compared_to_not_null(async));

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesAllOptional" AS e
WHERE (e."Json" -> 'OpNav2') IS NOT NULL
""");
    }

    public override async Task Filter_nested_optional_dependent_with_some_required_compared_to_null(bool async)
    {
        await base.Filter_nested_optional_dependent_with_some_required_compared_to_null(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesSomeRequired" AS e
WHERE (e."Json" -> 'ReqNav1') IS NULL
""");
    }

    public override async Task Filter_nested_optional_dependent_with_some_required_compared_to_not_null(bool async)
    {
        await base.Filter_nested_optional_dependent_with_some_required_compared_to_not_null(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesSomeRequired" AS e
WHERE (e."Json" -> 'ReqNav2') IS NOT NULL
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class OptionalDependentQueryNpgsqlFixture : OptionalDependentQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
