using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class ZonedDateTimeJsonQueryNpgsqlTest
    : IClassFixture<ZonedDateTimeJsonQueryNpgsqlTest.ZonedDateTimeJsonQueryFixture>
{
    private readonly ZonedDateTimeJsonQueryFixture _fixture;

    public ZonedDateTimeJsonQueryNpgsqlTest(ZonedDateTimeJsonQueryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        _fixture.TestSqlLoggerFactory.Clear();
        _fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public async Task Scalar_and_whole_document()
    {
        await using var context = _fixture.CreateContext();
        var value = Earlier.InUtc();

        Assert.Equal(
            value,
            await context.Set<Container>().Where(e => e.Details.Value == value).Select(e => e.Details.Value).SingleAsync());

        var entity = await context.Set<Container>().AsNoTracking().SingleAsync();
        var expected = Earlier.InZone(DateTimeZoneProviders.Tzdb["Europe/Berlin"]).ToOffsetDateTime().InFixedZone();
        var later = Later.InZone(DateTimeZoneProviders.Tzdb["Europe/Berlin"]).ToOffsetDateTime().InFixedZone();
        Assert.Equal(expected, entity.Details.Value);
        Assert.Null(entity.Details.NullableValue);
        Assert.Equal([expected, later], entity.Details.Values);
        Assert.Equal([expected, later], entity.Items.Select(e => e.Value));

        AssertSql(
            """
@__value_0='2018-10-28T00:30:00 UTC (+00)' (DbType = DateTime)

SELECT CAST(c."Details" ->> 'Value' AS timestamp with time zone)
FROM "Container" AS c
WHERE (CAST(c."Details" ->> 'Value' AS timestamp with time zone)) = @__value_0
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
    public async Task Recordset()
    {
        await using var context = _fixture.CreateContext();
        var value = Later.InUtc();

        Assert.Equal(
            value,
            await context.Set<Container>().SelectMany(e => e.Items)
                .Where(e => e.Value == value).Select(e => e.Value).SingleAsync());
        AssertSql(
            """
@__value_0='2018-10-28T01:30:00 UTC (+00)' (DbType = DateTime)

SELECT i."Value"
FROM "Container" AS c
JOIN LATERAL ROWS FROM (jsonb_to_recordset(c."Items") AS (
    "NullableValue" timestamp with time zone,
    "Value" timestamp with time zone,
    "Values" timestamp with time zone[]
)) WITH ORDINALITY AS i ON TRUE
WHERE i."Value" = @__value_0
LIMIT 2
""");
    }

    [ConditionalTheory]
    [InlineData("2018-10-28T00:30:00Z")]
    [InlineData("2018-10-28T00:30:00.123456Z")]
    public async Task Existing_UTC_document(string text)
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var json = $$"""{"Value":"{{text}}","NullableValue":null,"Values":[]}""";
        await context.Database.ExecuteSqlInterpolatedAsync($"""UPDATE "Container" SET "Details" = {json}::jsonb""");
        _fixture.TestSqlLoggerFactory.Clear();

        var expected = InstantPattern.ExtendedIso.Parse(text).Value.InUtc();
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

    private void AssertSql(params string[] expected)
        => _fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private static readonly Instant Earlier = Instant.FromUtc(2018, 10, 28, 0, 30);
    private static readonly Instant Later = Instant.FromUtc(2018, 10, 28, 1, 30);

    public class Context(DbContextOptions options) : PoolableDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Container>(b =>
            {
                b.OwnsOne(e => e.Details).ToJson();
                b.OwnsMany(e => e.Items).ToJson();
            });
    }

    public class ZonedDateTimeJsonQueryFixture : SharedStoreFixtureBase<Context>
    {
#pragma warning disable CS0618 // GlobalTypeMapper is obsolete
        public ZonedDateTimeJsonQueryFixture()
            => NpgsqlConnection.GlobalTypeMapper.UseNodaTime();
#pragma warning restore CS0618

        protected override string StoreName
            => "ZonedDateTimeJsonQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection).AddEntityFrameworkNpgsqlNodaTime();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);
            new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseNodaTime();
            return optionsBuilder;
        }

        protected override void Seed(Context context)
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
            context.SaveChanges();
        }
    }

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
