using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

public class ZonedDateTimeJsonQueryNpgsqlTest(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string StoreName
        => "ZonedDateTimeJsonQueryTest";

    protected override ITestStoreFactory TestStoreFactory
        => new NpgsqlTestStoreFactory(useConnectionString: true);

    [ConditionalFact]
    public async Task Scalar_and_whole_document()
    {
        var contextFactory = await Initialize();
        await using var context = contextFactory.CreateContext();
        var value = Earlier.InUtc();

        Assert.Equal(
            value,
            await context.Set<Container>().Where(e => e.Details.Value == value).Select(e => e.Details.Value).SingleAsync());

        var entity = await context.Set<Container>().AsNoTracking().SingleAsync();
        var expected = Earlier.InZone(DateTimeZoneProviders.Tzdb["Europe/Berlin"]).ToOffsetDateTime().InFixedZone();
        Assert.Equal(expected, entity.Details.Value);
        Assert.Null(entity.Details.NullableValue);

        AssertSql(
            """
@value='2018-10-28T00:30:00 UTC (+00)' (DbType = DateTime)

SELECT CAST(c."Details" ->> 'Value' AS timestamp with time zone)
FROM "Container" AS c
WHERE (CAST(c."Details" ->> 'Value' AS timestamp with time zone)) = @value
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
    public async Task Recordset_and_primitive_collection()
    {
        var contextFactory = await Initialize();
        await using var context = contextFactory.CreateContext();
        var value = Later.InUtc();

        Assert.Equal(
            value,
            await context.Set<Container>().SelectMany(e => e.Items)
                .Where(e => e.Value == value).Select(e => e.Value).SingleAsync());
        Assert.Equal(
            value,
            await context.Set<Container>().SelectMany(e => e.Details.Values).Where(e => e == value).SingleAsync());
        Assert.Equal(value, await context.Set<Container>().Select(e => e.Details.Values[1]).SingleAsync());

        AssertSql(
            """
@value='2018-10-28T01:30:00 UTC (+00)' (DbType = DateTime)

SELECT i."Value"
FROM "Container" AS c
JOIN LATERAL ROWS FROM (jsonb_to_recordset(c."Items") AS ("Value" timestamp with time zone)) WITH ORDINALITY AS i ON TRUE
WHERE i."Value" = @value
LIMIT 2
""",
            //
            """
@value='2018-10-28T01:30:00 UTC (+00)' (DbType = DateTime)

SELECT v.element::timestamptz
FROM "Container" AS c
JOIN LATERAL jsonb_array_elements_text(c."Details" -> 'Values') WITH ORDINALITY AS v(element) ON TRUE
WHERE v.element::timestamptz = @value
LIMIT 2
""",
            //
            """
SELECT CAST(c."Details" #>> '{Values,1}' AS timestamp with time zone)
FROM "Container" AS c
LIMIT 2
""");
    }

    [ConditionalTheory]
    [InlineData("2018-10-28T00:30:00Z")]
    [InlineData("2018-10-28T00:30:00.123456Z")]
    public async Task Existing_UTC_document(string text)
    {
        var contextFactory = await Initialize();
        await using var context = contextFactory.CreateContext();
        var json = $$"""{"Value":"{{text}}","NullableValue":null,"Values":[]}""";
        await context.Database.ExecuteSqlAsync($"""UPDATE "Container" SET "Details" = {json}::jsonb""");
        ListLoggerFactory.Clear();

        var expected = NodaTime.Text.InstantPattern.ExtendedIso.Parse(text).Value.InUtc();
        Assert.Equal(expected, (await context.Set<Container>().AsNoTracking().SingleAsync()).Details.Value);
        Assert.Equal(expected, await context.Set<Container>().Select(e => e.Details.Value).SingleAsync());

        AssertSql(
            """
SELECT c."Id", c."Details", c."Items"
FROM "Container" AS c
LIMIT 2
""",
            //
            """
SELECT CAST(c."Details" ->> 'Value' AS timestamp with time zone)
FROM "Container" AS c
LIMIT 2
""");
    }

    private async Task<ContextFactory<DbContext>> Initialize()
    {
        var contextFactory = await InitializeAsync<DbContext>(
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
                        Items = [new Details { Value = Earlier.InZone(zone) }, new Details { Value = Later.InZone(zone) }]
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
