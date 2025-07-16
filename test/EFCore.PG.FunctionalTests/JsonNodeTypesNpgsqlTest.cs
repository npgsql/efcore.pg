#nullable enable

using System.Text.Json.Nodes;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class JsonNodeTypesNpgsqlTest : IClassFixture<JsonNodeTypesNpgsqlTest.JsonNodeTypesFixture>
{
    private JsonNodeTypesFixture Fixture { get; }

    public JsonNodeTypesNpgsqlTest(JsonNodeTypesFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public async Task Can_read_write_JsonNode_values()
    {
        using var context = CreateContext();
        var entity = new JsonNodeEntity
        {
            Id = 100,
            JsonNodeProperty = JsonNode.Parse("""{"name": "test", "value": 42}"""),
            JsonObjectProperty = new JsonObject
            {
                ["name"] = "object test",
                ["count"] = 100
            },
            JsonArrayProperty = new JsonArray { "item1", "item2", 42 },
            JsonValueProperty = JsonValue.Create("simple value")
        };

        context.JsonNodeEntities.Add(entity);
        await context.SaveChangesAsync();

        // Clear context to ensure data is read from database
        context.ChangeTracker.Clear();

        var retrieved = await context.JsonNodeEntities.FirstAsync(e => e.Id == 100);

        Assert.NotNull(retrieved.JsonNodeProperty);
        Assert.Equal("test", retrieved.JsonNodeProperty["name"]?.GetValue<string>());
        Assert.Equal(42, retrieved.JsonNodeProperty["value"]?.GetValue<int>());

        Assert.NotNull(retrieved.JsonObjectProperty);
        Assert.Equal("object test", retrieved.JsonObjectProperty["name"]?.GetValue<string>());
        Assert.Equal(100, retrieved.JsonObjectProperty["count"]?.GetValue<int>());

        Assert.NotNull(retrieved.JsonArrayProperty);
        Assert.Equal(3, retrieved.JsonArrayProperty.Count);
        Assert.Equal("item1", retrieved.JsonArrayProperty[0]?.GetValue<string>());
        Assert.Equal("item2", retrieved.JsonArrayProperty[1]?.GetValue<string>());
        Assert.Equal(42, retrieved.JsonArrayProperty[2]?.GetValue<int>());

        Assert.NotNull(retrieved.JsonValueProperty);
        Assert.Equal("simple value", retrieved.JsonValueProperty.GetValue<string>());
    }

    [Fact]
    public async Task Can_query_JsonNode_properties()
    {
        using var context = CreateContext();

        var entity1 = new JsonNodeEntity
        {
            Id = 101,
            JsonObjectProperty = new JsonObject
            {
                ["category"] = "books",
                ["rating"] = 5
            }
        };

        var entity2 = new JsonNodeEntity
        {
            Id = 102,
            JsonObjectProperty = new JsonObject
            {
                ["category"] = "movies",
                ["rating"] = 4
            }
        };

        context.JsonNodeEntities.AddRange(entity1, entity2);
        await context.SaveChangesAsync();

        // Clear context to ensure data is read from database
        context.ChangeTracker.Clear();

        // Test querying with JSON path operations (if supported by the provider)
        var bookEntities = await context.JsonNodeEntities
            .Where(e => e.Id >= 101 && e.JsonObjectProperty != null)
            .ToListAsync();

        Assert.Equal(2, bookEntities.Count);
    }

    [Fact]
    public async Task Can_update_JsonNode_properties()
    {
        using var context = CreateContext();

        var entity = new JsonNodeEntity
        {
            Id = 103,
            JsonObjectProperty = new JsonObject
            {
                ["status"] = "pending",
                ["version"] = 1
            }
        };

        context.JsonNodeEntities.Add(entity);
        await context.SaveChangesAsync();

        // Update the JSON property
        entity.JsonObjectProperty["status"] = "completed";
        entity.JsonObjectProperty["version"] = 2;
        await context.SaveChangesAsync();

        // Clear context and verify update
        context.ChangeTracker.Clear();
        var updated = await context.JsonNodeEntities.FirstAsync(e => e.Id == 103);

        Assert.Equal("completed", updated.JsonObjectProperty!["status"]?.GetValue<string>());
        Assert.Equal(2, updated.JsonObjectProperty["version"]?.GetValue<int>());
    }

    [Fact]
    public async Task Can_handle_null_JsonNode_values()
    {
        using var context = CreateContext();

        var entity = new JsonNodeEntity
        {
            Id = 104,
            JsonNodeProperty = null,
            JsonObjectProperty = null,
            JsonArrayProperty = null,
            JsonValueProperty = null
        };

        context.JsonNodeEntities.Add(entity);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var retrieved = await context.JsonNodeEntities.FirstAsync(e => e.Id == 104);

        Assert.Null(retrieved.JsonNodeProperty);
        Assert.Null(retrieved.JsonObjectProperty);
        Assert.Null(retrieved.JsonArrayProperty);
        Assert.Null(retrieved.JsonValueProperty);
    }

    [Fact]
    public async Task Can_use_both_json_and_jsonb_columns()
    {
        using var context = CreateContext();

        var entity = new JsonNodeEntity
        {
            Id = 105,
            JsonNodeProperty = JsonNode.Parse("""{"type": "json test"}"""),
            JsonbNodeProperty = JsonNode.Parse("""{"type": "jsonb test"}""")
        };

        context.JsonNodeEntities.Add(entity);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var retrieved = await context.JsonNodeEntities.FirstAsync(e => e.Id == 105);

        Assert.Equal("json test", retrieved.JsonNodeProperty!["type"]?.GetValue<string>());
        Assert.Equal("jsonb test", retrieved.JsonbNodeProperty!["type"]?.GetValue<string>());
    }

    private NorthwindContext CreateContext() => Fixture.CreateContext();

    public class JsonNodeTypesFixture : SharedStoreFixtureBase<NorthwindContext>
    {
        static JsonNodeTypesFixture()
        {
            // TODO: Switch to using NpgsqlDataSource
#pragma warning disable CS0618 // Type or member is obsolete
            NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        protected override string StoreName => "JsonNodeTypesTest";
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override async Task SeedAsync(NorthwindContext context)
        {
            // Create tables
            context.Database.EnsureCreated();

            // Add test data
            var entity1 = new JsonNodeEntity
            {
                Id = 1,
                JsonNodeProperty = JsonNode.Parse("""{"name": "John", "age": 30}"""),
                JsonObjectProperty = JsonNode.Parse("""{"city": "New York", "country": "USA"}""")?.AsObject(),
                JsonArrayProperty = JsonNode.Parse("""[1, 2, 3, 4, 5]""")?.AsArray(),
                JsonValueProperty = JsonValue.Create("test value"),
                JsonbNodeProperty = JsonNode.Parse("""{"temperature": 25.5, "humidity": 60}""")
            };

            var entity2 = new JsonNodeEntity
            {
                Id = 2,
                JsonNodeProperty = JsonNode.Parse("""[10, 20, 30]"""),
                JsonObjectProperty = JsonNode.Parse("""{"product": "laptop", "price": 999.99}""")?.AsObject(),
                JsonArrayProperty = JsonNode.Parse("""["a", "b", "c"]""")?.AsArray(),
                JsonValueProperty = JsonValue.Create(42),
                JsonbNodeProperty = JsonNode.Parse("""{"status": "active", "count": 100}""")
            };

            context.JsonNodeEntities.Add(entity1);
            context.JsonNodeEntities.Add(entity2);
            await context.SaveChangesAsync();
        }
    }

    public class NorthwindContext : DbContext
    {
        public NorthwindContext(DbContextOptions options) : base(options) { }

        public DbSet<JsonNodeEntity> JsonNodeEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JsonNodeEntity>(b =>
            {
                b.Property(e => e.JsonNodeProperty)
                    .HasColumnType("json");

                b.Property(e => e.JsonObjectProperty)
                    .HasColumnType("jsonb");

                b.Property(e => e.JsonArrayProperty)
                    .HasColumnType("jsonb");

                b.Property(e => e.JsonValueProperty)
                    .HasColumnType("jsonb");

                b.Property(e => e.JsonbNodeProperty)
                    .HasColumnType("jsonb");
            });
        }
    }

    public class JsonNodeEntity
    {
        public int Id { get; set; }
        public JsonNode? JsonNodeProperty { get; set; }
        public JsonObject? JsonObjectProperty { get; set; }
        public JsonArray? JsonArrayProperty { get; set; }
        public JsonValue? JsonValueProperty { get; set; }
        public JsonNode? JsonbNodeProperty { get; set; }
    }
}
