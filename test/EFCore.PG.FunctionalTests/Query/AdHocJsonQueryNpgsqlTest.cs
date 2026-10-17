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

    [ConditionalTheory]
    [InlineData("jsonb")]
    [InlineData("json")]
    public virtual async Task Byte_array_mapped_to_json_column(string storeType)
    {
        var contextFactory = await InitializeNonSharedTest<ByteArrayJsonDbContext>(
            onModelCreating: mb => mb.Entity<ByteArrayJsonEntity>().Property(e => e.Json).HasColumnType(storeType),
            seed: async context =>
            {
                context.Entities.Add(new ByteArrayJsonEntity { Id = 1, Json = """{"Foo":"Bar"}"""u8.ToArray() });
                await context.SaveChangesAsync();

                // Simulate a JSON document which wasn't written via EF
                await context.Database.ExecuteSqlRawAsync("""INSERT INTO "Entities" ("Id", "Json") VALUES (2, '{{"Foo":"Baz"}}')""");
            });

        using var context = contextFactory.CreateDbContext();

        // The byte array must be written as the raw UTF-8 JSON document, and not as a JSON array of byte values
        Assert.Equal(
            "Bar",
            await context.Database.SqlQueryRaw<string>("""SELECT "Json" ->> 'Foo' AS "Value" FROM "Entities" WHERE "Id" = 1""")
                .SingleAsync());

        var entities = await context.Entities.OrderBy(e => e.Id).ToListAsync();

        Assert.Collection(
            entities,
            e => Assert.Equal("Bar", JsonDocument.Parse(e.Json).RootElement.GetProperty("Foo").GetString()),
            e => Assert.Equal("Baz", JsonDocument.Parse(e.Json).RootElement.GetProperty("Foo").GetString()));

        entities[1].Json = """{"Foo":"Qux"}"""u8.ToArray();
        await context.SaveChangesAsync();

        Assert.Equal(
            "Qux",
            await context.Database.SqlQueryRaw<string>("""SELECT "Json" ->> 'Foo' AS "Value" FROM "Entities" WHERE "Id" = 2""")
                .SingleAsync());
    }

    [ConditionalTheory]
    [InlineData("jsonb")]
    [InlineData("json")]
    public virtual async Task Json_aggregate_over_byte_values(string storeType)
    {
        var contextFactory = await InitializeNonSharedTest<ByteAggregateDbContext>(
            seed: async context =>
            {
                context.Entities.AddRange(
                    new ByteAggregateEntity { Id = 1, GroupId = 1, Value = 2 },
                    new ByteAggregateEntity { Id = 2, GroupId = 1, Value = 1 });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();

        // json_agg/jsonb_agg return a JSON array, so the byte[] result must contain the aggregated values, and not the raw UTF-8 bytes
        // of the JSON array text (as with a byte[] property mapped to json/jsonb above)
        var groups = context.Entities.GroupBy(e => e.GroupId);
        var values = storeType == "jsonb"
            ? await groups.Select(g => EF.Functions.JsonbAgg(g.Select(e => e.Value).OrderBy(v => v))).SingleAsync()
            : await groups.Select(g => EF.Functions.JsonAgg(g.Select(e => e.Value).OrderBy(v => v))).SingleAsync();

        Assert.Equal(new byte[] { 1, 2 }, values);

        AssertSql(
            $"""
SELECT {storeType}_agg(e."Value" ORDER BY e."Value" NULLS FIRST)
FROM "Entities" AS e
GROUP BY e."GroupId"
LIMIT 2
""");
    }

    protected class ByteArrayJsonDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<ByteArrayJsonEntity> Entities { get; set; }
    }

    public class ByteArrayJsonEntity
    {
        public int Id { get; set; }
        public byte[] Json { get; set; }
    }

    protected class ByteAggregateDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<ByteAggregateEntity> Entities { get; set; }
    }

    public class ByteAggregateEntity
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public byte Value { get; set; }
    }

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
