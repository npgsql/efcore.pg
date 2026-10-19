using System.Text.Json;

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

    [ConditionalTheory]
    [InlineData("jsonb")]
    [InlineData("json")]
    public virtual async Task Byte_array_mapped_to_json_column(string storeType)
    {
        await using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                context.Add(
                    new ByteArrayJsonEntity
                    {
                        Id = 1,
                        Json = """{"Foo":"Bar"}"""u8.ToArray(),
                        Jsonb = """{"Foo":"Bar"}"""u8.ToArray()
                    });
                await context.SaveChangesAsync();

                // Simulate a JSON document which wasn't written via EF.
                await context.Database.ExecuteSqlRawAsync(
                    """
INSERT INTO "ByteArrayJsonEntities" ("Id", "Json", "Jsonb") VALUES (2, '{{"Foo":"Baz"}}', '{{"Foo":"Baz"}}')
""");

                var column = storeType == "jsonb" ? nameof(ByteArrayJsonEntity.Jsonb) : nameof(ByteArrayJsonEntity.Json);
                var sql = $$"""SELECT "{{column}}" ->> 'Foo' AS "Value" FROM "ByteArrayJsonEntities" WHERE "Id" = {0}""";

                Assert.Equal("Bar", await context.Database.SqlQueryRaw<string>(sql, 1).SingleAsync());

                var entities = await context.Set<ByteArrayJsonEntity>().AsNoTracking().OrderBy(e => e.Id).ToListAsync();
                Assert.Equal(2, entities.Count);
                using var efDocument = JsonDocument.Parse(storeType == "jsonb" ? entities[0].Jsonb : entities[0].Json);
                using var externalDocument = JsonDocument.Parse(storeType == "jsonb" ? entities[1].Jsonb : entities[1].Json);
                Assert.Equal("Bar", efDocument.RootElement.GetProperty("Foo").GetString());
                Assert.Equal("Baz", externalDocument.RootElement.GetProperty("Foo").GetString());

                context.Attach(entities[1]);
                entities[1].Json = """{"Foo":"Qux"}"""u8.ToArray();
                entities[1].Jsonb = """{"Foo":"Qux"}"""u8.ToArray();
                await context.SaveChangesAsync();

                Assert.Equal("Qux", await context.Database.SqlQueryRaw<string>(sql, 2).SingleAsync());
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Byte_array_mapped_to_json_column_in_predicate(bool async)
    {
        await using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                var json = """{"Foo":"O'Reilly"}"""u8.ToArray();
                context.AddRange(
                    new ByteArrayJsonEntity { Id = 1, Json = json, Jsonb = json },
                    new ByteArrayJsonEntity
                    {
                        Id = 2,
                        Json = """{"Foo":"Bar"}"""u8.ToArray(),
                        Jsonb = """{"Foo":"Bar"}"""u8.ToArray()
                    });
                await context.SaveChangesAsync();

                var parameterQuery = context.Set<ByteArrayJsonEntity>().Where(e => e.Jsonb == json);
                var parameterResult = async ? await parameterQuery.SingleAsync() : parameterQuery.Single();
                Assert.Equal(1, parameterResult.Id);

                Fixture.TestSqlLoggerFactory.Clear();

                var constantQuery = context.Set<ByteArrayJsonEntity>().Where(e => e.Jsonb == EF.Constant(json));
                var constantResult = async ? await constantQuery.SingleAsync() : constantQuery.Single();
                Assert.Equal(1, constantResult.Id);

                AssertSql(
                    """
SELECT b."Id", b."Json", b."Jsonb"
FROM "ByteArrayJsonEntities" AS b
WHERE b."Jsonb" = '{"Foo":"O''Reilly"}'
LIMIT 2
""");
            });
    }

    public class ByteArrayJsonEntity
    {
        public int Id { get; set; }
        public byte[] Json { get; set; } = null!;
        public byte[] Jsonb { get; set; } = null!;
    }

    public class JsonTranslationsQueryNpgsqlFixture : JsonTranslationsQueryFixtureBase, ITestSqlLoggerFactory
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<JsonTranslationsEntity>().Property(e => e.JsonString).HasColumnType("jsonb");

            modelBuilder.Entity<ByteArrayJsonEntity>(b =>
            {
                b.ToTable("ByteArrayJsonEntities");
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Json).HasColumnType("json");
                b.Property(e => e.Jsonb).HasColumnType("jsonb");
            });
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
