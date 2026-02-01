using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocPrecompiledQueryNpgsqlTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : AdHocPrecompiledQueryRelationalTestBase(fixture, testOutputHelper)
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    public override async Task Index_no_evaluatability()
    {
        await base.Index_no_evaluatability();

        AssertSql(
            """
SELECT j."Id", j."IntList", j."JsonThing"
FROM "JsonEntities" AS j
WHERE j."IntList"[j."Id" + 1] = 2
""");
    }

    public override async Task Index_with_captured_variable()
    {
        await base.Index_with_captured_variable();

        AssertSql(
            """
@id='1'

SELECT j."Id", j."IntList", j."JsonThing"
FROM "JsonEntities" AS j
WHERE j."IntList"[@id + 1] = 2
""");
    }

    public override async Task JsonScalar()
    {
        await base.JsonScalar();

        AssertSql(
            """
SELECT j."Id", j."IntList", j."JsonThing"
FROM "JsonEntities" AS j
WHERE (j."JsonThing" ->> 'StringProperty') = 'foo'
""");
    }

    public override async Task Materialize_non_public()
    {
        await base.Materialize_non_public();

        AssertSql(
            """
@p0='10' (Nullable = true)
@p1='9' (Nullable = true)
@p2='8' (Nullable = true)

INSERT INTO "NonPublicEntities" ("PrivateAutoProperty", "PrivateProperty", "_privateField")
VALUES (@p0, @p1, @p2)
RETURNING "Id";
""",
            //
            """
SELECT n."Id", n."PrivateAutoProperty", n."PrivateProperty", n."_privateField"
FROM "NonPublicEntities" AS n
LIMIT 2
""");
    }

    public override async Task Projecting_property_requiring_converter_with_closure_is_not_supported()
    {
        await base.Projecting_property_requiring_converter_with_closure_is_not_supported();

        AssertSql();
    }

    public override async Task Projecting_expression_requiring_converter_without_closure_works()
    {
        await base.Projecting_expression_requiring_converter_without_closure_works();

        AssertSql(
            """
SELECT b."AudiobookDate"
FROM "Books" AS b
""");
    }

    public override async Task Projecting_entity_with_property_requiring_converter_with_closure_works()
    {
        await base.Projecting_entity_with_property_requiring_converter_with_closure_works();

        AssertSql(
            """
SELECT b."Id", b."AudiobookDate", b."Name", b."PublishDate"
FROM "Books" AS b
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
        => NpgsqlPrecompiledQueryTestHelpers.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        builder = base.AddOptions(builder);

        // TODO: Figure out if there's a nice way to continue using the retrying strategy
        var sqlServerOptionsBuilder = new NpgsqlDbContextOptionsBuilder(builder);
        sqlServerOptionsBuilder.ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
        return builder;
    }
}
