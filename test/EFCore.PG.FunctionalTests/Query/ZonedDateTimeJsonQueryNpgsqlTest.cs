using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

public class ZonedDateTimeJsonQueryNpgsqlTest(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "ZonedDateTimeJsonQueryTest";

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => new NpgsqlTestStoreFactory(useConnectionString: true);

    [ConditionalFact]
    public async Task Scalar_predicate_and_projection()
    {
        var contextFactory = await Initialize();
        await using var context = contextFactory.CreateDbContext();
        var value = Earlier.InUtc();

        var result = await context.Set<Container>()
            .Where(e => e.Details.Value == value)
            .Select(e => e.Details.Value)
            .SingleAsync();

        Assert.Equal(Earlier, result.ToInstant());
        Assert.Null(await context.Set<Container>().Select(e => e.Details.NullableValue).SingleAsync());

        var entity = await context.Set<Container>().SingleAsync();
        Assert.Equal(Earlier.InZone(DateTimeZoneProviders.Tzdb["Europe/Berlin"]), entity.Details.Value);

        AssertSql(
            """
@value='2018-10-28T00:30:00 UTC (+00)' (DbType = DateTime)

SELECT regexp_replace(c."Details" ->> 'Value', ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz
FROM "Container" AS c
WHERE regexp_replace(c."Details" ->> 'Value', ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz = @value
LIMIT 2
""",
            //
            """
SELECT regexp_replace(c."Details" ->> 'NullableValue', ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz
FROM "Container" AS c
LIMIT 2
""",
            //
            """
SELECT c."Id", c."Details", c."Items"
FROM "Container" AS c
LIMIT 2
""");
    }

    [ConditionalFact]
    public async Task Recordset_predicate_and_projection()
    {
        var contextFactory = await Initialize();
        await using var context = contextFactory.CreateDbContext();
        var value = Later.InUtc();

        var result = await context.Set<Container>()
            .SelectMany(e => e.Items)
            .Where(e => e.Value == value && e.NullableValue == null)
            .Select(e => e.Value)
            .SingleAsync();

        Assert.Equal(Later, result.ToInstant());

        AssertSql(
            """
@value='2018-10-28T01:30:00 UTC (+00)' (DbType = DateTime)

SELECT regexp_replace(i."Value", ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz AS "Value"
FROM "Container" AS c
JOIN LATERAL ROWS FROM (jsonb_to_recordset(c."Items") AS (
    "NullableValue" text,
    "Value" text
)) WITH ORDINALITY AS i ON TRUE
WHERE regexp_replace(i."Value", ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz = @value AND i."NullableValue" IS NULL
LIMIT 2
""");
    }

    [ConditionalFact]
    public async Task Primitive_collection_predicate_and_projection()
    {
        var contextFactory = await Initialize();
        await using var context = contextFactory.CreateDbContext();
        var value = Earlier;

        var result = await context.Set<Container>()
            .SelectMany(e => e.Details.Values)
            .Where(e => e.ToInstant() > value)
            .SingleAsync();

        Assert.Equal(Later, result.ToInstant());

        AssertSql(
            """
@value='2018-10-28T00:30:00Z' (DbType = DateTime)

SELECT regexp_replace(v.element, ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz
FROM "Container" AS c
JOIN LATERAL jsonb_array_elements_text(c."Details" -> 'Values') WITH ORDINALITY AS v(element) ON TRUE
WHERE regexp_replace(v.element, ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz > @value
LIMIT 2
""");
    }

    [ConditionalFact]
    public async Task Primitive_collection_contains()
    {
        var contextFactory = await Initialize();
        await using var context = contextFactory.CreateDbContext();
        var value = Earlier.InUtc();

        Assert.Equal(1, await context.Set<Container>().Where(e => e.Details.Values.Contains(value)).CountAsync());

        AssertSql(
            """
@value='2018-10-28T00:30:00 UTC (+00)' (DbType = DateTime)

SELECT count(*)::int
FROM "Container" AS c
WHERE @value IN (
    SELECT regexp_replace(v.element, ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz
    FROM jsonb_array_elements_text(c."Details" -> 'Values') WITH ORDINALITY AS v(element)
)
""");
    }

    [ConditionalFact]
    public async Task Primitive_collection_index()
    {
        var contextFactory = await Initialize();
        await using var context = contextFactory.CreateDbContext();

        var result = await context.Set<Container>().Select(e => e.Details.Values[1]).SingleAsync();

        Assert.Equal(Later, result.ToInstant());

        AssertSql(
            """
SELECT regexp_replace(c."Details" #>> '{Values,1}', ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz
FROM "Container" AS c
LIMIT 2
""");
    }

    [ConditionalFact]
    public async Task Legacy_UTC_scalar_and_recordset()
    {
        var contextFactory = await Initialize();
        await using var context = contextFactory.CreateDbContext();
        const string details = """{"Value":"2018-10-28T00:30:00Z","NullableValue":null,"Values":[]}""";
        const string items = """[{"Value":"2018-10-28T00:30:00Z","NullableValue":null,"Values":[]}]""";
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"""UPDATE "Container" SET "Details" = {details}::jsonb, "Items" = {items}::jsonb""");
        ListLoggerFactory.Clear();

        var value = Earlier.InUtc();
        Assert.Equal(
            value,
            await context.Set<Container>().Where(e => e.Details.Value == value).Select(e => e.Details.Value).SingleAsync());
        Assert.Equal(
            value,
            await context.Set<Container>().SelectMany(e => e.Items).Where(e => e.Value == value).Select(e => e.Value).SingleAsync());

        AssertSql(
            """
@value='2018-10-28T00:30:00 UTC (+00)' (DbType = DateTime)

SELECT regexp_replace(c."Details" ->> 'Value', ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz
FROM "Container" AS c
WHERE regexp_replace(c."Details" ->> 'Value', ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz = @value
LIMIT 2
""",
            //
            """
@value='2018-10-28T00:30:00 UTC (+00)' (DbType = DateTime)

SELECT regexp_replace(i."Value", ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz AS "Value"
FROM "Container" AS c
JOIN LATERAL ROWS FROM (jsonb_to_recordset(c."Items") AS ("Value" text)) WITH ORDINALITY AS i ON TRUE
WHERE regexp_replace(i."Value", ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz = @value
LIMIT 2
""");
    }

    [ConditionalFact]
    public async Task Recordset_distinct_projection()
    {
        var contextFactory = await Initialize();
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Set<Container>().SelectMany(e => e.Items).Select(e => e.Value).Distinct().ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.Contains(results, e => e.ToInstant() == Earlier);
        Assert.Contains(results, e => e.ToInstant() == Later);

        AssertSql(
            """
SELECT DISTINCT regexp_replace(i."Value", ' [^ ]+ \(([^)]+)\)$', '\1')::timestamptz AS "Value"
FROM "Container" AS c
JOIN LATERAL ROWS FROM (jsonb_to_recordset(c."Items") AS ("Value" text)) WITH ORDINALITY AS i ON TRUE
""");
    }

    private async Task<ContextFactory<DbContext>> Initialize()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: modelBuilder => modelBuilder.Entity<Container>(b =>
            {
                b.ComplexProperty(e => e.Details).ToJson();
                b.ComplexCollection(e => e.Items).ToJson();
            }),
            onConfiguring: b => new NpgsqlDbContextOptionsBuilder(b).UseNodaTime(),
            addServices: s => s.AddEntityFrameworkNpgsqlNodaTime(),
            seed: async context =>
            {
                var zone = DateTimeZoneProviders.Tzdb["Europe/Berlin"];
                context.Add(
                    new Container
                    {
                        Details = new Details
                        {
                            Value = Earlier.InZone(zone),
                            Values = [Earlier.InZone(zone), Later.InZone(zone)]
                        },
                        Items =
                        [
                            new Details { Value = Earlier.InZone(zone), NullableValue = Earlier.InZone(zone) },
                            new Details { Value = Later.InZone(zone) }
                        ]
                    });
                await context.SaveChangesAsync();
            });

        ListLoggerFactory.Clear();
        return contextFactory;
    }

    private void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);

    private static readonly Instant Earlier = Instant.FromUtc(2018, 10, 28, 0, 30);
    private static readonly Instant Later = Instant.FromUtc(2018, 10, 28, 1, 30);

    public class Container
    {
        public int Id { get; set; }
        public Details Details { get; set; } = null!;
        public List<Details> Items { get; set; } = [];
    }

    public class Details
    {
        public ZonedDateTime Value { get; set; }
        public ZonedDateTime? NullableValue { get; set; }
        public List<ZonedDateTime> Values { get; set; } = [];
    }
}
