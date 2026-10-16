#pragma warning disable EF8001 // ToJson on owned entities is obsolete

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonDbFunctionsTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "JsonDbFunctionsTest";

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task JsonContains(bool owned)
    {
        var contextFactory = await Initialize(owned);
        await using var context = contextFactory.CreateDbContext();
        var json = """{"Value": 1}""";

        var result = await context.Entities.Where(
                e => EF.Functions.JsonContains(e.Document, """{"Name": "Joe"}""")
                    && EF.Functions.JsonContains(e.Document.Details, json)
                    && EF.Functions.JsonContains(e.Document.Items[0], json)
                    && EF.Functions.JsonContains(e.Document.Details, e.Document.Items[0])
                    && EF.Functions.JsonContains(e.Document.Items, """[{"Value": 1}]"""))
            .Select(e => e.Id).ToListAsync();

        Assert.Equal([1], result);
        AssertSql(
            """
@json='{"Value": 1}' (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE e."Document" @> '{"Name": "Joe"}' AND (e."Document" -> 'Details') @> @json AND (e."Document" #> '{Items,0}') @> @json AND (e."Document" -> 'Details') @> (e."Document" #> '{Items,0}') AND (e."Document" -> 'Items') @> '[{"Value": 1}]'
""");
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task JsonContained(bool owned)
    {
        var contextFactory = await Initialize(owned);
        await using var context = contextFactory.CreateDbContext();
        var json = """{"Value": 1}""";

        var result = await context.Entities.Where(
                e => EF.Functions.JsonContained("""{"Name": "Joe"}""", e.Document)
                    && EF.Functions.JsonContained(e.Document.Details, json)
                    && EF.Functions.JsonContained(json, e.Document.Items[0])
                    && EF.Functions.JsonContained(e.Document.Items[0], e.Document.Details))
            .Select(e => e.Id).ToListAsync();

        Assert.Equal([1], result);
        AssertSql(
            """
@json='{"Value": 1}' (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE '{"Name": "Joe"}' <@ e."Document" AND (e."Document" -> 'Details') <@ @json AND @json <@ (e."Document" #> '{Items,0}') AND (e."Document" #> '{Items,0}') <@ (e."Document" -> 'Details')
""");
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task JsonExists(bool owned)
    {
        var contextFactory = await Initialize(owned);
        await using var context = contextFactory.CreateDbContext();

        var result = await context.Entities
            .Where(e => EF.Functions.JsonExists(e.Document, "Details") && EF.Functions.JsonExists(e.Document.Details, "Value"))
            .Select(e => e.Id).ToListAsync();

        Assert.Equal([1, 2], result.Order());
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Document" ? 'Details' AND (e."Document" -> 'Details') ? 'Value'
""");
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task JsonExistAny(bool owned)
    {
        var contextFactory = await Initialize(owned);
        await using var context = contextFactory.CreateDbContext();

        var result = await context.Entities.Where(
                e => EF.Functions.JsonExistAny(e.Document, "Missing", "Details")
                    && EF.Functions.JsonExistAny(e.Document.Details, "Missing", "Value"))
            .Select(e => e.Id).ToListAsync();

        Assert.Equal([1, 2], result.Order());
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Document" ?| ARRAY['Missing','Details']::text[] AND (e."Document" -> 'Details') ?| ARRAY['Missing','Value']::text[]
""");
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task JsonExistAll(bool owned)
    {
        var contextFactory = await Initialize(owned);
        await using var context = contextFactory.CreateDbContext();

        var result = await context.Entities.Where(
                e => EF.Functions.JsonExistAll(e.Document, "Name", "Details")
                    && !EF.Functions.JsonExistAll(e.Document.Details, "Missing", "Value"))
            .Select(e => e.Id).ToListAsync();

        Assert.Equal([1, 2], result.Order());
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Document" ?& ARRAY['Name','Details']::text[] AND NOT ((e."Document" -> 'Details') ?& ARRAY['Missing','Value']::text[])
""");
    }

    [ConditionalTheory]
    [InlineData(false, "jsonb")]
    [InlineData(true, "jsonb")]
    [InlineData(false, "json")]
    [InlineData(true, "json")]
    public async Task JsonTypeof(bool owned, string storeType)
    {
        var contextFactory = await Initialize(owned, storeType);
        await using var context = contextFactory.CreateDbContext();

        var result = await context.Entities.Where(
                e => EF.Functions.JsonTypeof(e.Document) == "object"
                    && EF.Functions.JsonTypeof(e.Document.Details) == "object"
                    && EF.Functions.JsonTypeof(e.Document.Items) == "array"
                    && EF.Functions.JsonTypeof(e.Document.Details.Value) == "number"
                    && EF.Functions.JsonTypeof(e.Document.Items[5]) == null)
            .Select(e => e.Id).ToListAsync();

        Assert.Equal([1, 2], result.Order());
        AssertSql(
            $$"""
SELECT e."Id"
FROM "Entities" AS e
WHERE {{storeType}}_typeof(e."Document") = 'object' AND {{storeType}}_typeof(e."Document" -> 'Details') = 'object' AND {{storeType}}_typeof(e."Document" -> 'Items') = 'array' AND {{storeType}}_typeof(e."Document" #> '{Details,Value}') = 'number' AND (e."Document" #> '{Items,5}') IS NULL
""");
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Jsonb_only_functions_reject_json(bool owned)
    {
        var contextFactory = await Initialize(owned, "json");
        await using var context = contextFactory.CreateDbContext();

        Expression<Func<JsonEntity, bool>>[] predicates =
        [
            e => EF.Functions.JsonContains(e.Document, "{}"),
            e => EF.Functions.JsonContained("{}", e.Document.Details),
            e => EF.Functions.JsonExists(e.Document, "Name"),
            e => EF.Functions.JsonExistAny(e.Document.Details, "Value"),
            e => EF.Functions.JsonExistAll(e.Document, "Name")
        ];

        foreach (var predicate in predicates)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.Entities.Where(predicate).ToListAsync());
            Assert.Equal("JSON methods on EF.Functions only support the jsonb type, not json.", exception.Message);
        }

        AssertSql();
    }

    private Task<ContextFactory<JsonFunctionsContext>> Initialize(bool owned, string storeType = "jsonb")
        => InitializeNonSharedTest<JsonFunctionsContext>(
            onModelCreating: modelBuilder =>
            {
                if (owned)
                {
                    modelBuilder.Entity<JsonEntity>().OwnsOne(
                        e => e.Document, b =>
                        {
                            b.ToJson().HasColumnType(storeType);
                            b.OwnsOne(d => d.Details);
                            b.OwnsMany(d => d.Items);
                        });
                }
                else
                {
                    modelBuilder.Entity<JsonEntity>().ComplexProperty(
                        e => e.Document, b =>
                        {
                            b.ToJson().HasColumnType(storeType);
                            b.ComplexProperty(d => d.Details);
                            b.ComplexCollection(d => d.Items);
                        });
                }
            },
            onConfiguring: options => options.ConfigureWarnings(w => w.Ignore(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning)),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new JsonEntity
                    {
                        Id = 1,
                        Document = new Document { Name = "Joe", Details = new Details { Value = 1 }, Items = [new Details { Value = 1 }] }
                    },
                    new JsonEntity
                    {
                        Id = 2,
                        Document = new Document { Name = "Moe", Details = new Details { Value = 2 }, Items = [new Details { Value = 2 }] }
                    });
                await context.SaveChangesAsync();
            });

    private class JsonFunctionsContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<JsonEntity> Entities => Set<JsonEntity>();
    }

    private class JsonEntity
    {
        public int Id { get; set; }
        public required Document Document { get; set; }
    }

    private class Document
    {
        public required string Name { get; set; }
        public required Details Details { get; set; }
        public required List<Details> Items { get; set; }
    }

    private class Details
    {
        public int Value { get; set; }
    }

    private void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);
}
