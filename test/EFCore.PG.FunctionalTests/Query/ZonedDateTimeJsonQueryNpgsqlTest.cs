using NodaTime;
using NodaTime.Text;
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

SELECT CASE
    WHEN regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND (c."Details" ->> 'Value') ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END
FROM "Container" AS c
WHERE CASE
    WHEN regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND (c."Details" ->> 'Value') ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END = @value
LIMIT 2
""",
            //
            """
SELECT CASE
    WHEN regexp_replace(c."Details" ->> 'NullableValue', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND (c."Details" ->> 'NullableValue') ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(c."Details" ->> 'NullableValue', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(c."Details" ->> 'NullableValue', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END
FROM "Container" AS c
LIMIT 2
""",
            //
            """
SELECT c."Id", c."NativeValue", c."Details", c."Items"
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

SELECT CASE
    WHEN regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND i."Value" ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END AS "Value"
FROM "Container" AS c
JOIN LATERAL ROWS FROM (jsonb_to_recordset(c."Items") AS (
    "NullableValue" text,
    "Value" text
)) WITH ORDINALITY AS i ON TRUE
WHERE CASE
    WHEN regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND i."Value" ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END = @value AND CASE
    WHEN regexp_replace(i."NullableValue", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND i."NullableValue" ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(i."NullableValue", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(i."NullableValue", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END IS NULL
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

SELECT CASE
    WHEN regexp_replace(v.element, '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND v.element ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(v.element, '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(v.element, '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END
FROM "Container" AS c
JOIN LATERAL jsonb_array_elements_text(c."Details" -> 'Values') WITH ORDINALITY AS v(element) ON TRUE
WHERE CASE
    WHEN regexp_replace(v.element, '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND v.element ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(v.element, '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(v.element, '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END > @value
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
    SELECT CASE
        WHEN regexp_replace(v.element, '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND v.element ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(v.element, '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
        ELSE regexp_replace(v.element, '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
    END
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
SELECT CASE
    WHEN regexp_replace(c."Details" #>> '{Values,1}', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND (c."Details" #>> '{Values,1}') ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(c."Details" #>> '{Values,1}', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(c."Details" #>> '{Values,1}', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END
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
        await context.Database.ExecuteSqlAsync(
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

SELECT CASE
    WHEN regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND (c."Details" ->> 'Value') ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END
FROM "Container" AS c
WHERE CASE
    WHEN regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND (c."Details" ->> 'Value') ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(c."Details" ->> 'Value', '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END = @value
LIMIT 2
""",
            //
            """
@value='2018-10-28T00:30:00 UTC (+00)' (DbType = DateTime)

SELECT CASE
    WHEN regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND i."Value" ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END AS "Value"
FROM "Container" AS c
JOIN LATERAL ROWS FROM (jsonb_to_recordset(c."Items") AS ("Value" text)) WITH ORDINALITY AS i ON TRUE
WHERE CASE
    WHEN regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND i."Value" ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END = @value
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
SELECT DISTINCT CASE
    WHEN regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz < TIMESTAMPTZ '2000-01-01T00:00:00Z' AND i."Value" ~ '(?p)\.\d{6}\d*[1-9]\d* ' THEN regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz + INTERVAL '00:00:00.000001'
    ELSE regexp_replace(i."Value", '(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$', '\1\2', 'g')::timestamptz
END AS "Value"
FROM "Container" AS c
JOIN LATERAL ROWS FROM (jsonb_to_recordset(c."Items") AS ("Value" text)) WITH ORDINALITY AS i ON TRUE
""");
    }

    [ConditionalTheory]
    [InlineData("2018-04-20T10:31:33.123456789Z")]
    [InlineData("2018-04-20T10:31:33.1234565Z")]
    [InlineData("1998-04-20T10:31:33.123456001Z")]
    [InlineData("1998-04-20T10:31:33.123456789Z")]
    [InlineData("1998-04-20T10:31:33.12345601Z")]
    [InlineData("2000-01-01T00:00:00.000000001Z")]
    [InlineData("1999-12-31T23:59:59.999999999Z")]
    [InlineData("1999-12-31T23:59:59.000000001Z")]
    [InlineData("1998-04-20T10:31:33.123456Z")]
    public async Task Sub_microsecond_predicates_and_projections(string text)
    {
        var instant = InstantPattern.ExtendedIso.Parse(text).GetValueOrThrow();
        var contextFactory = await Initialize(instant);
        await using var context = contextFactory.CreateDbContext();
        var value = instant.InUtc();
        var expected = await context.Database.SqlQuery<ZonedDateTime>($"SELECT {value} AS \"Value\"").SingleAsync();

        Assert.Equal(expected, await context.Set<Container>()
            .Where(e => e.Details.Value == value).Select(e => e.Details.Value).SingleAsync());
        Assert.Equal(2, await context.Set<Container>().SelectMany(e => e.Items).CountAsync(e => e.Value == value));
        Assert.All(
            await context.Set<Container>().SelectMany(e => e.Items).Select(e => e.Value).ToListAsync(),
            v => Assert.Equal(expected, v));
        Assert.Equal(
            expected,
            await context.Set<Container>().SelectMany(e => e.Items)
                .Where(e => e.NullableValue != null).Select(e => e.NullableValue).SingleAsync());
        Assert.Equal(1, await context.Set<Container>().CountAsync(e => e.Details.Values.Contains(value)));
        Assert.Equal(expected, await context.Set<Container>().Select(e => e.Details.Values[0]).SingleAsync());
        Assert.All(
            await context.Set<Container>().SelectMany(e => e.Details.Values).ToListAsync(),
            v => Assert.Equal(expected, v));

        var entity = await context.Set<Container>().AsNoTracking().SingleAsync();
        Assert.Equal(instant.InZone(DateTimeZoneProviders.Tzdb["Europe/Berlin"]), entity.Details.Value);
        Assert.All(entity.Details.Values, v => Assert.Equal(instant, v.ToInstant()));
    }

    [ConditionalTheory]
    [InlineData("1999-12-31T23:59:59.123456789Z", 0)]
    [InlineData("1999-12-31T23:59:59.123456789Z", 3)]
    [InlineData("1999-12-31T23:59:59.123456789Z", 6)]
    [InlineData("1999-12-31T23:59:59.999999999Z", 3)]
    [InlineData("1999-12-31T23:59:59.123456Z", 3)]
    [InlineData("2018-04-20T10:31:33.123456789Z", 3)]
    public async Task Precision_matches_native_column(string text, int precision)
    {
        var instant = InstantPattern.ExtendedIso.Parse(text).GetValueOrThrow();
        var contextFactory = await Initialize(instant, precision);
        await using var context = contextFactory.CreateDbContext();
        var expected = await context.Set<Container>().Select(e => e.NativeValue).SingleAsync();

        Assert.Equal(expected, await context.Set<Container>().Select(e => e.Details.Value).SingleAsync());
        Assert.All(
            await context.Set<Container>().SelectMany(e => e.Items).Select(e => e.Value).ToListAsync(),
            v => Assert.Equal(expected, v));
        Assert.Equal(
            expected,
            await context.Set<Container>().SelectMany(e => e.Items)
                .Where(e => e.NullableValue != null).Select(e => e.NullableValue).SingleAsync());
        Assert.Equal(expected, await context.Set<Container>().Select(e => e.Details.Values[0]).SingleAsync());
        Assert.All(
            await context.Set<Container>().SelectMany(e => e.Details.Values).ToListAsync(),
            v => Assert.Equal(expected, v));
        Assert.Equal(1, await context.Set<Container>().CountAsync(e => e.Details.Value == expected));
        Assert.Equal(2, await context.Set<Container>().SelectMany(e => e.Items).CountAsync(e => e.Value == expected));
        Assert.Equal(1, await context.Set<Container>().CountAsync(e => e.Details.Values.Contains(expected)));

        var entity = await context.Set<Container>().AsNoTracking().SingleAsync();
        Assert.Equal(instant, entity.Details.Value.ToInstant());
        Assert.All(entity.Details.Values, v => Assert.Equal(instant, v.ToInstant()));
    }

    private async Task<ContextFactory<DbContext>> Initialize(Instant? instant = null, int? precision = null)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: modelBuilder => modelBuilder.Entity<Container>(b =>
            {
                var details = b.ComplexProperty(e => e.Details).ToJson();
                var items = b.ComplexCollection(e => e.Items).ToJson();
                if (precision is { } p)
                {
                    b.Property(e => e.NativeValue).HasPrecision(p);
                    details.Property(e => e.Value).HasPrecision(p);
                    details.Property(e => e.NullableValue).HasPrecision(p);
                    details.PrimitiveCollection(e => e.Values).ElementType().HasPrecision(p);
                    items.Property(e => e.Value).Metadata.SetPrecision(p);
                    items.Property(e => e.NullableValue).Metadata.SetPrecision(p);
                    items.PrimitiveCollection(e => e.Values).ElementType().HasPrecision(p);
                }
            }),
            onConfiguring: b => new NpgsqlDbContextOptionsBuilder(b).UseNodaTime(),
            addServices: s => s.AddEntityFrameworkNpgsqlNodaTime(),
            seed: async context =>
            {
                var zone = DateTimeZoneProviders.Tzdb["Europe/Berlin"];
                var earlier = (instant ?? Earlier).InZone(zone);
                var later = (instant ?? Later).InZone(zone);
                context.Add(
                    new Container
                    {
                        NativeValue = earlier.ToInstant().InUtc(),
                        Details = new Details
                        {
                            Value = earlier,
                            Values = [earlier, later]
                        },
                        Items =
                        [
                            new Details { Value = earlier, NullableValue = earlier },
                            new Details { Value = later }
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
        public ZonedDateTime NativeValue { get; set; }
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
