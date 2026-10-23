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
        }

        protected override async Task SeedAsync(HstoreQueryContext context)
        {
            context.Entities.AddRange(
                new Entity
                {
                    Id = 1,
                    Tags = new() { ["Variant"] = "SD", ["VariantSuffix"] = "value" },
                    NullableTags = new() { ["Variant"] = "SD" }
                },
                new Entity { Id = 2, Tags = new() { ["Variant"] = "HD", ["VariantSuffix"] = "other" } },
                new Entity { Id = 3, NullableTags = new() },
                new Entity { Id = 4, Tags = new() { ["Variant"] = null }, NullableTags = new() { ["Variant"] = null } });
            await context.SaveChangesAsync();
        }
    }
}
