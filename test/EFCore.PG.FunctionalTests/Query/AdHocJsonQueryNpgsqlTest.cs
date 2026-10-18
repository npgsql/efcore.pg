using System.Text.Json;

#pragma warning disable EF8001 // ToJson on owned entities is obsolete

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocJsonQueryNpgsqlTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "AdHocJsonQueryTests";

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_predicate_on_bytea(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<TypesDbContext>(
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Bytea = [1, 2, 3] } },
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Bytea = [1, 2, 4] } });
                await context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var query = context.Entities.Where(x => x.JsonEntity.Bytea == new byte[] { 1, 2, 4 });

            var result = async
                ? await query.SingleAsync()
                : query.Single();

            Assert.Equal(2, result.Id);

            AssertSql(
                """
SELECT e."Id", e."JsonEntity"
FROM "Entities" AS e
WHERE (decode(e."JsonEntity" ->> 'Bytea', 'base64')) = BYTEA E'\\x010204'
LIMIT 2
""");
        }
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_predicate_on_interval(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<TypesDbContext>(
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Interval = new TimeSpan(1, 2, 3, 4, 123, 456) } },
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Interval = new TimeSpan(2, 2, 3, 4, 123, 456) } });
                await context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var query = context.Entities.Where(x => x.JsonEntity.Interval == new TimeSpan(2, 2, 3, 4, 123, 456));

            var result = async
                ? await query.SingleAsync()
                : query.Single();

            Assert.Equal(2, result.Id);

            AssertSql(
                """
SELECT e."Id", e."JsonEntity"
FROM "Entities" AS e
WHERE (CAST(e."JsonEntity" ->> 'Interval' AS interval)) = INTERVAL '2 02:03:04.123456'
LIMIT 2
""");
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Converted_json_composition(bool jsonb, bool coalesce)
    {
        var contextFactory = await InitializeNonSharedTest<ConvertedJsonContext>(
            onModelCreating: modelBuilder => modelBuilder.Entity<ConvertedJsonEntity>().Property(e => e.Payload)
                .HasColumnType(jsonb ? "jsonb" : "json")
                .HasConversion(value => value.Json, value => new JsonWrapper(value)),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new ConvertedJsonEntity { Id = 1, Payload = new JsonWrapper("""{"Age":25}""") },
                    new ConvertedJsonEntity { Id = 2 },
                    new ConvertedJsonEntity { Id = 3, Payload = new JsonWrapper("null") });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var fallback = new JsonWrapper("""{"Age":99}""");
        var entities = context.Entities.OrderBy(e => e.Id);
        var query = coalesce
            ? entities.Select(e => e.Payload ?? EF.Parameter(fallback))
            : entities.Select(e => e.Id == 2 ? EF.Parameter(fallback) : e.Payload);

        var values = await query.ToArrayAsync();
        using var first = JsonDocument.Parse(values[0].Json);
        using var second = JsonDocument.Parse(values[1].Json);
        using var third = JsonDocument.Parse(values[2].Json);
        Assert.Equal(25, first.RootElement.GetProperty("Age").GetInt32());
        Assert.Equal(99, second.RootElement.GetProperty("Age").GetInt32());
        Assert.Equal(JsonValueKind.Null, third.RootElement.ValueKind);

        var projection = coalesce
            ? """COALESCE(e."Payload", @fallback)"""
            : """
CASE
    WHEN e."Id" = 2 THEN @fallback
    ELSE e."Payload"
END
""";
        AssertSql(
            $$"""
@fallback='{"Age":99}' (DbType = Object)

SELECT {{projection}}
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Scalar_converted_json_case_parameter(bool jsonb)
    {
        var contextFactory = await InitializeNonSharedTest<ScalarConvertedJsonContext>(
            onModelCreating: modelBuilder => modelBuilder.Entity<ScalarConvertedJsonEntity>().Property(e => e.Payload)
                .HasColumnType(jsonb ? "jsonb" : "json")
                .HasConversion(
                    value => JsonSerializer.Serialize(new[] { value }, (JsonSerializerOptions)null),
                    value => JsonSerializer.Deserialize<int[]>(value, (JsonSerializerOptions)null)[0]),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new ScalarConvertedJsonEntity { Id = 1, Payload = 25 },
                    new ScalarConvertedJsonEntity { Id = 2, Payload = 35 });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var fallback = 99;
        var values = await context.Entities.OrderBy(e => e.Id)
            .Select(e => e.Id == 1 ? e.Payload : EF.Parameter(fallback)).ToArrayAsync();
        Assert.Equal([25, 99], values);

        AssertSql(
            """
@fallback='[99]' (Nullable = false) (DbType = Object)

SELECT CASE
    WHEN e."Id" = 1 THEN e."Payload"
    ELSE @fallback
END
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Scalar_converted_json_literal_comparison(bool jsonb)
    {
        var contextFactory = await InitializeNonSharedTest<ScalarConvertedJsonContext>(
            onModelCreating: modelBuilder => modelBuilder.Entity<ScalarConvertedJsonEntity>().Property(e => e.Payload)
                .HasColumnType(jsonb ? "jsonb" : "json")
                .HasConversion(
                    value => JsonSerializer.SerializeToElement(new[] { value }, (JsonSerializerOptions)null),
                    value => value[0].GetInt32()),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new ScalarConvertedJsonEntity { Id = 1, Payload = 25 },
                    new ScalarConvertedJsonEntity { Id = 2, Payload = 35 });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        Assert.Equal(1, await context.Entities.Where(e => e.Payload == 25).Select(e => e.Id).SingleAsync());

        var column = "e.\"Payload\"";
        var left = $"NULLIF({column}{(jsonb ? "" : "::jsonb")}, 'null'::jsonb)";
        var right = """NULLIF('[25]'::jsonb, 'null'::jsonb)""";
        AssertSql(
            $"""
SELECT e."Id"
FROM "Entities" AS e
WHERE {left} = {right} OR ({left} IS NULL AND {right} IS NULL)
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Native_enum_json_parameter_comparison(bool jsonb)
    {
        var contextFactory = await InitializeNonSharedTest<EnumJsonContext>(
            onModelCreating: modelBuilder => modelBuilder.Entity<EnumJsonEntity>().Property(e => e.Payload)
                .HasColumnType(jsonb ? "jsonb" : "json"),
            onConfiguring: options => options.UseNpgsql(
                ((NpgsqlTestStore)NonSharedTestStore).ConnectionString,
                o => o.MapEnum<JsonMood>("json_mood").ConfigureDataSource(b => b.EnableDynamicJson())),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new EnumJsonEntity { Id = 1, Payload = new() { ["Mood"] = JsonMood.Sad } },
                    new EnumJsonEntity { Id = 2, Payload = new() { ["Mood"] = JsonMood.Happy } });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var mood = JsonMood.Sad;
        Assert.Equal(1, await context.Entities.Where(e => e.Payload["Mood"] == (object)EF.Parameter(mood))
            .Select(e => e.Id).SingleAsync());

        var traversal = """e."Payload" -> 'Mood'""";
        var left = $"NULLIF({(jsonb ? traversal : $"CAST({traversal} AS jsonb)")}, 'null'::jsonb)";
        var right = """NULLIF(@mood, 'null'::jsonb)""";
        AssertSql(
            $"""
@mood='1' (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE {left} = {right} OR ({left} IS NULL AND {right} IS NULL)
LIMIT 2
""");
    }

    protected class EnumJsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<EnumJsonEntity> Entities { get; set; }
    }

    public class EnumJsonEntity
    {
        public int Id { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }

    public enum JsonMood
    {
        Happy,
        Sad
    }

    protected class ScalarConvertedJsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<ScalarConvertedJsonEntity> Entities { get; set; }
    }

    public class ScalarConvertedJsonEntity
    {
        public int Id { get; set; }
        public int Payload { get; set; }
    }

    protected class ConvertedJsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<ConvertedJsonEntity> Entities { get; set; }
    }

    public class ConvertedJsonEntity
    {
        public int Id { get; set; }
        public JsonWrapper Payload { get; set; }
    }

    public record JsonWrapper(string Json);

    protected class TypesDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TypesContainerEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<TypesContainerEntity>().OwnsOne(b => b.JsonEntity).ToJson();
    }

    public class TypesContainerEntity
    {
        public int Id { get; set; }
        public TypesJsonEntity JsonEntity { get; set; }
    }

    public class TypesJsonEntity
    {
        public byte[] Bytea { get; set; }
        public TimeSpan Interval { get; set; }
    }

    protected void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);
}
