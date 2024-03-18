using Microsoft.EntityFrameworkCore.TestModels.OptionalDependent;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

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
        await base.Filter_nested_optional_dependent_with_all_optional_compared_to_null(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesAllOptional" AS e
WHERE (e."Json" ->> 'OpNav1') IS NULL
""");
    }

    public override async Task Filter_nested_optional_dependent_with_all_optional_compared_to_not_null(bool async)
    {
        await base.Filter_nested_optional_dependent_with_all_optional_compared_to_not_null(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesAllOptional" AS e
WHERE (e."Json" ->> 'OpNav2') IS NOT NULL
""");
    }

    public override async Task Filter_nested_optional_dependent_with_some_required_compared_to_null(bool async)
    {
        await base.Filter_nested_optional_dependent_with_some_required_compared_to_null(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesSomeRequired" AS e
WHERE (e."Json" ->> 'ReqNav1') IS NULL
""");
    }

    public override async Task Filter_nested_optional_dependent_with_some_required_compared_to_not_null(bool async)
    {
        await base.Filter_nested_optional_dependent_with_some_required_compared_to_not_null(async);

        AssertSql(
            """
SELECT e."Id", e."Name", e."Json"
FROM "EntitiesSomeRequired" AS e
WHERE (e."Json" ->> 'ReqNav2') IS NOT NULL
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class OptionalDependentQueryNpgsqlFixture : OptionalDependentQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // The EF seed data has Unspecified DateTimes, but we map DateTime to timestamptz by default, which requires UTC DateTimes.
            // Configure the properties to have timestamp, which allows Unspecified DateTimes.
            modelBuilder.Entity<OptionalDependentEntityAllOptional>().OwnsOne(
                x => x.Json,
                b => b.OwnsOne(x => x.OpNav2, b2 => b2.Property(op => op.ReqNested2).HasColumnType("timestamp without time zone")));

            modelBuilder.Entity<OptionalDependentEntitySomeRequired>().OwnsOne(
                x => x.Json, b =>
                {
                    b.OwnsOne(x => x.OpNav2, b2 => b2.Property(op => op.ReqNested2).HasColumnType("timestamp without time zone"));
                    b.OwnsOne(x => x.ReqNav2, b2 => b2.Property(op => op.ReqNested2).HasColumnType("timestamp without time zone"));
                });
        }
    }
}
