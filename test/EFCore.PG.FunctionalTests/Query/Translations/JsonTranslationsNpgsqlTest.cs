namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class JsonTranslationsNpgsqlTest : JsonTranslationsRelationalTestBase<JsonTranslationsNpgsqlTest.JsonTranslationsQueryNpgsqlFixture>
{
    public JsonTranslationsNpgsqlTest(JsonTranslationsQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }


    public override async Task JsonExists_on_scalar_string_column()
    {
        // TODO: #3733
        await AssertTranslationFailed(base.JsonExists_on_scalar_string_column);

        AssertSql();
    }

    public override async Task JsonExists_on_complex_property()
    {
        // TODO: #3733
        await AssertTranslationFailed(base.JsonExists_on_complex_property);

        AssertSql();
    }

    public override async Task JsonExists_on_owned_entity()
    {
        // TODO: #3733
        await AssertTranslationFailed(base.JsonExists_on_owned_entity);

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

        protected override string RemoveJsonProperty(string column, string jsonPath)
        {
            // HACK. PostgreSQL doesn't have a delete function accepting JSON path, but the base class requires this
            // only for a single path segment, which we can do. Rethink this mechanism in EF.
            if (jsonPath.StartsWith("$."))
            {
                var segment = jsonPath[2..];
                if (!segment.Contains('.'))
                {
                    return $"{column} - '{segment}'";
                }
            }

            throw new UnreachableException();
        }
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
