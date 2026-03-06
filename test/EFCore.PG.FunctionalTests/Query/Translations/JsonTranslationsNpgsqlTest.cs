namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class JsonTranslationsNpgsqlTest : JsonTranslationsRelationalTestBase<JsonTranslationsNpgsqlTest.JsonTranslationsQueryNpgsqlFixture>
{
    public JsonTranslationsNpgsqlTest(JsonTranslationsQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [MinimumPostgresVersion(17, 0)]
    public override async Task JsonPathExists_on_scalar_string_column()
    {
        await base.JsonPathExists_on_scalar_string_column();

        AssertSql(
            """
SELECT j."Id", j."JsonString", j."JsonComplexType", j."JsonOwnedType"
FROM "JsonEntities" AS j
WHERE JSON_EXISTS(j."JsonString", '$.OptionalInt')
""");
    }

    [MinimumPostgresVersion(17, 0)]
    public override async Task JsonPathExists_on_complex_property()
    {
        await base.JsonPathExists_on_complex_property();

        AssertSql(
            """
SELECT j."Id", j."JsonString", j."JsonComplexType", j."JsonOwnedType"
FROM "JsonEntities" AS j
WHERE JSON_EXISTS(j."JsonComplexType", '$.OptionalInt')
""");
    }

    [MinimumPostgresVersion(17, 0)]
    public override async Task JsonPathExists_on_owned_entity()
    {
        await base.JsonPathExists_on_owned_entity();

        AssertSql(
            """
SELECT j."Id", j."JsonString", j."JsonComplexType", j."JsonOwnedType"
FROM "JsonEntities" AS j
WHERE JSON_EXISTS(j."JsonOwnedType", '$.OptionalInt')
""");
    }

    public class JsonTranslationsQueryNpgsqlFixture : JsonTranslationsQueryFixtureBase, ITestSqlLoggerFactory
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<JsonTranslationsEntity>().Property(e => e.JsonString).HasColumnType("jsonb");
        }

        protected override string RemoveJsonProperty(string column, string property)
            => $"{column} - '{property}'";
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
