namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class JsonTranslationsNpgsqlTest : JsonTranslationsRelationalTestBase<JsonTranslationsNpgsqlTest.JsonTranslationsQueryNpgsqlFixture>
{
    public JsonTranslationsNpgsqlTest(JsonTranslationsQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task JsonPathExists_on_scalar_string_column()
    {
        // TODO: #3733
        await AssertTranslationFailed(base.JsonPathExists_on_scalar_string_column);

        AssertSql();
    }

    public override async Task JsonPathExists_on_complex_property()
    {
        // TODO: #3733
        await AssertTranslationFailed(base.JsonPathExists_on_complex_property);

        AssertSql();
    }

    public override async Task JsonPathExists_on_owned_entity()
    {
        // TODO: #3733
        await AssertTranslationFailed(base.JsonPathExists_on_owned_entity);

        AssertSql();
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
