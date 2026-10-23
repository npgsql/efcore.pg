using System.Collections.Immutable;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class HstoreTranslationsTest : IClassFixture<HstoreTranslationsTest.HstoreQueryFixture>
{
    private HstoreQueryFixture Fixture { get; }

    public HstoreTranslationsTest(HstoreQueryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags["Variant"] == "SD").Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" -> 'Variant' = 'SD'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_parameter_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var key = "Variant";
        var query = context.Entities.Where(e => e.Tags[EF.Parameter(key)] == "SD").Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1], results);
        AssertSql(
            """
@key='Variant'

SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" -> @key = 'SD'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_column_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags[e.Key] == "SD").Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" -> e."Key" = 'SD'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_computed_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.Tags[e.Key + "Suffix"] == "value")
            .Select(e => e.Tags[e.Key + "Suffix"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(["value"], results);
        AssertSql(
            """
SELECT e."Tags" -> (e."Key" || 'Suffix')
FROM "Entities" AS e
WHERE e."Tags" -> (e."Key" || 'Suffix') = 'value'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_projection(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.Tags["Variant"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", "HD", null, null }, results);
        AssertSql(
            """
SELECT e."Tags" -> 'Variant'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.NullableTags!["Variant"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", null, null, null }, results);
        AssertSql(
            """
SELECT e."NullableTags" -> 'Variant'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_string_length(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.Tags["Variant"]!.Length == 2)
            .Select(e => e.Tags["Variant"]!.Length);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([2, 2], results);
        AssertSql(
            """
SELECT length(e."Tags" -> 'Variant')::int
FROM "Entities" AS e
WHERE length(e."Tags" -> 'Variant')::int = 2
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_string_methods(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags["Variant"]!.StartsWith("S")).Select(e => e.Tags["Variant"]!.ToUpper());
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(["SD"], results);
        AssertSql(
            """
SELECT upper(e."Tags" -> 'Variant')
FROM "Entities" AS e
WHERE e."Tags" -> 'Variant' LIKE 'S%'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_null(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags["Variant"] == null).OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" -> 'Variant' IS NULL
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_inequality(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags["Variant"] != "SD").OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([2, 3, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" -> 'Variant' <> 'SD' OR e."Tags" -> 'Variant' IS NULL
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.ImmutableTags[e.Key] == "SD").Select(e => e.ImmutableTags["Variant"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(["SD"], results);
        AssertSql(
            """
SELECT e."ImmutableTags" -> 'Variant'
FROM "Entities" AS e
WHERE e."ImmutableTags" -> e."Key" = 'SD'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable_parameter_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var key = "Variant";
        var query = context.Entities.Where(e => e.ImmutableTags[EF.Parameter(key)] == "SD").Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1], results);
        AssertSql(
            """
@key='Variant'

SELECT e."Id"
FROM "Entities" AS e
WHERE e."ImmutableTags" -> @key = 'SD'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable_computed_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.ImmutableTags[e.Key + "Suffix"]!.StartsWith("val"))
            .Select(e => e.ImmutableTags[e.Key + "Suffix"]!.Length);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([5], results);
        AssertSql(
            """
SELECT length(e."ImmutableTags" -> (e."Key" || 'Suffix'))::int
FROM "Entities" AS e
WHERE e."ImmutableTags" -> (e."Key" || 'Suffix') LIKE 'val%'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable_projection(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.ImmutableTags["Variant"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", "HD", null, null }, results);
        AssertSql(
            """
SELECT e."ImmutableTags" -> 'Variant'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.NullableImmutableTags!["Variant"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", null, null, null }, results);
        AssertSql(
            """
SELECT e."NullableImmutableTags" -> 'Variant'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable_inequality(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.ImmutableTags["Variant"] != "SD").OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([2, 3, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."ImmutableTags" -> 'Variant' <> 'SD' OR e."ImmutableTags" -> 'Variant' IS NULL
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_ordering(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Tags["Variant"]).ThenBy(e => e.ImmutableTags["Variant"]).ThenBy(e => e.Id)
            .Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3, 4, 2, 1], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
ORDER BY e."Tags" -> 'Variant' NULLS FIRST, e."ImmutableTags" -> 'Variant' NULLS FIRST, e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Mutable = e.Tags.ContainsKey("Variant"), Immutable = e.ImmutableTags.ContainsKey("Variant") });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([true, true, false, true], results.Select(e => e.Mutable));
        Assert.Equal([true, true, false, true], results.Select(e => e.Immutable));
        AssertSql(
            """
SELECT e."Tags" ? 'Variant' AS "Mutable", e."ImmutableTags" ? 'Variant' AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey_parameter_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var key = "Variant";
        var query = context.Entities
            .Where(e => e.Tags.ContainsKey(EF.Parameter(key)) && e.ImmutableTags.ContainsKey(EF.Parameter(key)))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2, 4], results);
        AssertSql(
            """
@key='Variant'

SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" ? @key AND e."ImmutableTags" ? @key
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey_column_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.Tags.ContainsKey(e.Key) && e.ImmutableTags.ContainsKey(e.Key))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" ? e."Key" AND e."ImmutableTags" ? e."Key"
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey_computed_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.Tags.ContainsKey(e.Key + "Suffix") && e.ImmutableTags.ContainsKey(e.Key + "Suffix"))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" ? (e."Key" || 'Suffix') AND e."ImmutableTags" ? (e."Key" || 'Suffix')
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey_negated(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => !e.Tags.ContainsKey("Variant") && !e.ImmutableTags.ContainsKey("Variant"))
            .Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE NOT (e."Tags" ? 'Variant') AND NOT (e."ImmutableTags" ? 'Variant')
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Mutable = (bool?)e.NullableTags!.ContainsKey("Variant"),
                Immutable = (bool?)e.NullableImmutableTags!.ContainsKey("Variant")
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new bool?[] { true, null, false, true }, results.Select(e => e.Mutable));
        Assert.Equal(new bool?[] { true, null, false, true }, results.Select(e => e.Immutable));
        AssertSql(
            """
SELECT e."NullableTags" ? 'Variant' AS "Mutable", e."NullableImmutableTags" ? 'Variant' AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Count(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags.Count == 2 && e.ImmutableTags.Count == 2).OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE cardinality(akeys(e."Tags")) = 2 AND cardinality(akeys(e."ImmutableTags")) = 2
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Count_projection(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => new { Mutable = e.Tags.Count, Immutable = e.ImmutableTags.Count });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([2, 2, 0, 1], results.Select(e => e.Mutable));
        Assert.Equal([2, 2, 0, 1], results.Select(e => e.Immutable));
        AssertSql(
            """
SELECT cardinality(akeys(e."Tags")) AS "Mutable", cardinality(akeys(e."ImmutableTags")) AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Count_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Mutable = (int?)e.NullableTags!.Count, Immutable = (int?)e.NullableImmutableTags!.Count });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new int?[] { 1, null, 0, 1 }, results.Select(e => e.Mutable));
        Assert.Equal(new int?[] { 1, null, 0, 1 }, results.Select(e => e.Immutable));
        AssertSql(
            """
SELECT cardinality(akeys(e."NullableTags")) AS "Mutable", cardinality(akeys(e."NullableImmutableTags")) AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsEmpty(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.ImmutableTags.IsEmpty).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE cardinality(akeys(e."ImmutableTags")) = 0
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsEmpty_negated(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => !e.ImmutableTags.IsEmpty).OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE cardinality(akeys(e."ImmutableTags")) <> 0
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsEmpty_projection(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.ImmutableTags.IsEmpty);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([false, false, true, false], results);
        AssertSql(
            """
SELECT cardinality(akeys(e."ImmutableTags")) = 0
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsEmpty_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.NullableImmutableTags!.IsEmpty).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE cardinality(akeys(e."NullableImmutableTags")) = 0
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class HstoreQueryContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }
    }

    public class Entity
    {
        public int Id { get; set; }
        public string Key { get; set; } = "Variant";
        public Dictionary<string, string?> Tags { get; set; } = new();
        public Dictionary<string, string?>? NullableTags { get; set; }
        public ImmutableDictionary<string, string?> ImmutableTags { get; set; } = ImmutableDictionary<string, string?>.Empty;
        public ImmutableDictionary<string, string?>? NullableImmutableTags { get; set; }
    }

    public class HstoreQueryFixture : SharedStoreFixtureBase<HstoreQueryContext>
    {
        protected override string StoreName
            => "HstoreQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Entity>().Property(e => e.Tags).HasColumnType("hstore");
            modelBuilder.Entity<Entity>().Property(e => e.NullableTags).HasColumnType("hstore");
            modelBuilder.Entity<Entity>().Property(e => e.ImmutableTags).HasColumnType("hstore");
            modelBuilder.Entity<Entity>().Property(e => e.NullableImmutableTags).HasColumnType("hstore");
        }

        protected override async Task SeedAsync(HstoreQueryContext context)
        {
            Entity[] entities =
            [
                new Entity
                {
                    Id = 1,
                    Tags = new() { ["Variant"] = "SD", ["VariantSuffix"] = "value" },
                    NullableTags = new() { ["Variant"] = "SD" }
                },
                new Entity { Id = 2, Tags = new() { ["Variant"] = "HD", ["VariantSuffix"] = "other" } },
                new Entity { Id = 3, NullableTags = new() },
                new Entity { Id = 4, Tags = new() { ["Variant"] = null }, NullableTags = new() { ["Variant"] = null } }
            ];

            foreach (var entity in entities)
            {
                entity.ImmutableTags = entity.Tags.ToImmutableDictionary();
                entity.NullableImmutableTags = entity.NullableTags?.ToImmutableDictionary();
            }

            context.Entities.AddRange(entities);
            await context.SaveChangesAsync();
        }
    }
}
